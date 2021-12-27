namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class BlizzardCharacterUpdateHandler
{
    public BlizzardCharacterUpdateHandler(IServiceProvider services)
    {
    }

    public async Task<HttpStatusCode> TryUpdate(long id, AppDbContext dbContext, CharacterRecord record)
    {
        return HttpStatusCode.UnavailableForLegalReasons;
    }
}