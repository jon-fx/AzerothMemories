namespace AzerothMemories.WebServer.Services.Commands;

public record Post_InvalidatePost(int PostId)
{
    public Post_InvalidatePost() : this(0)
    {
    }
}