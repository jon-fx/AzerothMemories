using ActualLab.Collections;

namespace AzerothMemories.WebServer.Services.Handlers;

internal static class AccountServices_OnSignOutCommand
{
    public static async Task TryHandle(ILogger<AccountServices> logger, CommonServices commonServices, Auth_SignOut command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        await context.InvokeRemainingHandlers(cancellationToken).ConfigureAwait(false);

        if (Invalidation.IsActive)
        {
            if (context.Operation.Items.KeylessTryGet(out Account_InvalidateAccountRecord? invRecord) && invRecord != null)
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

        context.Operation.Items.KeylessSet(new Account_InvalidateAccountRecord(accountRecord.Id, accountRecord.Username, accountRecord.FusionId));
    }
}