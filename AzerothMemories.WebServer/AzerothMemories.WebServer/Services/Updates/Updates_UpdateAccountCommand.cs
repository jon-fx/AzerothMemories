namespace AzerothMemories.WebServer.Services.Updates;

public record Updates_UpdateAccountCommand(long AccountId) : ICommand<HttpStatusCode>
{
    public Updates_UpdateAccountCommand() : this(0)
    {
    }
}