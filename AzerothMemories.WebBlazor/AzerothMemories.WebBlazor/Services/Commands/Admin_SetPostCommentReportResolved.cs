namespace AzerothMemories.WebBlazor.Services.Commands;

public record Admin_SetPostCommentReportResolved(Session Session, bool Delete, int PostId, int CommentId) : ISessionCommand<bool>
{
    public Admin_SetPostCommentReportResolved() : this(Session.Null, false, 0, 0)
    {
    }
}