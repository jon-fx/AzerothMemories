namespace AzerothMemories.WebServer.Services.Updates;

public record Updates_UpdateRecordResetStatusCommand(int? AccountId, int? CharacterId, int? GuildId) : ICommand<HttpStatusCode>
{
    public Updates_UpdateRecordResetStatusCommand() : this(null, null, null)
    {
    }
}