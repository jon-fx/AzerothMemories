namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Admin_SetPostCommentReportResolved(Session Session, bool Delete, int PostId, int CommentId) : ISessionCommand<bool>;