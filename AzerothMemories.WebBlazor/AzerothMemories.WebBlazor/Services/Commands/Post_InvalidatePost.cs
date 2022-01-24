namespace AzerothMemories.WebBlazor.Services.Commands;

public record Post_InvalidatePost(long PostId)
{
    public Post_InvalidatePost() : this(0)
    {
    }
}