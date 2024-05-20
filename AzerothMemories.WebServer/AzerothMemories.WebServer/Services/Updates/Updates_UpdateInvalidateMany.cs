namespace AzerothMemories.WebServer.Services.Updates;

public record Updates_UpdateInvalidateMany(int? AccountId, int? CharacterId, int? GuildId)
{
    public Updates_UpdateInvalidateMany() : this(null, null, null)
    {
    }

    public void Invalidate(CommonServices commonServices)
    {
        if (AccountId.HasValue)
        {
            _ = commonServices.AccountServices.DependsOnAccountRecord(AccountId.Value);
        }

        if (CharacterId.HasValue)
        {
            _ = commonServices.CharacterServices.DependsOnCharacterRecord(CharacterId.Value);
        }

        if (GuildId.HasValue)
        {
            _ = commonServices.GuildServices.DependsOnGuildRecord(GuildId.Value);
        }
    }
}