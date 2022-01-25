namespace AzerothMemories.WebServer.Services;

[RegisterComputeService]
[RegisterAlias(typeof(IFollowingServices))]
public class FollowingServices : IFollowingServices
{
    private readonly CommonServices _commonServices;

    public FollowingServices(CommonServices commonServices)
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

    [CommandHandler]
    public virtual async Task<AccountFollowingStatus?> TryStartFollowing(Following_TryStartFollowing command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Following_InvalidateRecord>();
            InvalidateFollowing(invRecord);

            return default;
        }

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session);
        if (activeAccount == null)
        {
            return null;
        }

        if (!activeAccount.CanInteract)
        {
            return null;
        }

        var otherAccountId = command.OtherAccountId;
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
            }, token: cancellationToken);

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

            followingViewModels[otherAccountId] = viewModel = await followingViewModelQuery.FirstAsync(cancellationToken);
        }

        var status = AccountFollowingStatus.Active;
        if (otherAccountViewModel.IsPrivate)
        {
            status = AccountFollowingStatus.Pending;
        }

        viewModel.Status = status;

        await database.AccountFollowing.Where(x => x.Id == viewModel.Id).Set(x => x.Status, viewModel.Status).Set(x => x.LastUpdateTime, SystemClock.Instance.GetCurrentInstant()).UpdateAsync(cancellationToken);

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
            TargetId = 1,
            OtherAccountId = activeAccount.Id,
            CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = viewModel.Status == AccountFollowingStatus.Active ? AccountHistoryType.StartedFollowing : AccountHistoryType.FollowingRequestReceived
        });

        transaction.Complete();

        context.Operation().Items.Set(new Following_InvalidateRecord(activeAccount.Id, otherAccountId));

        return viewModel.Status;
    }

    [CommandHandler]
    public virtual async Task<AccountFollowingStatus?> TryStopFollowing(Following_TryStopFollowing command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Following_InvalidateRecord>();
            InvalidateFollowing(invRecord);

            return default;
        }

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session);
        if (activeAccount == null)
        {
            return null;
        }

        if (!activeAccount.CanInteract)
        {
            return null;
        }

        var otherAccountId = command.OtherAccountId;
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

        await database.AccountFollowing.Where(x => x.Id == viewModel.Id).Set(x => x.Status, viewModel.Status).Set(x => x.LastUpdateTime, SystemClock.Instance.GetCurrentInstant()).UpdateAsync(cancellationToken);

        await _commonServices.AccountServices.TestingHistory(database, new AccountHistoryRecord
        {
            AccountId = activeAccount.Id,
            OtherAccountId = otherAccountId,
            CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.StoppedFollowing
        });

        transaction.Complete();

        context.Operation().Items.Set(new Following_InvalidateRecord(activeAccount.Id, otherAccountId));

        return viewModel.Status;
    }

    [CommandHandler]
    public virtual async Task<AccountFollowingStatus?> TryAcceptFollower(Following_TryAcceptFollower command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Following_InvalidateRecord>();
            InvalidateFollowing(invRecord);

            return default;
        }

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session);
        if (activeAccount == null)
        {
            return null;
        }

        if (!activeAccount.CanInteract)
        {
            return null;
        }

        var otherAccountId = command.OtherAccountId;
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

        await database.AccountFollowing.Where(x => x.Id == viewModel.Id).Set(x => x.Status, viewModel.Status).Set(x => x.LastUpdateTime, SystemClock.Instance.GetCurrentInstant()).UpdateAsync(cancellationToken);

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

        context.Operation().Items.Set(new Following_InvalidateRecord(activeAccount.Id, otherAccountId));

        return viewModel.Status;
    }

    [CommandHandler]
    public virtual async Task<AccountFollowingStatus?> TryRemoveFollower(Following_TryRemoveFollower command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Following_InvalidateRecord>();
            InvalidateFollowing(invRecord);

            return default;
        }

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session);
        if (activeAccount == null)
        {
            return null;
        }

        if (!activeAccount.CanInteract)
        {
            return null;
        }

        var otherAccountId = command.OtherAccountId;
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

        await database.AccountFollowing.Where(x => x.Id == viewModel.Id).Set(x => x.Status, viewModel.Status).Set(x => x.LastUpdateTime, SystemClock.Instance.GetCurrentInstant()).UpdateAsync(cancellationToken);

        await _commonServices.AccountServices.TestingHistory(database, new AccountHistoryRecord
        {
            AccountId = activeAccount.Id,
            OtherAccountId = otherAccountId,
            CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.FollowerRemoved
        });

        transaction.Complete();

        context.Operation().Items.Set(new Following_InvalidateRecord(activeAccount.Id, otherAccountId));

        return viewModel.Status;
    }

    private void InvalidateFollowing(Following_InvalidateRecord record)
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
}