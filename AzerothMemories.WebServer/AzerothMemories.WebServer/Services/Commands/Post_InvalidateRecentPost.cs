namespace AzerothMemories.WebServer.Services.Commands;

public record Post_InvalidateRecentPost(bool AlwaysTrue)
{
    public Post_InvalidateRecentPost() : this(true)
    {
    }
}