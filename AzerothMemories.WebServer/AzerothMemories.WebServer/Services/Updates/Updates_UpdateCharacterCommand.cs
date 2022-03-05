namespace AzerothMemories.WebServer.Services.Updates;

public record Updates_UpdateCharacterCommand(int CharacterId) : ICommand<HttpStatusCode>
{
    public Updates_UpdateCharacterCommand() : this(0)
    {
    }
}