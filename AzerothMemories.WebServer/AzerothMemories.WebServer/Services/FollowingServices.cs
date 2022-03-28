using AzerothMemories.WebServer.Services.Handlers;

namespace AzerothMemories.WebServer.Services;

[RegisterComputeService]
[RegisterAlias(typeof(IFollowingServices))]
public class FollowingServices : DbServiceBase<AppDbContext>, IFollowingServices, IDatabaseContextProvider
{
    private readonly CommonServices _commonServices;

    public FollowingServices(IServiceProvider services, CommonServices commonServices) : base(services)
    {
        _commonServices = commonServices;
    }

    [ComputeMethod]
    public virtual async Task<Dictionary<int, AccountFollowingViewModel>> TryGetAccountFollowing(int accountId)
    {
        if (accountId == 0)
        {
            return new Dictionary<int, AccountFollowingViewModel>();
        }

        await using var database = CreateDbContext();

        var followingQuery = from record in database.AccountFollowing
                             where record.AccountId == accountId
                             from follower in database.Accounts.Where(r => record.FollowerId == r.Id)
                             select new AccountFollowingViewModel
                             {
                                 Id = record.Id,
                                 AccountId = record.AccountId,
                                 FollowerId = record.FollowerId,
                                 FollowerUsername = follower.Username,
                                 FollowerAvatarLink = follower.Avatar,
                                 Status = record.Status
                             };

        return await followingQuery.ToDictionaryAsync(x => x.FollowerId, x => x).ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<Dictionary<int, AccountFollowingViewModel>> TryGetAccountFollowers(int accountId)
    {
        if (accountId == 0)
        {
            return new Dictionary<int, AccountFollowingViewModel>();
        }

        await using var database = CreateDbContext();

        var followersQuery = from record in database.AccountFollowing
                             where record.FollowerId == accountId
                             from follower in database.Accounts.Where(r => record.AccountId == r.Id)
                             select new AccountFollowingViewModel
                             {
                                 Id = record.Id,
                                 AccountId = record.FollowerId,
                                 FollowerId = record.AccountId,
                                 FollowerUsername = follower.Username,
                                 FollowerAvatarLink = follower.Avatar,
                                 Status = record.Status
                             };

        return await followersQuery.ToDictionaryAsync(x => x.FollowerId, x => x).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<AccountFollowingStatus?> TryStartFollowing(Following_TryStartFollowing command, CancellationToken cancellationToken = default)
    {
        return await FollowingServices_TryStartFollowing.TryHandle(_commonServices, this, command).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<AccountFollowingStatus?> TryStopFollowing(Following_TryStopFollowing command, CancellationToken cancellationToken = default)
    {
        return await FollowingServices_TryStopFollowing.TryHandle(_commonServices, this, command).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<AccountFollowingStatus?> TryAcceptFollower(Following_TryAcceptFollower command, CancellationToken cancellationToken = default)
    {
        return await FollowingServices_TryAcceptFollower.TryHandle(_commonServices, this, command).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<AccountFollowingStatus?> TryRemoveFollower(Following_TryRemoveFollower command, CancellationToken cancellationToken = default)
    {
        return await FollowingServices_TryRemoveFollower.TryHandle(_commonServices, this, command).ConfigureAwait(false);
    }

    public void InvalidateFollowing(Following_InvalidateRecord record)
    {
        if (record == null)
        {
            return;
        }

        if (record.AccountId > 0)
        {
            _ = TryGetAccountFollowing(record.AccountId);
            _ = TryGetAccountFollowers(record.AccountId);
        }

        if (record.OtherAccountId > 0)
        {
            _ = TryGetAccountFollowing(record.OtherAccountId);
            _ = TryGetAccountFollowers(record.OtherAccountId);
        }
    }

    public Task<AppDbContext> CreateCommandDbContext()
    {
        return CreateCommandDbContext(true);
    }
}