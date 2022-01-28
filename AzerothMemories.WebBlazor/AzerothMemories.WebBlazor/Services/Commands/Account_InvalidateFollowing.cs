namespace AzerothMemories.WebBlazor.Services.Commands;

public record Account_InvalidateFollowing(long AccountId, int Page)
{
    public Account_InvalidateFollowing() : this(0, 0)
    {
    }
}