namespace AzerothMemories.WebBlazor.Services;

[BasePath("character")]
public interface ICharacterServices
{
    //[ComputeMethod] [Get(nameof(TryGetAccount) + "/{accountId}")] Task<AccountViewModel> TryGetAccount(Session session, [Path] long accountId, CancellationToken cancellationToken = default);

    //[Post(nameof(TryChangeUsername) + "/{newUsername}")] Task<string> TryChangeUsername(Session session, [Path] string newUsername, CancellationToken cancellationToken = default);

    [Post(nameof(TryChangeCharacterAccountSync) + "/{characterId}/{newValue}")]
    Task<bool> TryChangeCharacterAccountSync(Session session, [Path] long characterId, [Path] bool newValue, CancellationToken cancellationToken = default);
}