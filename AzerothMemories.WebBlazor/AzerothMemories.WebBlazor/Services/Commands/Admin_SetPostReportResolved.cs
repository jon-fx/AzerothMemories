namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Admin_SetPostReportResolved(Session Session, bool Delete, int PostId) : ISessionCommand<bool>;