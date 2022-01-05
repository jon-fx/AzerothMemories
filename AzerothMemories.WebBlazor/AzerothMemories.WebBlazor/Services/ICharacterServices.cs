namespace AzerothMemories.WebBlazor.Services;

[BasePath("character")]
public interface ICharacterServices
{
    [Post(nameof(TryChangeCharacterAccountSync) + "/{characterId}/{newValue}")]
    Task<bool> TryChangeCharacterAccountSync(Session session, [Path] long characterId, [Path] bool newValue, CancellationToken cancellationToken = default);
}