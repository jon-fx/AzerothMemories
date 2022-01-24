namespace AzerothMemories.WebBlazor.Services.Commands;

public record Post_InvalidateAccount(long Id)
{
    public Post_InvalidateAccount() : this(0)
    {
    }
}