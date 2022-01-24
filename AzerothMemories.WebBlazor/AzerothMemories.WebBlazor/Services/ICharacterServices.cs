namespace AzerothMemories.WebBlazor.Services;

[BasePath("character")]
public interface ICharacterServices
{
    //Task<bool> TryChangeUsername([Body] Account_TryChangeUsername command, CancellationToken cancellationToken = default);

    [CommandHandler]
    [Post(nameof(TryChangeCharacterAccountSync))]
    Task<bool> TryChangeCharacterAccountSync([Body] Character_TryChangeCharacterAccountSync command, CancellationToken cancellationToken = default);

    [ComputeMethod]
    [Get(nameof(TryGetCharacter) + "/{characterId}")]
    Task<CharacterAccountViewModel> TryGetCharacter(Session session, [Path] long characterId);

    [ComputeMethod]
    [Get(nameof(TryGetCharacter) + "/{region}/{realmSlug}/{characterName}")]
    Task<CharacterAccountViewModel> TryGetCharacter(Session session, [Path] BlizzardRegion region, [Path] string realmSlug, [Path] string characterName);

    //[CommandHandler]
    //[Post(nameof(TryEnqueueUpdate))]
    //Task<bool> TryEnqueueUpdate([Body] Character_TryEnqueueUpdate command, CancellationToken cancellationToken = default);

    [CommandHandler]
    [Post(nameof(TrySetCharacterDeleted))]
    Task<bool> TrySetCharacterDeleted([Body] Character_TrySetCharacterDeleted command, CancellationToken cancellationToken = default);

    [CommandHandler]
    [Post(nameof(TrySetCharacterRenamedOrTransferred))]
    Task<bool> TrySetCharacterRenamedOrTransferred([Body] Character_TrySetCharacterRenamedOrTransferred command, CancellationToken cancellationToken = default);
}