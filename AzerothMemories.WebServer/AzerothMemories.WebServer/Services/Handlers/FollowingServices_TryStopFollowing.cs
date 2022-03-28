namespace AzerothMemories.WebServer.Services.Handlers;

internal static class FollowingServices_TryStopFollowing
{
    public static async Task<AccountFollowingStatus?> TryHandle(CommonServices commonServices, IDatabaseContextProvider databaseContextProvider, Following_TryStopFollowing command)
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

        var followingViewModels = await commonServices.FollowingServices.TryGetAccountFollowing(activeAccount.Id).ConfigureAwait(false);
        if (!followingViewModels.TryGetValue(otherAccountId, out var viewModel))
        {
            return null;
        }

        await using var database = await databaseContextProvider.CreateCommandDbContext().ConfigureAwait(false);

        var currentRecord = await database.AccountFollowing.FirstAsync(x => x.Id == viewModel.Id).ConfigureAwait(false);
        currentRecord.Status = viewModel.Status = AccountFollowingStatus.None;
        currentRecord.LastUpdateTime = SystemClock.Instance.GetCurrentInstant();

        await database.SaveChangesAsync().ConfigureAwait(false);

        await commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
        {
            AccountId = activeAccount.Id,
            OtherAccountId = otherAccountId,
            //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.StoppedFollowing
        }).ConfigureAwait(false);

        context.Operation().Items.Set(new Following_InvalidateRecord(activeAccount.Id, otherAccountId));

        return viewModel.Status;
    }
}