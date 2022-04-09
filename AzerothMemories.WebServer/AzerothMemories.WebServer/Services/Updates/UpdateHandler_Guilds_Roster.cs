namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class UpdateHandler_Guilds_Roster : UpdateHandlerBase<GuildRecord, GuildRoster>
{
    public UpdateHandler_Guilds_Roster(CommonServices commonServices) : base(BlizzardUpdateType.Guild_Roster, commonServices)
    {
    }

    protected override async Task<RequestResult<GuildRoster>> TryExecuteRequest(GuildRecord record, Instant blizzardLastModified)
    {
        var guildRef = new MoaRef(record.MoaRef);
        using var client = CommonServices.HttpClientProvider.GetWarcraftClient(guildRef.Region);
        return await client.GetGuildRosterAsync(guildRef.Realm, guildRef.Name, blizzardLastModified).ConfigureAwait(false);
    }

    protected override async Task InternalExecute(CommandContext context, AppDbContext database, GuildRecord record, GuildRoster requestResult)
    {
        foreach (var guildMember in requestResult.Members)
        {
            var characterId = guildMember.Character.Id;
            var characterName = guildMember.Character.Name;
            var characterRealm = guildMember.Character.Realm.Slug;
            var characterRef = MoaRef.GetCharacterRef(record.BlizzardRegionId, characterRealm, characterName, characterId);
            var characterRecord = await CommonServices.CharacterServices.GetOrCreateCharacterRecord(characterRef.Full, BlizzardUpdatePriority.CharacterLow).ConfigureAwait(false);
            if (characterRecord.BlizzardId != guildMember.Character.Id)
            {
                throw new NotImplementedException();
            }

            database.Attach(characterRecord);
            characterRecord.GuildId = record.Id;
            characterRecord.GuildRef = record.MoaRef;
            characterRecord.BlizzardGuildName = requestResult.Guild.Name;
            characterRecord.Name = guildMember.Character.Name;
            characterRecord.NameSearchable = DatabaseHelpers.GetSearchableName(guildMember.Character.Name);
            characterRecord.RealmId = guildMember.Character.Realm.Id;
            characterRecord.Class = (byte)guildMember.Character.PlayableClass.Id;
            characterRecord.Race = (byte)guildMember.Character.PlayableRace.Id;
            characterRecord.BlizzardGuildRank = (byte)guildMember.Rank;
            characterRecord.Level = (byte)guildMember.Character.Level;
        }
    }
}