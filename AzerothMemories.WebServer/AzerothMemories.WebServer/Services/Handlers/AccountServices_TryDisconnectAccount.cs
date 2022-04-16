﻿namespace AzerothMemories.WebServer.Services.Handlers;

internal static class AccountServices_TryDisconnectAccount
{
    public static async Task<bool> TryHandle(CommonServices commonServices, IDatabaseContextProvider databaseContextProvider, Account_TryDisconnectAccount command, CancellationToken cancellationToken)
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

        var authToken = accountRecord.AuthTokens.FirstOrDefault(x => x.Key == command.Key);
        if (authToken == null)
        {
            return false;
        }

        if (accountRecord.Id != authToken.AccountId)
        {
            return false;
        }

        await using var database = await databaseContextProvider.CreateCommandDbContextNow(cancellationToken).ConfigureAwait(false);
        database.Attach(authToken);

        authToken.Account = null;
        authToken.AccountId = null;

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        context.Operation().Items.Set(new Account_InvalidateAccountRecord(accountRecord.Id, accountRecord.Username, accountRecord.FusionId));

        return true;
    }
}