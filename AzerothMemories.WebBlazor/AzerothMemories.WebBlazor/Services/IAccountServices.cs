namespace AzerothMemories.WebBlazor.Services;

[BasePath("account")]
public interface IAccountServices
{
    [ComputeMethod]
    [Get(nameof(TryGetAccount))]
    Task<ActiveAccountViewModel> TryGetAccount(Session session, CancellationToken cancellationToken = default);

    [ComputeMethod]
    [Get(nameof(TryGetAccountById) + "/{accountId}")]
    Task<AccountViewModel> TryGetAccountById(Session session, [Path] long accountId, CancellationToken cancellationToken = default);

    [ComputeMethod]
    [Get(nameof(TryGetAccountByUsername) + "/{username}")]
    Task<AccountViewModel> TryGetAccountByUsername(Session session, [Path] string username, CancellationToken cancellationToken = default);

    [ComputeMethod]
    [Get(nameof(TryReserveUsername) + "/{username}")]
    Task<bool> TryReserveUsername(Session session, [Path] string username, CancellationToken cancellationToken = default);

    [Post(nameof(TryChangeUsername) + "/{newUsername}")]
    Task<bool> TryChangeUsername(Session session, [Path] string newUsername, CancellationToken cancellationToken = default);

    [Post(nameof(TryChangeIsPrivate) + "/{newValue}")]
    Task<bool> TryChangeIsPrivate(Session session, [Path] bool newValue, CancellationToken cancellationToken = default);

    [Post(nameof(TryChangeBattleTagVisibility) + "/{newValue}")]
    Task<bool> TryChangeBattleTagVisibility(Session session, [Path] bool newValue, CancellationToken cancellationToken = default);

    [Post(nameof(TryChangeAvatar) + "/{newValue}")]
    Task<string> TryChangeAvatar(Session session, [Path] string newValue);

    [Post(nameof(TryChangeSocialLink) + "/{linkId}/{newValue}")]
    Task<string> TryChangeSocialLink(Session session, [Path] int linkId, [Path] string newValue);

    [ComputeMethod]
    [Get(nameof(TryGetAchievementsByTime) + "/{timeStamp}/{diffInSeconds}")]
    Task<PostTagInfo[]> TryGetAchievementsByTime(Session session, [Path] long timeStamp, [Path] int diffInSeconds, [Query] string locale = null, CancellationToken cancellationToken = default);
}