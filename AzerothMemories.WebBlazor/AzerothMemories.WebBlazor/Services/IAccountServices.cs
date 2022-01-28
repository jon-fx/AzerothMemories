namespace AzerothMemories.WebBlazor.Services;

[BasePath("account")]
public interface IAccountServices
{
    [ComputeMethod]
    [Get(nameof(TryGetActiveAccount))]
    Task<AccountViewModel> TryGetActiveAccount(Session session);

    [ComputeMethod]
    [Get(nameof(TryGetAccountById) + "/{accountId}")]
    Task<AccountViewModel> TryGetAccountById(Session session, [Path] long accountId);

    [ComputeMethod]
    [Get(nameof(TryGetAccountByUsername) + "/{username}")]
    Task<AccountViewModel> TryGetAccountByUsername(Session session, [Path] string username);

    [Post(nameof(TryEnqueueUpdate))]
    Task<bool> TryEnqueueUpdate(Session session);

    [ComputeMethod]
    [Get(nameof(CheckIsValidUsername) + "/{username}")]
    Task<bool> CheckIsValidUsername(Session session, [Path] string username);

    [CommandHandler]
    [Post(nameof(TryChangeUsername))]
    Task<bool> TryChangeUsername([Body] Account_TryChangeUsername command, CancellationToken cancellationToken = default);

    [CommandHandler]
    [Post(nameof(TryChangeIsPrivate))]
    Task<bool> TryChangeIsPrivate([Body] Account_TryChangeIsPrivate command, CancellationToken cancellationToken = default);

    [CommandHandler]
    [Post(nameof(TryChangeBattleTagVisibility))]
    Task<bool> TryChangeBattleTagVisibility([Body] Account_TryChangeBattleTagVisibility command, CancellationToken cancellationToken = default);

    [CommandHandler]
    [Post(nameof(TryChangeAvatar))]
    Task<string> TryChangeAvatar([Body] Account_TryChangeAvatar command, CancellationToken cancellationToken = default);

    [CommandHandler]
    [Post(nameof(TryChangeSocialLink))]
    Task<string> TryChangeSocialLink([Body] Account_TryChangeSocialLink command, CancellationToken cancellationToken = default);

    [ComputeMethod]
    [Get(nameof(TryGetAchievementsByTime) + "/{timeStamp}/{diffInSeconds}")]
    Task<PostTagInfo[]> TryGetAchievementsByTime(Session session, [Path] long timeStamp, [Path] int diffInSeconds, [Query] string locale);

    [ComputeMethod]
    [Get(nameof(TryGetAccountHistory))]
    Task<AccountHistoryPageResult> TryGetAccountHistory(Session session, [Query] int currentPage = 0);
}