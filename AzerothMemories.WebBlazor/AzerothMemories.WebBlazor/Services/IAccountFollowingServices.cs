namespace AzerothMemories.WebBlazor.Services;

[BasePath("accountfollowing")]
public interface IAccountFollowingServices
{
    [Post(nameof(TryStartFollowing) + "/{otherAccountId}")]
    Task<AccountFollowingStatus?> TryStartFollowing(Session session, [Path] long otherAccountId);

    [Post(nameof(TryStopFollowing) + "/{otherAccountId}")]
    Task<AccountFollowingStatus?> TryStopFollowing(Session session, [Path] long otherAccountId);

    [Post(nameof(TryAcceptFollower) + "/{otherAccountId}")]
    Task<AccountFollowingStatus?> TryAcceptFollower(Session session, [Path] long otherAccountId);

    [Post(nameof(TryRemoveFollower) + "/{otherAccountId}")]
    Task<AccountFollowingStatus?> TryRemoveFollower(Session session, [Path] long otherAccountId);
}