namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class UpdateHandler_Guilds_Achievements : UpdateHandlerBase<GuildRecord, GuildAchievements>
{
    public UpdateHandler_Guilds_Achievements(CommonServices commonServices) : base(BlizzardUpdateType.Guild_Achievements, commonServices)
    {
    }

    protected override async Task<RequestResult<GuildAchievements>> TryExecuteRequest(GuildRecord record, Instant blizzardLastModified)
    {
        var guildRef = new MoaRef(record.MoaRef);
        using var client = CommonServices.WarcraftClientProvider.Get(guildRef.Region);
        return await client.GetGuildAchievementsAsync(guildRef.Realm, guildRef.Name, blizzardLastModified).ConfigureAwait(false);
    }

    protected override async Task InternalExecute(CommandContext context, AppDbContext database, GuildRecord record, GuildAchievements requestResult)
    {
        var currentAchievements = await database.GuildAchievements.Where(x => x.GuildId == record.Id).ToDictionaryAsync(x => x.AchievementId, x => x).ConfigureAwait(false);

        foreach (var achievement in requestResult.Achievements)
        {
            var timeStamp = achievement.CompletedTimestamp.GetValueOrDefault(0);
            if (timeStamp <= 0)
            {
                continue;
            }

            if (!currentAchievements.TryGetValue(achievement.Id, out var achievementRecord))
            {
                achievementRecord = new GuildAchievementRecord
                {
                    GuildId = record.Id,
                    AchievementId = achievement.Id,
                };

                database.GuildAchievements.Add(achievementRecord);
            }

            achievementRecord.AchievementTimeStamp = Instant.FromUnixTimeMilliseconds(timeStamp);
        }

        record.AchievementTotalPoints = requestResult.TotalPoints;
        record.AchievementTotalQuantity = requestResult.TotalQuantity;
    }
}