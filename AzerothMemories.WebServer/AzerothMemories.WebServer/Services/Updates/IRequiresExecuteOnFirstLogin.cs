namespace AzerothMemories.WebServer.Services.Updates;

internal interface IRequiresExecuteOnFirstLogin
{
    Task OnFirstLogin(CommandContext context, AppDbContext database, AccountRecord accountRecord, CharacterRecord characterRecord);
}