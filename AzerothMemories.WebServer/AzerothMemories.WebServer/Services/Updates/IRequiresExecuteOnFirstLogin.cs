namespace AzerothMemories.WebServer.Services.Updates;

internal interface IRequiresExecuteOnFirstLogin
{
    Task OnFirstLogin(AppDbContext database, AccountRecord accountRecord, CharacterRecord characterRecord);
}