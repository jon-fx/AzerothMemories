namespace AzerothMemories.WebBlazor.Services.Commands;

public record Admin_SetPostTagReportResolved(Session Session, bool Delete, int PostId, string TagString, int ReportedTagId) : ISessionCommand<bool>
{
    public Admin_SetPostTagReportResolved() : this(Session.Null, false, 0, null, 0)
    {
    }
}