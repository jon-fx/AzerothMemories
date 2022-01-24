namespace AzerothMemories.WebBlazor.Services.Commands;

public record Post_TryReportPostComment(Session Session, long PostId, long CommentId, PostReportedReason Reason, string ReasonText) : ICommand<bool>
{
    public Post_TryReportPostComment() : this(Session.Null, 0, 0, PostReportedReason.None, null)
    {
    }
}