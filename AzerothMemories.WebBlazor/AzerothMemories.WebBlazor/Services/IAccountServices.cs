namespace AzerothMemories.WebBlazor.Services;

public interface IAccountServices : IComputeService
{
    [ComputeMethod]
    Task<AccountViewModel> TryGetActiveAccount(Session session);

    [ComputeMethod]
    Task<AccountViewModel> TryGetAccountById(Session session, int accountId);

    [ComputeMethod]
    Task<AccountViewModel> TryGetAccountByUsername(Session session, string username);

    Task<bool> TryEnqueueUpdate(Session session);

    [ComputeMethod]
    Task<bool> CheckIsValidUsername(Session session, string username);

    [CommandHandler]
    Task<bool> TryChangeUsername(Account_TryChangeUsername command, CancellationToken cancellationToken = default);

    [CommandHandler]
    Task<bool> TryChangeIsPrivate(Account_TryChangeIsPrivate command, CancellationToken cancellationToken = default);

    [CommandHandler]
    Task<bool> TryChangeBattleTagVisibility(Account_TryChangeBattleTagVisibility command, CancellationToken cancellationToken = default);

    [CommandHandler]
    Task<string> TryChangeAvatar(Account_TryChangeAvatar command, CancellationToken cancellationToken = default);

    [CommandHandler]
    Task<string> TryChangeAvatarUpload(Account_TryChangeAvatarUpload command, CancellationToken cancellationToken = default);

    [CommandHandler]
    Task<string> TryChangeSocialLink(Account_TryChangeSocialLink command, CancellationToken cancellationToken = default);

    [CommandHandler]
    Task<bool> TryDisconnectAccount(Account_TryDisconnectAccount command, CancellationToken cancellationToken = default);

    [ComputeMethod]
    Task<PostTagInfo[]> TryGetAchievementsByTime(Session session, long timeStamp, int diffInSeconds, ServerSideLocale locale);

    [ComputeMethod]
    Task<AccountHistoryPageResult> TryGetAccountHistory(Session session, int currentPage = 0);
}