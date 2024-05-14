namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class UpdateHandler_Guilds : UpdateHandlerBaseResult<GuildRecord, Guild>
{
    public UpdateHandler_Guilds(CommonServices commonServices) : base(BlizzardUpdateType.Guild, commonServices)
    {
    }

    protected override async Task<RequestResult<Guild>> TryExecuteRequest(GuildRecord record, AuthTokenRecord? authTokenRecord, Instant blizzardLastModified)
    {
        var guildRef = new MoaRef(record.MoaRef);
        if (!guildRef.IsValidGuild)
        {
            throw new NotImplementedException();
        }

        using var client = CommonServices.HttpClientProvider.GetWarcraftClient(guildRef.Region);
        return await client.GetGuildProfileSummaryAsync(guildRef.Realm, guildRef.Name, blizzardLastModified).ConfigureAwait(false);
    }

    protected override Task InternalExecuteWithResult(CommandContext context, AppDbContext database, GuildRecord record, Guild requestResult)
    {
        record.BlizzardId = requestResult.Id;
        record.Name = requestResult.Name ?? $"GuildRecord-{record.Id}";
        record.NameSearchable = DatabaseHelpers.GetSearchableName(record.Name);
        record.RealmId = requestResult.Realm?.Id ?? 0;
        record.BlizzardCreatedTimestamp = Instant.FromUnixTimeMilliseconds(requestResult.CreatedTimestamp);
        record.MemberCount = requestResult.MemberCount;
        record.AchievementPoints = requestResult.AchievementPoints;
        record.Faction = requestResult.Faction?.AsFaction() ?? CharacterFaction.None;

        Exceptions.ThrowIf(new MoaRef(record.MoaRef).Id != 0);

        return Task.CompletedTask;
    }
}