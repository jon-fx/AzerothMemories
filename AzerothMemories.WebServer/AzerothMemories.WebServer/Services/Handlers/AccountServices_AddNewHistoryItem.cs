namespace AzerothMemories.WebServer.Services.Handlers;

internal static class AccountServices_AddNewHistoryItem
{
    public static async Task<bool> TryHandle(CommonServices commonServices, IDatabaseContextProvider databaseContextProvider, Account_AddNewHistoryItem command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Account_InvalidateFollowing>();
            if (invRecord != null)
            {
                _ = commonServices.AccountServices.TryGetAccountHistory(invRecord.AccountId, invRecord.Page);
            }

            return default;
        }

        if (command.AccountId == 0)
        {
            throw new NotImplementedException();
        }

        await using var database = await databaseContextProvider.CreateCommandDbContextNow(cancellationToken).ConfigureAwait(false);
        var query = from r in database.AccountHistory
                    where r.AccountId == command.AccountId &&
                          r.OtherAccountId == command.OtherAccountId &&
                          r.Type == command.Type &&
                          r.TargetId == command.TargetId &&
                          r.TargetPostId == command.TargetPostId &&
                          r.TargetCommentId == command.TargetCommentId
                    select r;

        var record = await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (record == null)
        {
            record = new AccountHistoryRecord();

            database.AccountHistory.Add(record);
        }

        record.Type = command.Type;
        record.AccountId = command.AccountId;
        record.OtherAccountId = command.OtherAccountId;
        record.TargetId = command.TargetId;
        record.TargetPostId = command.TargetPostId;
        record.TargetCommentId = command.TargetCommentId;
        record.CreatedTime = SystemClock.Instance.GetCurrentInstant();

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        context.Operation().Items.Set(new Account_InvalidateFollowing(record.AccountId, 1));

        return true;
    }
}