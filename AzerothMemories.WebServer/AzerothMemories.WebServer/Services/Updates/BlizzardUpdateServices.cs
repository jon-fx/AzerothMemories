namespace AzerothMemories.WebServer.Services.Updates;

[RegisterComputeService]
public class BlizzardUpdateServices : DbServiceBase<AppDbContext>
{
    private readonly CommonServices _commonServices;

    public BlizzardUpdateServices(IServiceProvider services, CommonServices commonServices) : base(services)
    {
        _commonServices = commonServices;
    }

    [CommandHandler]
    public virtual async Task<HttpStatusCode> UpdateAccount(Updates_UpdateAccountCommand command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Updates_UpdateAccountInvalidate>();
            if (invRecord != null)
            {
                _ = _commonServices.AccountServices.DependsOnAccountRecord(invRecord.AccountId);
                _ = _commonServices.AccountServices.DependsOnAccountAchievements(invRecord.AccountId);
                _ = _commonServices.CharacterServices.TryGetAllAccountCharacters(invRecord.AccountId);

                foreach (var characterId in invRecord.CharacterIds)
                {
                    _ = _commonServices.CharacterServices.DependsOnCharacterRecord(characterId);
                }
            }

            return default;
        }

        await using var database = await CreateCommandDbContext(cancellationToken);
        var record = await database.Accounts.FirstOrDefaultAsync(x => x.Id == command.AccountId, cancellationToken);
        if (record == null)
        {
            return default;
        }

        if (string.IsNullOrWhiteSpace(record.BattleNetToken) || SystemClock.Instance.GetCurrentInstant() >= record.BattleNetTokenExpiresAt.GetValueOrDefault(Instant.FromUnixTimeMilliseconds(0)))
        {
            record.UpdateJob = null;
            record.UpdateJobEndTime = SystemClock.Instance.GetCurrentInstant();
            record.UpdateJobLastResult = HttpStatusCode.Forbidden;

            await database.SaveChangesAsync(cancellationToken);

            return record.UpdateJobLastResult;
        }

        using var client = _commonServices.WarcraftClientProvider.Get(record.BlizzardRegionId);
        var characters = await database.Characters.Where(x => x.AccountId == command.AccountId).ToDictionaryAsync(x => x.MoaRef, x => x, cancellationToken);
        var accountSummaryResult = await client.GetAccountProfile(record.BattleNetToken, 0 /*record.BlizzardAccountLastModified*/).ConfigureAwait(false);
        if (accountSummaryResult.IsSuccess)
        {
            var deletedCharactersSets = new Dictionary<string, CharacterRecord>(characters);

            foreach (var account in accountSummaryResult.ResultData.WowAccounts)
            {
                foreach (var accountCharacter in account.Characters)
                {
                    var characterRef = MoaRef.GetCharacterRef(record.BlizzardRegionId, accountCharacter.Realm.Slug, accountCharacter.Name, accountCharacter.Id);
                    if (!characters.TryGetValue(characterRef.Full, out var characterRecord))
                    {
                        characterRecord = await _commonServices.CharacterServices.GetOrCreateCharacterRecord(characterRef.Full);

                        database.Characters.Attach(characterRecord);
                    }

                    if (characterRecord.AccountId.HasValue)
                    {
                    }
                    else
                    {
                        await database.Database.ExecuteSqlRawAsync($"UPDATE \"Characters_Achievements\" SET \"AccountId\" = {record.Id} WHERE \"CharacterId\" = {characterRecord.Id} AND \"AccountId\" IS NULL", cancellationToken);
                    }

                    characterRecord.AccountId = record.Id;
                    characterRecord.MoaRef = characterRef.Full;
                    characterRecord.BlizzardId = accountCharacter.Id;
                    //characterRecord.BlizzardAccountId = account.Id;
                    characterRecord.BlizzardRegionId = characterRef.Region;
                    characterRecord.CharacterStatus = CharacterStatus2.None;
                    characterRecord.RealmId = accountCharacter.Realm.Id;
                    characterRecord.Name = accountCharacter.Name;
                    characterRecord.NameSearchable = DatabaseHelpers.GetSearchableName(accountCharacter.Name);
                    characterRecord.Race = (byte)accountCharacter.PlayableRace.Id;
                    characterRecord.Class = (byte)accountCharacter.PlayableClass.Id;
                    characterRecord.Gender = accountCharacter.Gender.AsGender();
                    characterRecord.Faction = accountCharacter.Faction.AsFaction();
                    characterRecord.Level = (byte)accountCharacter.Level;

                    deletedCharactersSets.Remove(characterRecord.MoaRef);
                }
            }

            foreach (var character in deletedCharactersSets.Values)
            {
                character.CharacterStatus = CharacterStatus2.MaybeDeleted;
            }
        }
        else if (accountSummaryResult.IsNotModified)
        {
        }

        record.UpdateJob = null;
        record.UpdateJobEndTime = SystemClock.Instance.GetCurrentInstant();
        record.UpdateJobLastResult = accountSummaryResult.ResultCode;

        await database.SaveChangesAsync(cancellationToken);

        context.Operation().Items.Set(new Updates_UpdateAccountInvalidate(record.Id, record.FusionId, record.Username, characters.Values.Select(x => x.Id).ToHashSet()));

        return accountSummaryResult.ResultCode;
    }

    [CommandHandler]
    public virtual async Task<HttpStatusCode> UpdateCharacter(Updates_UpdateCharacterCommand command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Character_InvalidateCharacterRecord>();
            if (invRecord != null)
            {
                if (invRecord.CharacterId > 0)
                {
                    _ = _commonServices.CharacterServices.DependsOnCharacterRecord(invRecord.CharacterId);
                    _ = _commonServices.CharacterServices.TryGetCharacterRecord(invRecord.CharacterId);
                }

                if (invRecord.AccountId > 0)
                {
                    _ = _commonServices.AccountServices.DependsOnAccountAchievements(invRecord.AccountId);
                    _ = _commonServices.CharacterServices.TryGetAllAccountCharacters(invRecord.AccountId);
                }
            }

            return default;
        }

        await using var database = await CreateCommandDbContext(cancellationToken);
        var record = await database.Characters.FirstOrDefaultAsync(x => x.Id == command.CharacterId, cancellationToken);
        if (record == null)
        {
            return default;
        }

        var updateResult = await TryCharacterUpdateInternal(command.CharacterId, database, record);

        record.UpdateJob = null;
        record.UpdateJobEndTime = SystemClock.Instance.GetCurrentInstant();
        record.UpdateJobLastResult = updateResult;

        await database.SaveChangesAsync(cancellationToken);

        if (record.AccountId.HasValue)
        {
            await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
            {
                AccountId = record.AccountId.Value,
                Type = AccountHistoryType.CharacterUpdated,
                TargetId = record.Id
            }, cancellationToken);
        }

        context.Operation().Items.Set(new Character_InvalidateCharacterRecord(record.Id, record.AccountId.GetValueOrDefault()));

        return updateResult;
    }

    private async Task<HttpStatusCode> TryCharacterUpdateInternal(long id, AppDbContext database, CharacterRecord record)
    {
        var characterRef = new MoaRef(record.MoaRef);
        using var client = _commonServices.WarcraftClientProvider.Get(record.BlizzardRegionId);
        var characterSummary = await client.GetCharacterProfileSummaryAsync(characterRef.Realm, characterRef.Name, record.BlizzardProfileLastModified).ConfigureAwait(false);
        if (characterSummary.IsSuccess)
        {
            await OnCharacterUpdate(database, record, characterSummary.ResultData, characterSummary.ResultLastModifiedMs).ConfigureAwait(false);
        }
        else if (characterSummary.IsNotModified)
        {
        }
        else
        {
            return characterSummary.ResultCode;
        }

        var characterRenders = await client.GetCharacterRendersAsync(characterRef.Realm, characterRef.Name, record.BlizzardRendersLastModified).ConfigureAwait(false);
        if (characterRenders.IsSuccess)
        {
            await OnCharacterRendersUpdate(database, record, characterRenders.ResultData, characterRenders.ResultLastModifiedMs).ConfigureAwait(false);
        }
        else if (characterRenders.IsNotModified)
        {
        }
        else
        {
            return characterRenders.ResultCode;
        }

        var achievementsSummary = await client.GetCharacterAchievementsSummaryAsync(characterRef.Realm, characterRef.Name, record.BlizzardAchievementsLastModified).ConfigureAwait(false);
        if (achievementsSummary.IsSuccess)
        {
            await OnAchievementsUpdate(database, record, achievementsSummary);
        }
        else if (achievementsSummary.IsNotModified)
        {
        }
        else
        {
            return achievementsSummary.ResultCode;
        }

        return HttpStatusCode.OK;
    }

    private async Task OnCharacterUpdate(AppDbContext database, CharacterRecord record, CharacterProfileSummary character, long lastModifiedTime)
    {
        record.RealmId = character.Realm.Id;
        record.Name = character.Name;
        record.NameSearchable = DatabaseHelpers.GetSearchableName(record.Name);
        record.Class = (byte)character.CharacterClass.Id;
        record.Race = (byte)character.Race.Id;
        record.Level = (byte)character.Level;
        record.Faction = character.Faction.AsFaction();
        record.Gender = character.Gender.AsGender();

        var guildData = character.Guild;
        long newGuildId = 0;
        string newGuildName = null;
        GuildRecord guildRecord = null;

        if (guildData != null)
        {
            newGuildId = guildData.Id;
            newGuildName = guildData.Name;
            var newGuildRef = MoaRef.GetGuildRef(record.BlizzardRegionId, guildData.Realm.Slug, newGuildName, newGuildId).Full;

            guildRecord = await _commonServices.GuildServices.GetOrCreate(newGuildRef);
            newGuildId = guildRecord.BlizzardId;
        }

        record.BlizzardGuildId = newGuildId;
        record.BlizzardGuildName = newGuildName;
        record.GuildRef = guildRecord?.MoaRef;
        record.GuildId = guildRecord?.Id;
        record.BlizzardProfileLastModified = lastModifiedTime;
    }

    private Task OnCharacterRendersUpdate(AppDbContext database, CharacterRecord record, CharacterMediaSummary media, long lastModifiedTime)
    {
        var assets = media.Assets;
        var characterAvatarRender = record.AvatarLink;
        if (assets != null)
        {
            var avatar = assets.FirstOrDefault(x => x.Key == "avatar");
            characterAvatarRender = avatar?.Value.AbsoluteUri;
        }

        record.AvatarLink = characterAvatarRender;
        record.BlizzardRendersLastModified = lastModifiedTime;

        return Task.CompletedTask;
    }

    private async Task OnAchievementsUpdate(AppDbContext database, CharacterRecord record, RequestResult<CharacterAchievementsSummary> achievementsSummary)
    {
        Exceptions.ThrowIf(record.Id == 0);

        var achievementData = achievementsSummary.ResultData;
        var currentAchievements = await database.CharacterAchievements.Where(x => x.CharacterId == record.Id).ToDictionaryAsync(x => x.AchievementId, x => x).ConfigureAwait(false);

        var accountId = record.AccountId;
        foreach (var achievement in achievementData.Achievements)
        {
            var timeStamp = achievement.CompletedTimestamp.GetValueOrDefault(0);
            if (timeStamp <= 0)
            {
                continue;
            }

            if (!currentAchievements.TryGetValue(achievement.Id, out var achievementRecord))
            {
                achievementRecord = new CharacterAchievementRecord
                {
                    AccountId = accountId,
                    CharacterId = record.Id,
                    AchievementId = achievement.Id,
                };

                database.CharacterAchievements.Add(achievementRecord);
            }

            achievementRecord.AchievementTimeStamp = Instant.FromUnixTimeMilliseconds(timeStamp);
            achievementRecord.CompletedByCharacter = achievement.Criteria == null || achievement.Criteria != null && achievement.Criteria.IsCompleted;
        }

        record.AchievementTotalPoints = achievementData.TotalPoints;
        record.AchievementTotalQuantity = achievementData.TotalQuantity;
        record.BlizzardAchievementsLastModified = achievementsSummary.ResultLastModifiedMs;
    }

    [CommandHandler]
    public virtual async Task<HttpStatusCode> UpdateGuild(Updates_UpdateGuildCommand command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Guild_InvalidateGuildRecord>();
            if (invRecord != null)
            {
                _ = _commonServices.GuildServices.DependsOnGuildRecord(invRecord.GuildId);

                foreach (var characterId in invRecord.CharacterIds)
                {
                    _ = _commonServices.CharacterServices.DependsOnCharacterRecord(characterId);
                }
            }

            return default;
        }

        await using var database = await CreateCommandDbContext(cancellationToken);
        var record = await database.Guilds.FirstOrDefaultAsync(x => x.Id == command.GuildId, cancellationToken);
        if (record == null)
        {
            return default;
        }

        var updateResult = await TryUpdateGuildInternal(command.GuildId, database, record);

        record.UpdateJob = null;
        record.UpdateJobEndTime = SystemClock.Instance.GetCurrentInstant();
        record.UpdateJobLastResult = updateResult;

        await database.SaveChangesAsync(cancellationToken);

        var characterQuery = from r in database.Characters
                             where r.GuildId == r.Id
                             select r.Id;

        var characterIds = await characterQuery.ToArrayAsync(cancellationToken);

        context.Operation().Items.Set(new Guild_InvalidateGuildRecord(record.Id, characterIds.ToHashSet()));

        return updateResult;
    }

    private async Task<HttpStatusCode> TryUpdateGuildInternal(long id, AppDbContext database, GuildRecord record)
    {
        var guildRef = new MoaRef(record.MoaRef);
        using var client = _commonServices.WarcraftClientProvider.Get(guildRef.Region);
        var guildSummary = await client.GetGuildProfileSummaryAsync(guildRef.Realm, guildRef.Name, record.BlizzardProfileLastModified).ConfigureAwait(false);
        if (guildSummary.IsSuccess)
        {
            record.BlizzardId = guildSummary.ResultData.Id;
            record.Name = guildSummary.ResultData.Name;
            record.NameSearchable = DatabaseHelpers.GetSearchableName(record.Name);
            record.RealmId = guildSummary.ResultData.Realm.Id;
            record.BlizzardCreatedTimestamp = guildSummary.ResultData.CreatedTimestamp;
            record.MemberCount = guildSummary.ResultData.MemberCount;
            record.AchievementPoints = guildSummary.ResultData.AchievementPoints;
            record.Faction = guildSummary.ResultData.Faction.AsFaction();
            record.BlizzardProfileLastModified = guildSummary.ResultLastModifiedMs;

            Exceptions.ThrowIf(new MoaRef(record.MoaRef).Id != record.BlizzardId);
        }
        else if (guildSummary.IsNotModified)
        {
        }
        else
        {
            return guildSummary.ResultCode;
        }

        Exceptions.ThrowIf(record.Id == 0);

        var guildAchievements = await client.GetGuildAchievementsAsync(guildRef.Realm, guildRef.Name, record.BlizzardAchievementsLastModified).ConfigureAwait(false);
        if (guildAchievements.IsSuccess)
        {
            await OnGuildGuildAchievementsUpdate(database, record, guildAchievements.ResultLastModifiedMs);
        }
        else if (guildAchievements.IsNotModified)
        {
        }
        else
        {
            return guildAchievements.ResultCode;
        }

        var guildRoster = await client.GetGuildRosterAsync(guildRef.Realm, guildRef.Name, record.BlizzardRosterLastModified).ConfigureAwait(false);
        if (guildRoster.IsSuccess)
        {
            foreach (var guildMember in guildRoster.ResultData.Members)
            {
                var characterId = guildMember.Character.Id;
                var characterName = guildMember.Character.Name;
                var characterRealm = guildMember.Character.Realm.Slug;
                var characterRef = MoaRef.GetCharacterRef(record.BlizzardRegionId, characterRealm, characterName, characterId);
                var characterRecord = await _commonServices.CharacterServices.GetOrCreateCharacterRecord(characterRef.Full, BlizzardUpdatePriority.CharacterLow);
                if (characterRecord.BlizzardId != guildMember.Character.Id)
                {
                    throw new NotImplementedException();
                }

                await database.Characters.Where(x => x.Id == characterRecord.Id).UpdateAsync(x => new CharacterRecord
                {
                    GuildId = record.Id,
                    GuildRef = guildRef.Full,
                    BlizzardGuildId = record.BlizzardId,
                    BlizzardGuildName = guildRoster.ResultData.Guild.Name,
                    Name = guildMember.Character.Name,
                    NameSearchable = DatabaseHelpers.GetSearchableName(guildMember.Character.Name),
                    RealmId = guildMember.Character.Realm.Id,
                    Class = (byte)guildMember.Character.PlayableClass.Id,
                    Race = (byte)guildMember.Character.PlayableRace.Id,
                    BlizzardGuildRank = (byte)guildMember.Rank,
                    Level = (byte)guildMember.Character.Level,
                });
            }

            record.BlizzardRosterLastModified = guildRoster.ResultLastModifiedMs;
        }
        else if (guildRoster.IsNotModified)
        {
        }
        else
        {
            return guildRoster.ResultCode;
        }

        //var guildModifiedTime = guildSummary.ResultLastModified.ToUnixTimeMilliseconds();
        //var achievementsModifiedTime = guildAchievements.ResultLastModified.ToUnixTimeMilliseconds();
        //var rosterModifiedTime = guildRoster.ResultLastModified.ToUnixTimeMilliseconds();
        //await grain.SetLastModifiedTimes(new[] { guildModifiedTime, achievementsModifiedTime, rosterModifiedTime });

        return HttpStatusCode.OK;
    }

    private Task OnGuildGuildAchievementsUpdate(AppDbContext database, GuildRecord record, long lastModifiedTime)
    {
        record.BlizzardAchievementsLastModified = lastModifiedTime;

        return Task.CompletedTask;
    }
}