namespace AzerothMemories.WebBlazor.Services.Commands;

public record Post_TryPublishComment(Session Session, int PostId, int ParentCommentId, string CommentText) : ISessionCommand<int>
{
    public Post_TryPublishComment() : this(Session.Null, 0, 0, null)
    {
    }
}