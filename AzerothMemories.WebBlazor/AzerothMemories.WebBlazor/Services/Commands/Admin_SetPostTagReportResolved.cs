namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Admin_SetPostTagReportResolved(Session Session, bool Delete, int PostId, string TagString, int ReportedTagId) : ISessionCommand<bool>;