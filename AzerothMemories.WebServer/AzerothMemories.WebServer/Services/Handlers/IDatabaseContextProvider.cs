namespace AzerothMemories.WebServer.Services.Handlers;

internal interface IDatabaseContextProvider
{
    Task<AppDbContext> CreateCommandDbContext();
}