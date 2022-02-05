namespace AzerothMemories.WebBlazor.Services.Commands;

public record Post_TryDeleteComment(Session Session, long PostId, long CommentId) : ISessionCommand<long>
{
    public Post_TryDeleteComment() : this(Session.Null, 0, 0)
    {
    }
}