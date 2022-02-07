namespace AzerothMemories.WebServer.Services.Updates;

public record Updates_OnHourlyUpdate(int Id) : ICommand<int>
{
    public Updates_OnHourlyUpdate() : this(0)
    {
    }
}