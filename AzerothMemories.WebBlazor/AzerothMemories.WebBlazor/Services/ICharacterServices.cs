namespace AzerothMemories.WebBlazor.Services;

[BasePath("character")]
public interface ICharacterServices
{
    [Post(nameof(TryChangeCharacterAccountSync) + "/{characterId}/{newValue}")]
    Task<bool> TryChangeCharacterAccountSync(Session session, [Path] long characterId, [Path] bool newValue);

    [ComputeMethod]
    [Get(nameof(TryGetCharacter) + "/{characterId}")]
    Task<CharacterAccountViewModel> TryGetCharacter(Session session, [Path] long characterId);

    [ComputeMethod]
    [Get(nameof(TryGetCharacter) + "/{region}/{realmSlug}/{characterName}")]
    Task<CharacterAccountViewModel> TryGetCharacter(Session session, [Path] BlizzardRegion region, [Path] string realmSlug, [Path] string characterName);

    [Post(nameof(TryEnqueueUpdate) + "/{region}/{realmSlug}/{characterName}")]
    Task<bool> TryEnqueueUpdate(Session session, [Path] BlizzardRegion region, [Path] string realmSlug, [Path] string characterName);

    [Post(nameof(TrySetCharacterDeleted) + "/{characterId}")]
    Task<bool> TrySetCharacterDeleted(Session session, [Path] long characterId);

    [Post(nameof(TrySetCharacterRenamedOrTransferred) + "/{oldCharacterId}/{newCharacterId}")]
    Task<bool> TrySetCharacterRenamedOrTransferred(Session session, [Path] long oldCharacterId, [Path] long newCharacterId);
}