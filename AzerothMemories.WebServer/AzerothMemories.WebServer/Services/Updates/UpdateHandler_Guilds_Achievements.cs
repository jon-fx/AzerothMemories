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

    protected override Task InternalExecute(CommandContext context, AppDbContext database, GuildRecord record, GuildAchievements requestResult)
    {
        return Task.CompletedTask;
    }
}