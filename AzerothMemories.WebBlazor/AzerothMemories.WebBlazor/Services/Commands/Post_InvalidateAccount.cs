namespace AzerothMemories.WebBlazor.Services.Commands;

public record Post_InvalidateAccount(int AccountId)
{
    public Post_InvalidateAccount() : this(0)
    {
    }
}