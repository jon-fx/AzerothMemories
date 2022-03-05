namespace AzerothMemories.WebBlazor.Services.Commands;

public record Post_TryReportPost(Session Session, int PostId, PostReportedReason Reason, string ReasonText) : ISessionCommand<bool>
{
    public Post_TryReportPost() : this(Session.Null, 0, PostReportedReason.None, null)
    {
    }
}