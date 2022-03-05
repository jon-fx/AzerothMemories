namespace AzerothMemories.WebServer.Services.Updates;

public record Updates_UpdateAccountInvalidate(int AccountId, string FusionId, string Username, HashSet<int> CharacterIds)
{
    public Updates_UpdateAccountInvalidate() : this(0, null, null, null)
    {
    }
}