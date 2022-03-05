namespace AzerothMemories.WebBlazor.Services.Commands;

public record Post_TryDeletePost(Session Session, int PostId) : ISessionCommand<long>
{
    public Post_TryDeletePost() : this(Session.Null, 0)
    {
    }
}