namespace AzerothMemories.WebServer.Services.Handlers;

internal static class AccountServices_OnSetupSessionCommand
{
    public static async Task TryHandle(CommonServices commonServices, IDatabaseContextProvider databaseContextProvider, SetupSessionCommand command)
    {
        var context = CommandContext.GetCurrent();
        await context.InvokeRemainingHandlers().ConfigureAwait(false);

        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Account_InvalidateAccountRecord>();
            if (invRecord != null)
            {
                _ = commonServices.AccountServices.DependsOnAccountRecord(invRecord.Id);
                _ = commonServices.AccountServices.TryGetAccountRecordUsername(invRecord.Username);
                _ = commonServices.AccountServices.TryGetAccountRecordFusionId(invRecord.FusionId);

                _ = commonServices.AdminServices.GetAccountCount();
                _ = commonServices.AdminServices.GetSessionCount();
            }

            return;
        }

        var accountRecord = await commonServices.AccountServices.TryGetActiveAccountRecord(command.Session).ConfigureAwait(false);
        if (accountRecord == null)
        {
            return;
        }

        if (accountRecord.ShouldUpdateLoginConsecutiveDays())
        {
            await using var database = await databaseContextProvider.CreateCommandDbContext().ConfigureAwait(false);
            database.Attach(accountRecord);

            accountRecord.TryUpdateLoginConsecutiveDaysCount();

            await database.SaveChangesAsync().ConfigureAwait(false);
        }

        context.Operation().Items.Set(new Account_InvalidateAccountRecord(accountRecord.Id, accountRecord.Username, accountRecord.FusionId));
    }
}