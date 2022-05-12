namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Post_TryReportPost(Session Session, int PostId, PostReportedReason Reason, string ReasonText) : ISessionCommand<bool>;