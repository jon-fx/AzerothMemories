namespace AzerothMemories.WebServer.Services.Commands;

public record Account_InvalidateFollowing(int AccountId, int Page)
{
    public Account_InvalidateFollowing() : this(0, 0)
    {
    }
}