namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class BlizzardCharacterUpdateHandler
{
    private readonly CommonServices _commonServices;

    public BlizzardCharacterUpdateHandler(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    public async Task<HttpStatusCode> TryUpdate(long id, DatabaseConnection database, CharacterRecord record)
    {
        var result = await TryUpdateInternal(id, database, record);

        _commonServices.CharacterServices.OnCharacterUpdate(record);

        return result;
    }

    private async Task<HttpStatusCode> TryUpdateInternal(long id, DatabaseConnection database, CharacterRecord record)
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
            await UpdateAchievementsPosts(database, record, achievementsSummary);
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

    private async Task OnCharacterUpdate(DatabaseConnection database, CharacterRecord record, CharacterProfileSummary character, long lastModifiedTime)
    {
        var query = database.GetUpdateQuery(record, out var changed);
        if (CheckAndChange.Check(ref record.RealmId, character.Realm.Id, ref changed))
        {
            query = query.Set(x => x.RealmId, record.RealmId);
        }

        if (CheckAndChange.Check(ref record.Name, character.Name, ref changed))
        {
            query = query.Set(x => x.Name, record.Name);
        }

        if (CheckAndChange.Check(ref record.NameSearchable, DatabaseHelpers.GetSearchableName(record.Name), ref changed))
        {
            query = query.Set(x => x.NameSearchable, record.NameSearchable);
        }

        if (CheckAndChange.Check(ref record.Class, (byte)character.CharacterClass.Id, ref changed))
        {
            query = query.Set(x => x.Class, record.Class);
        }

        if (CheckAndChange.Check(ref record.Race, (byte)character.Race.Id, ref changed))
        {
            query = query.Set(x => x.Race, record.Race);
        }

        if (CheckAndChange.Check(ref record.Level, (byte)character.Level, ref changed))
        {
            query = query.Set(x => x.Level, record.Level);
        }

        if (CheckAndChange.Check(ref record.Faction, character.Faction.AsFaction(), ref changed))
        {
            query = query.Set(x => x.Faction, record.Faction);
        }

        if (CheckAndChange.Check(ref record.Gender, character.Gender.AsGender(), ref changed))
        {
            query = query.Set(x => x.Gender, record.Gender);
        }

        var guildData = character.Guild;
        long newGuildId = 0;
        string newGuildName = null;
        string newGuildRef = null;

        if (guildData != null)
        {
            newGuildId = guildData.Id;
            newGuildName = guildData.Name;
            newGuildRef = MoaRef.GetGuildRef(record.BlizzardRegionId, guildData.Realm.Slug, guildData.Name, guildData.Id).Full;
        }

        if (CheckAndChange.Check(ref record.BlizzardGuildId, newGuildId, ref changed))
        {
            query = query.Set(x => x.BlizzardGuildId, record.BlizzardGuildId);
        }

        if (CheckAndChange.Check(ref record.BlizzardGuildName, newGuildName, ref changed))
        {
            query = query.Set(x => x.BlizzardGuildName, record.BlizzardGuildName);
        }

        if (CheckAndChange.Check(ref record.GuildRef, newGuildRef, ref changed))
        {
            query = query.Set(x => x.GuildRef, record.GuildRef);
        }

        GuildRecord guildRecord = null;
        if (record.BlizzardGuildId > 0 && record.GuildRef != null)
        {
            guildRecord = await _commonServices.GuildServices.GetOrCreate(record.GuildRef);
            //var guild = GrainFactory.GetGrain<IGuildGrain>(record.GuildRef);
            //var grainRef = this.AsReference<ICharacterGrain>();
            //await guild.OnCharacterUpdate(grainRef, record.GuildId);
        }

        if (CheckAndChange.Check(ref record.GuildId, guildRecord?.Id, ref changed))
        {
            query = query.Set(x => x.GuildId, record.GuildId);
        }

        if (CheckAndChange.Check(ref record.BlizzardProfileLastModified, lastModifiedTime, ref changed))
        {
            query = query.Set(x => x.BlizzardProfileLastModified, record.BlizzardProfileLastModified);
        }

        if (changed)
        {
            await query.UpdateAsync();
        }
    }

    private async Task OnCharacterRendersUpdate(DatabaseConnection database, CharacterRecord record, CharacterMediaSummary media, long lastModifiedTime)
    {
        var assets = media.Assets;
        var characterAvatarRender = record.AvatarLink;
        if (assets != null)
        {
            var avatar = assets.FirstOrDefault(x => x.Key == "avatar");
            characterAvatarRender = avatar?.Value.AbsoluteUri;
        }

        var query = database.GetUpdateQuery(record, out var changed);
        if (CheckAndChange.Check(ref record.AvatarLink, characterAvatarRender, ref changed))
        {
            query = query.Set(x => x.AvatarLink, record.AvatarLink);
        }

        if (CheckAndChange.Check(ref record.BlizzardRendersLastModified, lastModifiedTime, ref changed))
        {
            query = query.Set(x => x.BlizzardRendersLastModified, record.BlizzardRendersLastModified);
        }

        if (changed)
        {
            await query.UpdateAsync();
        }
    }

    private async Task UpdateAchievementsPosts(DatabaseConnection database, CharacterRecord record, RequestResult<CharacterAchievementsSummary> achievementsSummary)
    {
        Exceptions.ThrowIf(record.Id == 0);

        var changed = false;
        var achievementData = achievementsSummary.ResultData;
        var currentAchievements = await database.CharacterAchievements.Where(x => x.CharacterId == record.Id && x.AchievementId > 0).ToDictionaryAsync(x => x.AchievementId, x => x).ConfigureAwait(false);

        var accountId = record.AccountId;
        var newAchievementRecords = new HashSet<CharacterAchievementRecord>();
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

                newAchievementRecords.Add(achievementRecord);
            }

            IUpdatable<CharacterAchievementRecord> updateQuery = null;
            if (achievementRecord.Id > 0)
            {
                updateQuery = database.GetUpdateQuery(achievementRecord, out _);
            }

            if (CheckAndChange.Check(ref achievementRecord.AchievementTimeStamp, timeStamp, ref changed) && updateQuery != null)
            {
                updateQuery = updateQuery.Set(x => x.AchievementTimeStamp, achievementRecord.AchievementTimeStamp);
            }

            if (CheckAndChange.Check(ref achievementRecord.CompletedByCharacter, achievement.Criteria == null || achievement.Criteria != null && achievement.Criteria.IsCompleted, ref changed) && updateQuery != null)
            {
                updateQuery = updateQuery.Set(x => x.CompletedByCharacter, achievementRecord.CompletedByCharacter);
            }

            if (changed && updateQuery != null)
            {
                await updateQuery.UpdateAsync().ConfigureAwait(false);
            }
        }

        if (newAchievementRecords.Count > 0)
        {
            await database.CharacterAchievements.BulkCopyAsync(newAchievementRecords).ConfigureAwait(false);
        }

        //if (updatedAchievementRecords.Count > 0)
        //{
        //    await database.CharacterAchievements.Merge()
        //        .Using(updatedAchievementRecords)
        //        .OnTargetKey()
        //        .InsertWhenNotMatched()
        //        .UpdateWhenMatched()
        //        .MergeAsync()
        //        .ConfigureAwait(false);
        //}

#if DEBUG
        currentAchievements = await database.CharacterAchievements.Where(x => x.CharacterId == record.Id).ToDictionaryAsync(x => x.AchievementId, x => x).ConfigureAwait(false);
#endif

        var query = database.GetUpdateQuery(record, out changed);
        if (CheckAndChange.Check(ref record.AchievementTotalPoints, achievementData.TotalPoints, ref changed))
        {
            query = query.Set(x => x.AchievementTotalPoints, record.AchievementTotalPoints);
        }

        if (CheckAndChange.Check(ref record.AchievementTotalQuantity, achievementData.TotalQuantity, ref changed))
        {
            query = query.Set(x => x.AchievementTotalQuantity, record.AchievementTotalQuantity);
        }

        if (CheckAndChange.Check(ref record.BlizzardAchievementsLastModified, achievementsSummary.ResultLastModifiedMs, ref changed))
        {
            query = query.Set(x => x.BlizzardAchievementsLastModified, record.BlizzardAchievementsLastModified);
        }

        if (changed)
        {
            await query.UpdateAsync();
        }

        //await characterServices.OnAchievementUpdate(database, achievementData.TotalPoints, achievementData.TotalQuantity, achievementsSummary.ResultLastModifiedMs);
    }
}