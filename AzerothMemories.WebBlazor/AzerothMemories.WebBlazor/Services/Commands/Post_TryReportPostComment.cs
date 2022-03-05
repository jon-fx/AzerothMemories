namespace AzerothMemories.WebBlazor.Services.Commands;

public record Post_TryReportPostComment(Session Session, int PostId, int CommentId, PostReportedReason Reason, string ReasonText) : ISessionCommand<bool>
{
    public Post_TryReportPostComment() : this(Session.Null, 0, 0, PostReportedReason.None, null)
    {
    }
}