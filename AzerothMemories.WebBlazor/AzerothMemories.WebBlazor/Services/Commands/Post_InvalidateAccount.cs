namespace AzerothMemories.WebBlazor.Services.Commands;

public record Post_InvalidateAccount(long AccountId)
{
    public Post_InvalidateAccount() : this(0)
    {
    }
}