namespace AzerothMemories.WebBlazor.Services;

[BasePath("following")]
public interface IFollowingServices : IComputeService
{
    [CommandHandler]
    [Post(nameof(TryStartFollowing))]
    Task<AccountFollowingStatus?> TryStartFollowing([Body] Following_TryStartFollowing command, CancellationToken cancellationToken = default);

    [CommandHandler]
    [Post(nameof(TryStopFollowing))]
    Task<AccountFollowingStatus?> TryStopFollowing([Body] Following_TryStopFollowing command, CancellationToken cancellationToken = default);

    [CommandHandler]
    [Post(nameof(TryAcceptFollower))]
    Task<AccountFollowingStatus?> TryAcceptFollower([Body] Following_TryAcceptFollower command, CancellationToken cancellationToken = default);

    [CommandHandler]
    [Post(nameof(TryRemoveFollower))]
    Task<AccountFollowingStatus?> TryRemoveFollower([Body] Following_TryRemoveFollower command, CancellationToken cancellationToken = default);
}