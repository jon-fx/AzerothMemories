namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class BlizzardCharacterUpdateHandler
{
    private readonly IServiceProvider _services;
    private readonly DatabaseProvider _databaseProvider;
    private readonly WarcraftClientProvider _warcraftClientProvider;

    public BlizzardCharacterUpdateHandler(IServiceProvider services)
    {
        _services = services;
        _databaseProvider = _services.GetRequiredService<DatabaseProvider>();
        _warcraftClientProvider = _services.GetRequiredService<WarcraftClientProvider>();
    }

    public async Task<HttpStatusCode> TryUpdate(long id, DatabaseConnection database, CharacterRecord record)
    {
        var characterRef = new MoaRef(record.MoaRef);
        using var client = _warcraftClientProvider.Get(record.BlizzardRegionId);
        var characterSummary = await client.GetCharacterProfileSummaryAsync(characterRef.Realm, characterRef.Name, record.BlizzardProfileLastModified).ConfigureAwait(false);
        if (characterSummary.IsSuccess)
        {
            //record.Id = await grain.OnCharacterUpdate(characterSummary.ResultData, characterSummary.ResultLastModifiedMs).ConfigureAwait(false);
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
            //await grain.OnCharacterRendersUpdate(characterRenders.ResultData, characterRenders.ResultLastModifiedMs).ConfigureAwait(false);
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
            await UpdateAchievementsPosts(record, achievementsSummary);
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

    private async Task UpdateAchievementsPosts(CharacterRecord record, RequestResult<CharacterAchievementsSummary> achievementsSummary)
    {
        Exceptions.ThrowIf(record.Id == 0);

        await using var database = _databaseProvider.GetDatabase();
        var achievementData = achievementsSummary.ResultData;
        var currentAchievements = await database.CharacterAchievements.Where(x => x.CharacterId == record.Id && x.AchievementId > 0).ToDictionaryAsync(x => x.AchievementId, x => x).ConfigureAwait(false);

        var updatedAchievementRecords = new HashSet<CharacterAchievementRecord>();

        foreach (var achievement in achievementData.Achievements)
        {
            var timeSpan = achievement.CompletedTimestamp.GetValueOrDefault(0);
            if (timeSpan == 0)
            {
                continue;
            }

            var changed = false;
            if (!currentAchievements.TryGetValue(achievement.Id, out var achievementRecord))
            {
                changed = true;
                achievementRecord = new CharacterAchievementRecord
                {
                    AccountId = record.AccountId,
                    CharacterId = record.Id,
                    AchievementId = achievement.Id
                };
            }

            CheckAndChange.Check(ref achievementRecord.AccountId, record.AccountId, ref changed);
            CheckAndChange.Check(ref achievementRecord.AchievementTimeStamp, timeSpan, ref changed);
            CheckAndChange.Check(ref achievementRecord.CompletedByCharacter, achievement.Criteria == null || achievement.Criteria != null && achievement.Criteria.IsCompleted, ref changed);
            CheckAndChange.Check(ref achievementRecord.AccountId, record.AccountId, ref changed);

            if (changed)
            {
                updatedAchievementRecords.Add(achievementRecord);
            }
        }

        if (updatedAchievementRecords.Count > 0)
        {
            await database.CharacterAchievements.Merge()
                .Using(updatedAchievementRecords)
                .OnTargetKey()
                .InsertWhenNotMatched()
                .UpdateWhenMatched()
                .MergeAsync()
                .ConfigureAwait(false);
        }

#if DEBUG
        currentAchievements = await database.CharacterAchievements.Where(x => x.CharacterId == record.Id && x.AchievementId > 0).ToDictionaryAsync(x => x.AchievementId, x => x).ConfigureAwait(false);
#endif

        //await grain.OnAchievementUpdate(achievementData.TotalPoints, achievementData.TotalQuantity, achievementsSummary.ResultLastModifiedMs);
    }
}