namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class UpdateHandler_Characters_AchievementStatistics : UpdateHandlerBaseResult<CharacterRecord, CharacterAchievementStatistics>, IRequiresExecuteOnFirstLogin
{
    public UpdateHandler_Characters_AchievementStatistics(CommonServices commonServices, ILogger<BlizzardUpdateServices> logger) : base(BlizzardUpdateType.Character_AchievementStatistics, commonServices, logger)
    {
    }

    public async Task OnFirstLogin(CommandContext context, AppDbContext database, AccountRecord accountRecord, CharacterRecord characterRecord)
    {
        var records = await database.CharacterAchievementStatistics.Where(x => x.CharacterId == characterRecord.Id && x.AccountId == null).ToArrayAsync().ConfigureAwait(false);
        foreach (var record in records)
        {
            record.AccountId = accountRecord.Id;
        }
    }

    protected override async Task<RequestResult<CharacterAchievementStatistics>> TryExecuteRequest(CharacterRecord record, AuthTokenRecord? authTokenRecord, Instant blizzardLastModified)
    {
        var characterRef = new MoaRef(record.MoaRef);
        if (!characterRef.IsValidCharacter)
        {
            throw new NotImplementedException();
        }

        using var client = CommonServices.HttpClientProvider.GetWarcraftClient(record.BlizzardRegionId);
        return await client.GetCharacterStatisticsSummaryAsync(characterRef.Realm, characterRef.Name, blizzardLastModified).ConfigureAwait(false);
    }

    protected override async Task InternalExecuteWithResult(CommandContext context, AppDbContext database, CharacterRecord record, CharacterAchievementStatistics requestResult)
    {
        var currentAchievementStatistics = await database.CharacterAchievementStatistics.Where(x => x.CharacterId == record.Id).ToDictionaryAsync(x => x.StatisticId, x => x).ConfigureAwait(false);

        foreach (var category in requestResult.Categories.SafeEnumerable())
        {
            foreach (var statistic in category.EnumerateStatistics())
            {
                if (!currentAchievementStatistics.TryGetValue(statistic.Id, out var achievementRecord))
                {
                    achievementRecord = new CharacterAchievementStatisticRecord
                    {
                        AccountId = record.AccountId,
                        CharacterId = record.Id,
                        StatisticId = statistic.Id,
                    };

                    currentAchievementStatistics.Add(achievementRecord.StatisticId, achievementRecord);
                    database.CharacterAchievementStatistics.Add(achievementRecord);
                }

                if (statistic.Description != null)
                {
                    achievementRecord.StatisticDescription.EnUs = statistic.Description.En_US;
                    achievementRecord.StatisticDescription.EsMx = statistic.Description.Es_MX;
                    achievementRecord.StatisticDescription.PtBr = statistic.Description.Pt_BR;
                    achievementRecord.StatisticDescription.EnGb = statistic.Description.En_GB;

                    achievementRecord.StatisticDescription.EsEs = statistic.Description.Es_ES;
                    achievementRecord.StatisticDescription.FrFr = statistic.Description.Fr_FR;
                    achievementRecord.StatisticDescription.RuRu = statistic.Description.Ru_RU;
                    achievementRecord.StatisticDescription.DeDe = statistic.Description.De_DE;

                    achievementRecord.StatisticDescription.PtPt = statistic.Description.Pt_PT;
                    achievementRecord.StatisticDescription.ItIt = statistic.Description.It_IT;

                    achievementRecord.StatisticDescription.KoKr = statistic.Description.Ko_KR;
                    achievementRecord.StatisticDescription.ZhTw = statistic.Description.Zh_TW;
                    achievementRecord.StatisticDescription.ZhCn = statistic.Description.Zh_CN;
                }

                achievementRecord.StatisticQuantity = statistic.Quantity;
                //achievementRecord.StatisticDescription = statistic.Description;
                achievementRecord.StatisticTimeStamp = Instant.FromUnixTimeMilliseconds(statistic.LastUpdatedTimestamp);
            }
        }
    }
}