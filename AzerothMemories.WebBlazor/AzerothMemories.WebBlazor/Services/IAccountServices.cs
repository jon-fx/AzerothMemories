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

    [Post(nameof(TryChangeUsername) + "/{newUsername}")]
    Task<bool> TryChangeUsername(Session session, [Path] string newUsername);

    [Post(nameof(TryChangeIsPrivate) + "/{newValue}")]
    Task<bool> TryChangeIsPrivate(Session session, [Path] bool newValue);

    [Post(nameof(TryChangeBattleTagVisibility) + "/{newValue}")]
    Task<bool> TryChangeBattleTagVisibility(Session session, [Path] bool newValue);

    [Post(nameof(TryChangeAvatar))]
    Task<string> TryChangeAvatar(Session session, [Body] StringBody stringBody);

    [Post(nameof(TryChangeSocialLink) + "/{linkId}")]
    Task<string> TryChangeSocialLink(Session session, [Path] int linkId, [Body] StringBody stringBody);

    [ComputeMethod]
    [Get(nameof(TryGetAchievementsByTime) + "/{timeStamp}/{diffInSeconds}")]
    Task<PostTagInfo[]> TryGetAchievementsByTime(Session session, [Path] long timeStamp, [Path] int diffInSeconds, [Query] string locale);

    [ComputeMethod]
    [Get(nameof(TryGetAccountHistory))]
    Task<AccountHistoryPageResult> TryGetAccountHistory(Session session, [Query] int currentPage = 0);
}