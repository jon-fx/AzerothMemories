namespace AzerothMemories.WebServer.Services.Updates;

public record Updates_UpdateRecordCommand(int? AccountId, int? CharacterId, int? GuildId, bool ForcedUpdate, int RequiredChildRecordCount) : ICommand<HttpStatusCode>
{
    public Updates_UpdateRecordCommand() : this(null, null, null, false, 0)
    {
    }
}