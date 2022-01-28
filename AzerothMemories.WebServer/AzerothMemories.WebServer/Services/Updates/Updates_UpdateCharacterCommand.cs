namespace AzerothMemories.WebServer.Services.Updates;

public record Updates_UpdateCharacterCommand(long CharacterId) : ICommand<HttpStatusCode>
{
    public Updates_UpdateCharacterCommand() : this(0)
    {
    }
}