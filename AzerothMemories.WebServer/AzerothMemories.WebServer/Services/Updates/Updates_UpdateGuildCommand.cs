namespace AzerothMemories.WebServer.Services.Updates;

public record Updates_UpdateGuildCommand(long GuildId) : ICommand<HttpStatusCode>
{
    public Updates_UpdateGuildCommand() : this(0)
    {
    }
}