namespace AzerothMemories.WebBlazor.Services.Commands;

public record Post_TryDeletePost(Session Session, long PostId) : ISessionCommand<long>
{
    public Post_TryDeletePost() : this(Session.Null, 0)
    {
    }
}