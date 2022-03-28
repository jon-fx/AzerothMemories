namespace AzerothMemories.WebServer.Services.Handlers;

internal static class AccountServices_TryChangeIsPrivate
{
    public static async Task<bool> TryHandle(CommonServices commonServices, IDatabaseContextProvider databaseContextProvider, Account_TryChangeIsPrivate command)
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

        var accountRecord = await commonServices.AccountServices.TryGetActiveAccountRecord(command.Session).ConfigureAwait(false);
        if (accountRecord == null)
        {
            return false;
        }

        await using var database = await databaseContextProvider.CreateCommandDbContext().ConfigureAwait(false);
        database.Attach(accountRecord);
        accountRecord.IsPrivate = command.NewValue;
        await database.SaveChangesAsync().ConfigureAwait(false);

        context.Operation().Items.Set(new Account_InvalidateAccountRecord(accountRecord.Id, accountRecord.Username, accountRecord.FusionId));

        return command.NewValue;
    }
}