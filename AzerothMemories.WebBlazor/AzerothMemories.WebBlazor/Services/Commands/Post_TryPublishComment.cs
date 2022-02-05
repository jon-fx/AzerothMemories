namespace AzerothMemories.WebBlazor.Services.Commands;

public record Post_TryPublishComment(Session Session, long PostId, long ParentCommentId, string CommentText) : ISessionCommand<long>
{
    public Post_TryPublishComment() : this(Session.Null, 0, 0, null)
    {
    }
}