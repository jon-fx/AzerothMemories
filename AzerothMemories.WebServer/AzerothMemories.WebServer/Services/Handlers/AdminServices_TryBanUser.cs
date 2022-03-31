namespace AzerothMemories.WebServer.Services.Handlers;

internal static class AdminServices_TryBanUser
{
    public static async Task<bool> TryHandle(CommonServices commonServices, IDatabaseContextProvider databaseContextProvider, Admin_TryBanUser command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Account_InvalidateAccountRecord>();
            if (invRecord != null)
            {
                _ = commonServices.AccountServices.DependsOnAccountRecord(invRecord.Id);
            }

            return default;
        }

        var reason = command.BanReason;
        if (string.IsNullOrWhiteSpace(reason))
        {
        }
        else if (reason.Length > 200)
        {
            return false;
        }

        var activeAccount = await commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (!activeAccount.IsAdmin())
        {
            return false;
        }

        if (activeAccount.Id == command.AccountId)
        {
            return false;
        }

        var accountRecord = await commonServices.AccountServices.TryGetAccountRecord(command.AccountId).ConfigureAwait(false);
        await using var database = await databaseContextProvider.CreateCommandDbContextNow(cancellationToken).ConfigureAwait(false);
        database.Attach(accountRecord);
        accountRecord.BanExpireTime = SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromMilliseconds(command.Duration));
        accountRecord.BanReason = reason;

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        context.Operation().Items.Set(new Account_InvalidateAccountRecord(accountRecord.Id, accountRecord.Username, accountRecord.FusionId));

        return true;
    }
}