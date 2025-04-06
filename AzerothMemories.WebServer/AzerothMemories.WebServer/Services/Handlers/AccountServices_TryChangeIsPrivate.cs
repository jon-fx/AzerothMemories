using ActualLab.Collections;

namespace AzerothMemories.WebServer.Services.Handlers;

internal static class AccountServices_TryChangeIsPrivate
{
    public static async Task<bool> TryHandle(ILogger<AccountServices> logger, CommonServices commonServices, Account_TryChangeIsPrivate command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        if (Invalidation.IsActive)
        {
            if (context.Operation.Items.KeylessTryGet(out Account_InvalidateAccountRecord? invRecord) && invRecord != null)
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

        await using var database = await commonServices.DatabaseHub.CreateOperationDbContext(cancellationToken).ConfigureAwait(false);
        database.Attach(accountRecord);
        accountRecord.IsPrivate = command.NewValue;
        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        context.Operation.Items.KeylessSet(new Account_InvalidateAccountRecord(accountRecord.Id, accountRecord.Username, accountRecord.FusionId));

        return command.NewValue;
    }
}