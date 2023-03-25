namespace AzerothMemories.WebBlazor.Services;

[BasePath("character")]
public interface ICharacterServices : IComputeService
{
    [CommandHandler]
    [Post(nameof(TryChangeCharacterAccountSync))]
    Task<bool> TryChangeCharacterAccountSync([Body] Character_TryChangeCharacterAccountSync command, CancellationToken cancellationToken = default);

    [ComputeMethod]
    [Get(nameof(TryGetCharacter) + "/{characterId}")]
    Task<CharacterAccountViewModel> TryGetCharacter(Session session, [Path] int characterId);

    [ComputeMethod]
    [Get(nameof(TryGetCharacter) + "/{region}/{realmSlug}/{characterName}")]
    Task<CharacterAccountViewModel> TryGetCharacter(Session session, [Path] BlizzardRegion region, [Path] string realmSlug, [Path] string characterName);

    [Post(nameof(TryEnqueueUpdate) + "/{region}/{realmSlug}/{characterName}")]
    Task<bool> TryEnqueueUpdate(Session session, [Path] BlizzardRegion region, [Path] string realmSlug, [Path] string characterName);

    [CommandHandler]
    [Post(nameof(TrySetCharacterDeleted))]
    Task<bool> TrySetCharacterDeleted([Body] Character_TrySetCharacterDeleted command, CancellationToken cancellationToken = default);

    [CommandHandler]
    [Post(nameof(TrySetCharacterRenamedOrTransferred))]
    Task<bool> TrySetCharacterRenamedOrTransferred([Body] Character_TrySetCharacterRenamedOrTransferred command, CancellationToken cancellationToken = default);
}