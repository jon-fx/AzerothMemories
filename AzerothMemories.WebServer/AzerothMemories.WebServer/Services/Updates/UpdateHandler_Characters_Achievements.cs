namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class UpdateHandler_Characters_Achievements : UpdateHandlerBaseResult<CharacterRecord, CharacterAchievementsSummary>, IRequiresExecuteOnFirstLogin
{
    public UpdateHandler_Characters_Achievements(CommonServices commonServices, ILogger<BlizzardUpdateServices> logger) : base(BlizzardUpdateType.Character_Achievements, commonServices, logger)
    {
    }

    public async Task OnFirstLogin(CommandContext context, AppDbContext database, AccountRecord accountRecord, CharacterRecord characterRecord)
    {
        var records = await database.CharacterAchievements.Where(x => x.CharacterId == characterRecord.Id && x.AccountId == null).ToArrayAsync().ConfigureAwait(false);
        foreach (var record in records)
        {
            record.AccountId = accountRecord.Id;
        }
    }

    protected override async Task<RequestResult<CharacterAchievementsSummary>> TryExecuteRequest(CharacterRecord record, AuthTokenRecord? authTokenRecord, Instant blizzardLastModified)
    {
        var characterRef = new MoaRef(record.MoaRef);
        if (!characterRef.IsValidCharacter)
        {
            throw new NotImplementedException();
        }

        using var client = CommonServices.HttpClientProvider.GetWarcraftClient(record.BlizzardRegionId);
        return await client.GetCharacterAchievementsSummaryAsync(characterRef.Realm, characterRef.Name, blizzardLastModified).ConfigureAwait(false);
    }

    protected override async Task InternalExecuteWithResult(CommandContext context, AppDbContext database, CharacterRecord record, CharacterAchievementsSummary requestResult)
    {
        var currentAchievements = await database.CharacterAchievements.Where(x => x.CharacterId == record.Id).ToDictionaryAsync(x => x.AchievementId, x => x).ConfigureAwait(false);
        var firstEverAchievements = await database.CharacterFirstAchievements.ToDictionaryAsync(x => x.AchievementId, x => x).ConfigureAwait(false);

        foreach (var achievement in requestResult.Achievements.SafeEnumerable())
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
                    AccountId = record.AccountId,
                    CharacterId = record.Id,
                    AchievementId = achievement.Id
                };

                database.CharacterAchievements.Add(achievementRecord);
            }

            achievementRecord.AchievementTimeStamp = Instant.FromUnixTimeMilliseconds(timeStamp);

            if (firstEverAchievements.TryGetValue(achievementRecord.AchievementId, out var firstEverAchievementRecord))
            {
                if (firstEverAchievementRecord.AchievementTimeStamp > achievementRecord.AchievementTimeStamp)
                {
                    firstEverAchievementRecord.AchievementTimeStamp = achievementRecord.AchievementTimeStamp;
                }
            }
            else
            {
                firstEverAchievementRecord = new CharacterFirstAchievementRecord
                {
                    AchievementId = achievementRecord.AchievementId,
                    AchievementTimeStamp = achievementRecord.AchievementTimeStamp
                };

                database.CharacterFirstAchievements.Add(firstEverAchievementRecord);
                firstEverAchievements.Add(firstEverAchievementRecord.AchievementId, firstEverAchievementRecord);
            }
        }

        record.AchievementTotalPoints = requestResult.TotalPoints;
        record.AchievementTotalQuantity = requestResult.TotalQuantity;
    }
}