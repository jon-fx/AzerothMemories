namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Post_TryReportPostComment(Session Session, int PostId, int CommentId, PostReportedReason Reason, string ReasonText) : ISessionCommand<bool>;