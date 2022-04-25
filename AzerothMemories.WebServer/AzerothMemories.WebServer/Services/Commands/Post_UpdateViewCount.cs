namespace AzerothMemories.WebServer.Services.Commands;

public record Post_UpdateViewCount(int AccountId, int PostId) : ICommand<PostRecord>
{
    public Post_UpdateViewCount() : this(0, 0)
    {
    }
}