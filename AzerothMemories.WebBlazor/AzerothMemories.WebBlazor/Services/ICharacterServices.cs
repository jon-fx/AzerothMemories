namespace AzerothMemories.WebBlazor.Services;

[BasePath("character")]
public interface ICharacterServices
{
    [Post(nameof(TryChangeCharacterAccountSync) + "/{characterId}/{newValue}")]
    Task<bool> TryChangeCharacterAccountSync(Session session, [Path] long characterId, [Path] bool newValue, CancellationToken cancellationToken = default);

    [ComputeMethod]
    [Get(nameof(TryGetCharacter) + "/{characterId}")]
    Task<CharacterAccountViewModel> TryGetCharacter(Session session, [Path] long characterId);

    [ComputeMethod]
    [Get(nameof(TryGetCharacter) + "/{region}/{realmSlug}/{characterName}")]
    Task<CharacterAccountViewModel> TryGetCharacter(Session session, [Path] BlizzardRegion region, [Path] string realmSlug, [Path] string characterName);
}