namespace AzerothMemories.WebServer.Services.Updates;

public record Updates_UpdateAccountInvalidate(long AccountId, string FusionId, string Username, HashSet<long> CharacterIds)
{
    public Updates_UpdateAccountInvalidate() : this(0, null, null, null)
    {
    }
}