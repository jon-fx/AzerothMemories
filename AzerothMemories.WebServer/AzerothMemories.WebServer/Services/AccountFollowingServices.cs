namespace AzerothMemories.WebServer.Services;

[RegisterComputeService]
[RegisterAlias(typeof(IAccountFollowingServices))]
public class AccountFollowingServices : IAccountFollowingServices
{
    private readonly CommonServices _commonServices;

    public AccountFollowingServices(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    [ComputeMethod]
    public virtual async Task<Dictionary<long, AccountFollowingViewModel>> TryGetAccountFollowing(long accountId)
    {
        if (accountId == 0)
        {
            return null;
        }

        await using var database = _commonServices.DatabaseProvider.GetDatabase();

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

        return await followingQuery.ToDictionaryAsync(x => x.FollowerId, x => x);
    }

    [ComputeMethod]
    public virtual async Task<Dictionary<long, AccountFollowingViewModel>> TryGetAccountFollowers(long accountId)
    {
        if (accountId == 0)
        {
            return null;
        }

        await using var database = _commonServices.DatabaseProvider.GetDatabase();

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

        return await followersQuery.ToDictionaryAsync(x => x.FollowerId, x => x);
    }

    public async Task<AccountFollowingStatus?> TryStartFollowing(Session session, long otherAccountId)
    {
        var activeAccountId = await _commonServices.AccountServices.TryGetActiveAccountId(session);
        if (activeAccountId == 0)
        {
            return null;
        }

        if (activeAccountId == otherAccountId)
        {
            return null;
        }

        var followingViewModels = await TryGetAccountFollowing(activeAccountId);
        if (followingViewModels == null)
        {
            return null;
        }

        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        var otherAccountViewModel = await _commonServices.AccountServices.TryGetAccountRecord(otherAccountId);
        if (otherAccountViewModel == null)
        {
            return null;
        }

        if (!followingViewModels.TryGetValue(otherAccountId, out var viewModel))
        {
            var recordId = await database.InsertWithInt64IdentityAsync(new AccountFollowingRecord
            {
                AccountId = activeAccountId,
                FollowerId = otherAccountId,
                LastUpdateTime = SystemClock.Instance.GetCurrentInstant(),
                CreatedTime = SystemClock.Instance.GetCurrentInstant()
            });

            var followingViewModelQuery = from record in database.AccountFollowing
                                          where record.Id == recordId
                                          from follower in database.Accounts.Where(r => record.AccountId == r.Id)
                                          select new AccountFollowingViewModel
                                          {
                                              Id = record.Id,
                                              AccountId = record.AccountId,
                                              FollowerId = record.FollowerId,
                                              FollowerUsername = follower.Username,
                                              FollowerAvatarLink = follower.Avatar,
                                              Status = record.Status
                                          };

            followingViewModels[otherAccountId] = viewModel = await followingViewModelQuery.FirstAsync();
        }

        var status = AccountFollowingStatus.Active;
        if (otherAccountViewModel.IsPrivate)
        {
            status = AccountFollowingStatus.Pending;
        }

        viewModel.Status = status;

        await database.AccountFollowing.Where(x => x.Id == viewModel.Id).Set(x => x.Status, viewModel.Status).Set(x => x.LastUpdateTime, SystemClock.Instance.GetCurrentInstant()).UpdateAsync();

        await _commonServices.AccountServices.TestingHistory(database, new AccountHistoryRecord
        {
            AccountId = activeAccountId,
            OtherAccountId = otherAccountId,
            CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = viewModel.Status == AccountFollowingStatus.Active ? AccountHistoryType.StartedFollowing : AccountHistoryType.FollowingRequestSent
        });

        await _commonServices.AccountServices.TestingHistory(database, new AccountHistoryRecord
        {
            AccountId = otherAccountId,
            OtherAccountId = activeAccountId,
            CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = viewModel.Status == AccountFollowingStatus.Active ? AccountHistoryType.StartedFollowing : AccountHistoryType.FollowingRequestReceived
        });

        InvalidateFollowing(activeAccountId, otherAccountId);

        return viewModel.Status;
    }

    public async Task<AccountFollowingStatus?> TryStopFollowing(Session session, long otherAccountId)
    {
        var activeAccountId = await _commonServices.AccountServices.TryGetActiveAccountId(session);
        if (activeAccountId == 0)
        {
            return null;
        }

        if (activeAccountId == otherAccountId)
        {
            return null;
        }

        var followingViewModels = await TryGetAccountFollowing(activeAccountId);
        if (followingViewModels == null)
        {
            return null;
        }

        if (!followingViewModels.TryGetValue(otherAccountId, out var viewModel))
        {
            return null;
        }

        viewModel.Status = AccountFollowingStatus.None;

        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        await database.AccountFollowing.Where(x => x.Id == viewModel.Id).Set(x => x.Status, viewModel.Status).Set(x => x.LastUpdateTime, SystemClock.Instance.GetCurrentInstant()).UpdateAsync();

        await _commonServices.AccountServices.TestingHistory(database, new AccountHistoryRecord
        {
            AccountId = activeAccountId,
            OtherAccountId = otherAccountId,
            CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.StoppedFollowing
        });

        InvalidateFollowing(activeAccountId, otherAccountId);

        return viewModel.Status;
    }

    public async Task<AccountFollowingStatus?> TryAcceptFollower(Session session, long otherAccountId)
    {
        var activeAccountId = await _commonServices.AccountServices.TryGetActiveAccountId(session);
        if (activeAccountId == 0)
        {
            return null;
        }

        if (activeAccountId == otherAccountId)
        {
            return null;
        }

        var followersViewModels = await TryGetAccountFollowers(activeAccountId);
        if (followersViewModels == null)
        {
            return null;
        }

        if (!followersViewModels.TryGetValue(otherAccountId, out var viewModel))
        {
            return null;
        }

        viewModel.Status = AccountFollowingStatus.Active;

        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        await database.AccountFollowing.Where(x => x.Id == viewModel.Id).Set(x => x.Status, viewModel.Status).Set(x => x.LastUpdateTime, SystemClock.Instance.GetCurrentInstant()).UpdateAsync();

        await _commonServices.AccountServices.TestingHistory(database, new AccountHistoryRecord
        {
            AccountId = activeAccountId,
            OtherAccountId = otherAccountId,
            CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.FollowingRequestAccepted1
        });

        await _commonServices.AccountServices.TestingHistory(database, new AccountHistoryRecord
        {
            AccountId = otherAccountId,
            OtherAccountId = activeAccountId,
            CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.FollowingRequestAccepted2
        });

        InvalidateFollowing(activeAccountId, otherAccountId);

        return viewModel.Status;
    }

    public async Task<AccountFollowingStatus?> TryRemoveFollower(Session session, long otherAccountId)
    {
        var activeAccountId = await _commonServices.AccountServices.TryGetActiveAccountId(session);
        if (activeAccountId == 0)
        {
            return null;
        }

        if (activeAccountId == otherAccountId)
        {
            return null;
        }

        var followersViewModels = await TryGetAccountFollowers(activeAccountId);
        if (followersViewModels == null)
        {
            return null;
        }

        if (!followersViewModels.TryGetValue(otherAccountId, out var viewModel))
        {
            return null;
        }

        viewModel.Status = AccountFollowingStatus.None;

        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        await database.AccountFollowing.Where(x => x.Id == viewModel.Id).Set(x => x.Status, viewModel.Status).Set(x => x.LastUpdateTime, SystemClock.Instance.GetCurrentInstant()).UpdateAsync();

        await _commonServices.AccountServices.TestingHistory(database, new AccountHistoryRecord
        {
            AccountId = activeAccountId,
            OtherAccountId = otherAccountId,
            CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.FollowerRemoved
        });

        InvalidateFollowing(activeAccountId, otherAccountId);

        return viewModel.Status;
    }

    private void InvalidateFollowing(long activeAccountId, long otherAccountId)
    {
        using var computed = Computed.Invalidate();
        _ = TryGetAccountFollowing(activeAccountId);
        _ = TryGetAccountFollowers(activeAccountId);
        _ = TryGetAccountFollowing(otherAccountId);
        _ = TryGetAccountFollowers(otherAccountId);
    }
}