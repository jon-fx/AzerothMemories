namespace AzerothMemories.WebServer.Services.Updates;

internal interface IUpdateHandlerBase<TRecord> where TRecord : IBlizzardUpdateRecord
{
    BlizzardUpdateType UpdateType { get; }

    CommonServices CommonServices { get; }

    Task<HttpStatusCode> ExecuteOn(CommandContext context, AppDbContext database, TRecord record, BlizzardUpdateChildRecord lastModifiedTimes);
}