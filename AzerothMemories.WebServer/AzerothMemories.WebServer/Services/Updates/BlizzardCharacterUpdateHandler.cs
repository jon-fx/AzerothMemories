namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class BlizzardCharacterUpdateHandler
{
    public BlizzardCharacterUpdateHandler(IServiceProvider services)
    {
    }

    public async Task<HttpStatusCode> TryUpdate(long id, DatabaseConnection database, CharacterRecord record)
    {
        return HttpStatusCode.UnavailableForLegalReasons;
    }
}