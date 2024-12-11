namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class UpdateHandler_Characters : UpdateHandlerBaseResult<CharacterRecord, CharacterProfileSummary>
{
    public UpdateHandler_Characters(CommonServices commonServices, ILogger<BlizzardUpdateServices> logger) : base(BlizzardUpdateType.Character, commonServices, logger)
    {
    }

    protected override async Task<RequestResult<CharacterProfileSummary>> TryExecuteRequest(CharacterRecord record, AuthTokenRecord? authTokenRecord, Instant blizzardLastModified)
    {
        var characterRef = new MoaRef(record.MoaRef);
        if (!characterRef.IsValidCharacter)
        {
            throw new NotImplementedException();
        }

        using var client = CommonServices.HttpClientProvider.GetWarcraftClient(record.BlizzardRegionId);
        return await client.GetCharacterProfileSummaryAsync(characterRef.Realm, characterRef.Name, blizzardLastModified).ConfigureAwait(false);
    }

    protected override async Task InternalExecuteWithResult(AppDbContext database, CharacterRecord record, CharacterProfileSummary requestResult)
    {
        record.RealmId = requestResult.Realm?.Id ?? 0;
        record.Name = requestResult.Name ?? $"Character-{record.Id}";
        record.NameSearchable = DatabaseHelpers.GetSearchableName(record.Name);
        record.Class = (byte)(requestResult.CharacterClass?.Id ?? 0);
        record.Race = (byte)(requestResult.Race?.Id ?? 0);
        record.Level = (byte)requestResult.Level;
        record.Faction = requestResult.Faction?.AsFaction() ?? CharacterFaction.None;
        record.Gender = requestResult.Gender?.AsGender() ?? byte.MinValue;
        record.CharacterStatus = CharacterStatus2.None;

        var guildData = requestResult.Guild;
        string? newGuildName = null;
        GuildRecord? guildRecord = null;

        if (guildData != null)
        {
            newGuildName = guildData.Name;
            var newGuildRef = MoaRef.GetGuildRef(record.BlizzardRegionId, guildData.Realm?.Slug, newGuildName).Full;
            guildRecord = await CommonServices.GuildServices.GetOrCreate(newGuildRef).ConfigureAwait(false);
        }

        record.BlizzardGuildName = newGuildName;
        record.GuildRef = guildRecord?.MoaRef;
        record.GuildId = guildRecord?.Id;
    }
}