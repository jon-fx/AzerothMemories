using Stl.Fusion;
using Stl.Fusion.Authentication;

namespace AzerothMemories.Services;

[BasePath("account")]
public interface IAccountServices
{
    [ComputeMethod] [Get(nameof(TryGetAccount))] Task<ActiveAccountViewModel> TryGetAccount(Session session, CancellationToken cancellationToken = default);

    [ComputeMethod] [Get(nameof(TryGetAccount) + "/{accountId}")] Task<AccountViewModel> TryGetAccount(Session session, [Path] long accountId, CancellationToken cancellationToken = default);

    [ComputeMethod] [Get(nameof(TryGetAccount) + "/{username}")] Task<AccountViewModel> TryGetAccount(Session session, [Path] string username, CancellationToken cancellationToken = default);

    //[Post(nameof(TryChangeUsername) + "/{newUsername}")] Task<string> TryChangeUsername(Session session, [Path] string newUsername, CancellationToken cancellationToken = default);

    [ComputeMethod, Get(nameof(TryGetAchievementsByTime) + "/{timeStamp}/{diffInSeconds}")]
    Task<PostTagInfo[]> TryGetAchievementsByTime(Session session, [Path] long timeStamp, [Path] int diffInSeconds, CancellationToken cancellationToken = default);
}