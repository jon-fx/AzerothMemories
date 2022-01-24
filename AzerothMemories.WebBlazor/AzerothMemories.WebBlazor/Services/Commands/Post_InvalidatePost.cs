namespace AzerothMemories.WebBlazor.Services.Commands;

public record Post_InvalidatePost(long Id)
{
    public Post_InvalidatePost() : this(0)
    {
    }
}