namespace AzerothMemories.WebBlazor.Services.Commands;

public record Admin_SetPostReportResolved(Session Session, bool Delete, int PostId) : ISessionCommand<bool>
{
    public Admin_SetPostReportResolved() : this(Session.Null, false, 0)
    {
    }
}