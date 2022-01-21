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
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session);
        if (activeAccount == null)
        {
            return null;
        }

        if (!activeAccount.CanInteract)
        {
            return null;
        }

        if (activeAccount.Id == otherAccountId)
        {
            return null;
        }

        var followingViewModels = await TryGetAccountFollowing(activeAccount.Id);
        if (followingViewModels == null)
        {
            return null;
        }

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
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
                AccountId = activeAccount.Id,
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
            AccountId = activeAccount.Id,
            OtherAccountId = otherAccountId,
            CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = viewModel.Status == AccountFollowingStatus.Active ? AccountHistoryType.StartedFollowing : AccountHistoryType.FollowingRequestSent
        });

        await _commonServices.AccountServices.TestingHistory(database, new AccountHistoryRecord
        {
            AccountId = otherAccountId,
            OtherAccountId = activeAccount.Id,
            CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = viewModel.Status == AccountFollowingStatus.Active ? AccountHistoryType.StartedFollowing : AccountHistoryType.FollowingRequestReceived
        });

        transaction.Complete();

        InvalidateFollowing(activeAccount.Id, otherAccountId);

        return viewModel.Status;
    }

    public async Task<AccountFollowingStatus?> TryStopFollowing(Session session, long otherAccountId)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session);
        if (activeAccount == null)
        {
            return null;
        }

        if (!activeAccount.CanInteract)
        {
            return null;
        }

        if (activeAccount.Id == otherAccountId)
        {
            return null;
        }

        var followingViewModels = await TryGetAccountFollowing(activeAccount.Id);
        if (followingViewModels == null)
        {
            return null;
        }

        if (!followingViewModels.TryGetValue(otherAccountId, out var viewModel))
        {
            return null;
        }

        viewModel.Status = AccountFollowingStatus.None;

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        await database.AccountFollowing.Where(x => x.Id == viewModel.Id).Set(x => x.Status, viewModel.Status).Set(x => x.LastUpdateTime, SystemClock.Instance.GetCurrentInstant()).UpdateAsync();

        await _commonServices.AccountServices.TestingHistory(database, new AccountHistoryRecord
        {
            AccountId = activeAccount.Id,
            OtherAccountId = otherAccountId,
            CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.StoppedFollowing
        });

        transaction.Complete();

        InvalidateFollowing(activeAccount.Id, otherAccountId);

        return viewModel.Status;
    }

    public async Task<AccountFollowingStatus?> TryAcceptFollower(Session session, long otherAccountId)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session);
        if (activeAccount == null)
        {
            return null;
        }

        if (!activeAccount.CanInteract)
        {
            return null;
        }

        if (activeAccount.Id == otherAccountId)
        {
            return null;
        }

        var followersViewModels = await TryGetAccountFollowers(activeAccount.Id);
        if (followersViewModels == null)
        {
            return null;
        }

        if (!followersViewModels.TryGetValue(otherAccountId, out var viewModel))
        {
            return null;
        }

        viewModel.Status = AccountFollowingStatus.Active;

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        await database.AccountFollowing.Where(x => x.Id == viewModel.Id).Set(x => x.Status, viewModel.Status).Set(x => x.LastUpdateTime, SystemClock.Instance.GetCurrentInstant()).UpdateAsync();

        await _commonServices.AccountServices.TestingHistory(database, new AccountHistoryRecord
        {
            AccountId = activeAccount.Id,
            OtherAccountId = otherAccountId,
            CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.FollowingRequestAccepted1
        });

        await _commonServices.AccountServices.TestingHistory(database, new AccountHistoryRecord
        {
            AccountId = otherAccountId,
            OtherAccountId = activeAccount.Id,
            CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.FollowingRequestAccepted2
        });

        transaction.Complete();

        InvalidateFollowing(activeAccount.Id, otherAccountId);

        return viewModel.Status;
    }

    public async Task<AccountFollowingStatus?> TryRemoveFollower(Session session, long otherAccountId)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session);
        if (activeAccount == null)
        {
            return null;
        }

        if (!activeAccount.CanInteract)
        {
            return null;
        }

        if (activeAccount.Id == otherAccountId)
        {
            return null;
        }

        var followersViewModels = await TryGetAccountFollowers(activeAccount.Id);
        if (followersViewModels == null)
        {
            return null;
        }

        if (!followersViewModels.TryGetValue(otherAccountId, out var viewModel))
        {
            return null;
        }

        viewModel.Status = AccountFollowingStatus.None;

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        await database.AccountFollowing.Where(x => x.Id == viewModel.Id).Set(x => x.Status, viewModel.Status).Set(x => x.LastUpdateTime, SystemClock.Instance.GetCurrentInstant()).UpdateAsync();

        await _commonServices.AccountServices.TestingHistory(database, new AccountHistoryRecord
        {
            AccountId = activeAccount.Id,
            OtherAccountId = otherAccountId,
            CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.FollowerRemoved
        });

        transaction.Complete();

        InvalidateFollowing(activeAccount.Id, otherAccountId);

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