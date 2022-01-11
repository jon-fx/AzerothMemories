namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class BlizzardGuildUpdateHandler
{
    private readonly CommonServices _commonServices;

    public BlizzardGuildUpdateHandler(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    public async Task<HttpStatusCode> TryUpdate(long id, DatabaseConnection database, GuildRecord record)
    {
        var result = await TryUpdateInternal(id, database, record);

        _commonServices.GuildServices.OnGuildUpdate(record);

        return result;
    }

    private async Task<HttpStatusCode> TryUpdateInternal(long id, DatabaseConnection database, GuildRecord record)
    {
        var guildRef = new MoaRef(record.MoaRef);
        using var client = _commonServices.WarcraftClientProvider.Get(guildRef.Region);
        var guildSummary = await client.GetGuildProfileSummaryAsync(guildRef.Realm, guildRef.Name, record.BlizzardProfileLastModified).ConfigureAwait(false);
        if (guildSummary.IsSuccess)
        {
            await OnGuildUpdate(database, record, guildSummary.ResultData, guildSummary.ResultLastModifiedMs);
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
                await OnGuildUpdateCharacter(database, characterRef, guildRef, record.Id, record.BlizzardId, guildRoster.ResultData.Guild.Name, (byte)guildMember.Rank, guildMember.Character);
            }

            await OnGuildRosterUpdate(database, record, guildRoster.ResultLastModifiedMs);
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

    private async Task OnGuildUpdate(DatabaseConnection database, GuildRecord record, Guild guildSummary, long lastModifiedTime)
    {
        var query = database.GetUpdateQuery(record, out var changed);

        if (CheckAndChange.Check(ref record.BlizzardId, guildSummary.Id, ref changed))
        {
            query = query.Set(x => x.BlizzardId, record.BlizzardId);
        }

        if (CheckAndChange.Check(ref record.Name, guildSummary.Name, ref changed))
        {
            query = query.Set(x => x.Name, record.Name);
        }

        if (CheckAndChange.Check(ref record.NameSearchable, DatabaseHelpers.GetSearchableName(record.Name), ref changed))
        {
            query = query.Set(x => x.NameSearchable, record.NameSearchable);
        }

        if (CheckAndChange.Check(ref record.RealmId, guildSummary.Realm.Id, ref changed))
        {
            query = query.Set(x => x.RealmId, record.RealmId);
        }

        if (CheckAndChange.Check(ref record.BlizzardCreatedTimestamp, guildSummary.CreatedTimestamp, ref changed))
        {
            query = query.Set(x => x.BlizzardCreatedTimestamp, record.BlizzardCreatedTimestamp);
        }

        if (CheckAndChange.Check(ref record.MemberCount, guildSummary.MemberCount, ref changed))
        {
            query = query.Set(x => x.MemberCount, record.MemberCount);
        }

        if (CheckAndChange.Check(ref record.AchievementPoints, guildSummary.AchievementPoints, ref changed))
        {
            query = query.Set(x => x.AchievementPoints, record.AchievementPoints);
        }

        if (CheckAndChange.Check(ref record.Faction, guildSummary.Faction.AsFaction(), ref changed))
        {
            query = query.Set(x => x.Faction, record.Faction);
        }

        if (CheckAndChange.Check(ref record.BlizzardProfileLastModified, lastModifiedTime, ref changed))
        {
            query = query.Set(x => x.BlizzardProfileLastModified, record.BlizzardProfileLastModified);
        }

        Exceptions.ThrowIf(new MoaRef(record.MoaRef).Id != record.BlizzardId);

        if (changed)
        {
            await query.UpdateAsync();
        }
    }

    private async Task OnGuildGuildAchievementsUpdate(DatabaseConnection database, GuildRecord record, long lastModifiedTime)
    {
        var query = database.GetUpdateQuery(record, out var changed);
        if (CheckAndChange.Check(ref record.BlizzardAchievementsLastModified, lastModifiedTime, ref changed))
        {
            query = query.Set(x => x.BlizzardAchievementsLastModified, record.BlizzardAchievementsLastModified);
        }

        if (changed)
        {
            await query.UpdateAsync();
        }
    }

    private async Task OnGuildUpdateCharacter(DatabaseConnection database, MoaRef characterRef, MoaRef guildRef, long guildId, long blizzardGuildId, string guildName, byte guildRank, GuildCharacter character)
    {
        var record = await _commonServices.CharacterServices.GetOrCreateCharacterRecord(characterRef.Full, BlizzardUpdatePriority.CharacterLow);
        var query = database.GetUpdateQuery(record, out var changed);

        if (CheckAndChange.Check(ref record.BlizzardId, character.Id, ref changed))
        {
            throw new NotImplementedException();
        }

        if (CheckAndChange.Check(ref record.GuildId, guildId, ref changed))
        {
            query = query.Set(x => x.GuildId, record.GuildId);
        }

        if (CheckAndChange.Check(ref record.GuildRef, guildRef.Full, ref changed))
        {
            query = query.Set(x => x.GuildRef, record.GuildRef);
        }

        if (CheckAndChange.Check(ref record.BlizzardGuildId, blizzardGuildId, ref changed))
        {
            query = query.Set(x => x.BlizzardGuildId, record.BlizzardGuildId);
        }

        if (CheckAndChange.Check(ref record.BlizzardGuildName, guildName, ref changed))
        {
            query = query.Set(x => x.BlizzardGuildName, record.BlizzardGuildName);
        }

        if (CheckAndChange.Check(ref record.Name, character.Name, ref changed))
        {
            query = query.Set(x => x.Name, record.Name);
        }

        if (CheckAndChange.Check(ref record.NameSearchable, DatabaseHelpers.GetSearchableName(record.Name), ref changed))
        {
            query = query.Set(x => x.NameSearchable, record.NameSearchable);
        }

        if (CheckAndChange.Check(ref record.RealmId, character.Realm.Id, ref changed))
        {
            query = query.Set(x => x.RealmId, record.RealmId);
        }

        if (CheckAndChange.Check(ref record.Class, (byte)character.PlayableClass.Id, ref changed))
        {
            query = query.Set(x => x.Class, record.Class);
        }

        if (CheckAndChange.Check(ref record.Race, (byte)character.PlayableRace.Id, ref changed))
        {
            query = query.Set(x => x.Race, record.Race);
        }

        if (CheckAndChange.Check(ref record.BlizzardGuildRank, guildRank, ref changed))
        {
            query = query.Set(x => x.BlizzardGuildRank, record.BlizzardGuildRank);
        }

        if (CheckAndChange.Check(ref record.Level, (byte)character.Level, ref changed))
        {
            query = query.Set(x => x.Level, record.Level);
        }

        if (changed)
        {
            await query.UpdateAsync();
        }
    }

    private async Task OnGuildRosterUpdate(DatabaseConnection database, GuildRecord record, long lastModifiedTime)
    {
        var query = database.GetUpdateQuery(record, out var changed);
        if (CheckAndChange.Check(ref record.BlizzardRosterLastModified, lastModifiedTime, ref changed))
        {
            query = query.Set(x => x.BlizzardRosterLastModified, record.BlizzardRosterLastModified);
        }

        if (changed)
        {
            await query.UpdateAsync();
        }
    }
}