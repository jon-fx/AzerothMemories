﻿namespace AzerothMemories.WebServer.Services.Handlers;

internal static class FollowingServices_TryRemoveFollower
{
    public static async Task<AccountFollowingStatus?> TryHandle(ILogger<FollowingServices> services, CommonServices commonServices, Following_TryRemoveFollower command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Following_InvalidateRecord>();
            commonServices.FollowingServices.InvalidateFollowing(invRecord);

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

        var followersViewModels = await commonServices.FollowingServices.TryGetAccountFollowers(activeAccount.Id).ConfigureAwait(false);
        if (!followersViewModels.TryGetValue(otherAccountId, out var viewModel))
        {
            return null;
        }

        await using var database = await commonServices.DatabaseHub.CreateCommandDbContext(cancellationToken).ConfigureAwait(false);

        var currentRecord = await database.AccountFollowing.FirstAsync(x => x.Id == viewModel.Id, cancellationToken).ConfigureAwait(false);
        currentRecord.Status = viewModel.Status = AccountFollowingStatus.None;
        currentRecord.LastUpdateTime = SystemClock.Instance.GetCurrentInstant();

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await commonServices.Commander.Call(new Account_AddNewHistoryItem
        {
            AccountId = activeAccount.Id,
            OtherAccountId = otherAccountId,
            Type = AccountHistoryType.FollowerRemoved
        }, cancellationToken).ConfigureAwait(false);

        context.Operation().Items.Set(new Following_InvalidateRecord(activeAccount.Id, otherAccountId));

        return viewModel.Status;
    }
}