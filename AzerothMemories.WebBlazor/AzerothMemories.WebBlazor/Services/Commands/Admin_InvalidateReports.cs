namespace AzerothMemories.WebBlazor.Services.Commands;

public record Admin_InvalidateReports(bool AlwaysTrue)
{
    public Admin_InvalidateReports() : this(true)
    {
    }
}