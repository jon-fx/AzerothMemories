namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class UpdateHandler_Guilds_Roster : UpdateHandlerBaseResult<GuildRecord, GuildRoster>
{
    public UpdateHandler_Guilds_Roster(UpdateHandlerInfo handlerInfo) : base(handlerInfo)
    {
    }

    protected override async Task<RequestResult<GuildRoster>> TryExecuteRequest(GuildRecord record, AuthTokenRecord? authTokenRecord, Instant blizzardLastModified)
    {
        var guildRef = new MoaRef(record.MoaRef);
        if (!guildRef.IsValidGuild)
        {
            throw new NotImplementedException();
        }

        using var client = CommonServices.HttpClientProvider.GetWarcraftClient(guildRef.Region);
        return await client.GetGuildRosterAsync(guildRef.RealmVersion, guildRef.Realm, guildRef.Name, blizzardLastModified).ConfigureAwait(false);
    }

    protected override async Task InternalExecuteWithResult(AppDbContext database, GuildRecord record, GuildRoster requestResult)
    {
        foreach (var guildMember in requestResult.Members.SafeEnumerable())
        {
            var guildMemberCharacter = guildMember.Character;
            if (guildMemberCharacter == null)
            {
                continue;
            }

            var characterId = guildMemberCharacter.Id;
            var characterName = guildMemberCharacter.Name;
            var characterRealm = guildMemberCharacter.Realm?.Slug;
            var characterRef = MoaRef.GetCharacterRef(record.BlizzardRegionId, record.BlizzardRealmVersionId, characterRealm, characterName, characterId);
            var characterRecord = await CommonServices.CharacterServices.GetOrCreateCharacterRecord(characterRef.Full, false).ConfigureAwait(false);
            if (characterRecord == null)
            {
                continue;
            }

            if (characterRecord.BlizzardId != guildMemberCharacter.Id)
            {
                throw new NotImplementedException();
            }

            database.Attach(characterRecord);
            characterRecord.GuildId = record.Id;
            characterRecord.GuildRef = record.MoaRef;
            characterRecord.BlizzardGuildName = requestResult.Guild?.Name;
            characterRecord.Name = guildMemberCharacter.Name ?? $"Character-{characterRecord.Id}";
            characterRecord.NameSearchable = DatabaseHelpers.GetSearchableName(characterRecord.Name);
            characterRecord.RealmId = guildMemberCharacter.Realm?.Id ?? 0;
            characterRecord.Class = (byte)(guildMemberCharacter.PlayableClass?.Id ?? 0);
            characterRecord.Race = (byte)(guildMemberCharacter.PlayableRace?.Id ?? 0);
            characterRecord.BlizzardGuildRank = (byte)guildMember.Rank;
            characterRecord.Level = (byte)guildMemberCharacter.Level;
            characterRecord.BlizzardRealmVersionId = record.BlizzardRealmVersionId;
        }
    }
}