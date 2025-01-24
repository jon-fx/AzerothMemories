namespace AzerothMemories.WebBlazor.Services;

public interface ICharacterServices : IComputeService
{
    [CommandHandler]
    Task<bool> TryChangeCharacterAccountSync(Character_TryChangeCharacterAccountSync command, CancellationToken cancellationToken = default);

    [ComputeMethod]
    Task<CharacterAccountViewModel> TryGetCharacter(Session session, int characterId, bool enqueueUpdate);

    [ComputeMethod]
    Task<CharacterAccountViewModel?> TryGetCharacter(Session session, BlizzardRegion region, string realmSlug, string characterName, bool enqueueUpdate);

    //Task<bool> TryEnqueueUpdate(Session session, BlizzardRegion region, string realmSlug, string characterName);

    [CommandHandler]
    Task<bool> TrySetCharacterDeleted(Character_TrySetCharacterDeleted command, CancellationToken cancellationToken = default);

    [CommandHandler]
    Task<bool> TrySetCharacterRenamedOrTransferred(Character_TrySetCharacterRenamedOrTransferred command, CancellationToken cancellationToken = default);
}