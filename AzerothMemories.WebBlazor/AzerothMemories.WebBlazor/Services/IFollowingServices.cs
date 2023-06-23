namespace AzerothMemories.WebBlazor.Services;

public interface IFollowingServices : IComputeService
{
    [CommandHandler]
    Task<AccountFollowingStatus?> TryStartFollowing(Following_TryStartFollowing command, CancellationToken cancellationToken = default);

    [CommandHandler]
    Task<AccountFollowingStatus?> TryStopFollowing(Following_TryStopFollowing command, CancellationToken cancellationToken = default);

    [CommandHandler]
    Task<AccountFollowingStatus?> TryAcceptFollower(Following_TryAcceptFollower command, CancellationToken cancellationToken = default);

    [CommandHandler]
    Task<AccountFollowingStatus?> TryRemoveFollower(Following_TryRemoveFollower command, CancellationToken cancellationToken = default);
}