namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class UpdateHandler_Characters : UpdateHandlerBaseResult<CharacterRecord, CharacterProfileSummary>
{
    public UpdateHandler_Characters(CommonServices commonServices) : base(BlizzardUpdateType.Character, commonServices)
    {
    }

    protected override async Task<RequestResult<CharacterProfileSummary>> TryExecuteRequest(CharacterRecord record, Instant blizzardLastModified)
    {
        var characterRef = new MoaRef(record.MoaRef);
        using var client = CommonServices.HttpClientProvider.GetWarcraftClient(record.BlizzardRegionId);
        return await client.GetCharacterProfileSummaryAsync(characterRef.Realm, characterRef.Name, blizzardLastModified).ConfigureAwait(false);
    }

    protected override async Task InternalExecuteWithResult(CommandContext context, AppDbContext database, CharacterRecord record, CharacterProfileSummary requestResult)
    {
        record.RealmId = requestResult.Realm.Id;
        record.Name = requestResult.Name;
        record.NameSearchable = DatabaseHelpers.GetSearchableName(record.Name);
        record.Class = (byte)requestResult.CharacterClass.Id;
        record.Race = (byte)requestResult.Race.Id;
        record.Level = (byte)requestResult.Level;
        record.Faction = requestResult.Faction.AsFaction();
        record.Gender = requestResult.Gender.AsGender();

        var guildData = requestResult.Guild;
        string newGuildName = null;
        GuildRecord guildRecord = null;

        if (guildData != null)
        {
            newGuildName = guildData.Name;
            var newGuildRef = MoaRef.GetGuildRef(record.BlizzardRegionId, guildData.Realm.Slug, newGuildName).Full;
            guildRecord = await CommonServices.GuildServices.GetOrCreate(newGuildRef).ConfigureAwait(false);
        }

        record.BlizzardGuildName = newGuildName;
        record.GuildRef = guildRecord?.MoaRef;
        record.GuildId = guildRecord?.Id;
    }
}