namespace AzerothMemories.WebServer.Services.Updates;

public record Updates_UpdateAccountCommand(int AccountId) : ICommand<HttpStatusCode>
{
    public Updates_UpdateAccountCommand() : this(0)
    {
    }
}