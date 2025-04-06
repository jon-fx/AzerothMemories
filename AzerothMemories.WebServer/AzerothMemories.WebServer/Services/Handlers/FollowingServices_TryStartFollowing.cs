using ActualLab.Collections;

namespace AzerothMemories.WebServer.Services.Handlers;

internal static class FollowingServices_TryStartFollowing
{
    public static async Task<AccountFollowingStatus?> TryHandle(ILogger<FollowingServices> logger, CommonServices commonServices, Following_TryStartFollowing command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        if (Invalidation.IsActive)
        {
            if (context.Operation.Items.KeylessTryGet(out Following_InvalidateRecord? invRecord) && invRecord != null)
            {
                commonServices.FollowingServices.InvalidateFollowing(invRecord);
            }

            return default;
        }

        var activeAccount = await commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return null;
        }

        if (!activeAccount.CanChangeFollowing())
        {
            return null;
        }

        var otherAccountId = command.OtherAccountId;
        if (activeAccount.Id == otherAccountId)
        {
            return null;
        }

        var followingViewModels = await commonServices.FollowingServices.TryGetAccountFollowing(activeAccount.Id).ConfigureAwait(false);
        await using var database = await commonServices.DatabaseHub.CreateOperationDbContext(cancellationToken).ConfigureAwait(false);

        var otherAccountViewModel = await commonServices.AccountServices.TryGetAccountRecord(otherAccountId).ConfigureAwait(false);
        if (otherAccountViewModel == null)
        {
            return null;
        }

        if (!followingViewModels.TryGetValue(otherAccountId, out var viewModel))
        {
            var temp = new AccountFollowingRecord
            {
                AccountId = activeAccount.Id,
                FollowerId = otherAccountId,
                LastUpdateTime = SystemClock.Instance.GetCurrentInstant(),
                CreatedTime = SystemClock.Instance.GetCurrentInstant()
            };

            await database.AccountFollowing.AddAsync(temp, cancellationToken).ConfigureAwait(false);
            await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var followingViewModelQuery = from record in database.AccountFollowing
                                          where record.Id == temp.Id
                                          from follower in database.Accounts.Where(r => record.AccountId == r.Id)
                                          select new AccountFollowingViewModel
                                          {
                                              Id = record.Id,
                                              AccountId = record.AccountId,
                                              FollowerId = record.FollowerId,
                                              FollowerUsername = follower.GetUsernameSafe(),
                                              FollowerAvatarLink = follower.Avatar,
                                              Status = record.Status
                                          };

            followingViewModels[otherAccountId] = viewModel = await followingViewModelQuery.FirstAsync(cancellationToken).ConfigureAwait(false);
        }

        var status = AccountFollowingStatus.Active;
        if (otherAccountViewModel.IsPrivate)
        {
            status = AccountFollowingStatus.Pending;
        }

        var currentRecord = await database.AccountFollowing.FirstAsync(x => x.Id == viewModel.Id, cancellationToken).ConfigureAwait(false);
        currentRecord.Status = viewModel.Status = status;
        currentRecord.LastUpdateTime = SystemClock.Instance.GetCurrentInstant();

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await commonServices.Commander.Call(new Account_AddNewHistoryItem
        {
            AccountId = activeAccount.Id,
            OtherAccountId = otherAccountId,
            Type = viewModel.Status == AccountFollowingStatus.Active ? AccountHistoryType.StartedFollowing : AccountHistoryType.FollowingRequestSent
        }, cancellationToken).ConfigureAwait(false);

        await commonServices.Commander.Call(new Account_AddNewHistoryItem
        {
            AccountId = otherAccountId,
            TargetId = 1,
            OtherAccountId = activeAccount.Id,
            Type = viewModel.Status == AccountFollowingStatus.Active ? AccountHistoryType.StartedFollowing : AccountHistoryType.FollowingRequestReceived
        }, cancellationToken).ConfigureAwait(false);

        context.Operation.Items.KeylessSet(new Following_InvalidateRecord(activeAccount.Id, otherAccountId));

        return viewModel.Status;
    }
}