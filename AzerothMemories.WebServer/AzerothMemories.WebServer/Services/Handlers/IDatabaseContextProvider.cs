namespace AzerothMemories.WebServer.Services.Handlers;

internal interface IDatabaseContextProvider
{
    Task<AppDbContext> CreateCommandDbContextNow(CancellationToken cancellationToken);
}