namespace AzerothMemories.WebServer.Services.Updates;

public record Updates_UpdateGuildCommand(int GuildId) : ICommand<HttpStatusCode>
{
    public Updates_UpdateGuildCommand() : this(0)
    {
    }
}