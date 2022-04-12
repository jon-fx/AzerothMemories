namespace AzerothMemories.WebServer.Services.Updates;

internal interface IRequiresExecuteOnFirstLogin
{
    Task OnFirstLogin(CommandContext context, AppDbContext database, AuthTokenRecord authTokenRecord, CharacterRecord characterRecord);
}