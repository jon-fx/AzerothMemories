namespace AzerothMemories.WebServer.Services.Handlers;

internal static class AccountServices_TryChangeUsername
{
    public static async Task<bool> TryHandle(CommonServices commonServices, Account_TryChangeUsername command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Account_InvalidateAccountRecord>();
            if (invRecord != null)
            {
                _ = commonServices.AccountServices.DependsOnAccountRecord(invRecord.Id);
                _ = commonServices.AccountServices.DependsOnAccountUsername(invRecord.Id);

                var username = invRecord.Username;
                if (!string.IsNullOrWhiteSpace(username))
                {
                    _ = commonServices.AccountServices.CheckIsValidUsername(username);
                }
            }

            var invPreviousUsername = context.Operation().Items.Get<string>();
            if (!string.IsNullOrWhiteSpace(invPreviousUsername))
            {
                _ = commonServices.AccountServices.CheckIsValidUsername(invPreviousUsername);
                _ = commonServices.AccountServices.TryGetAccountRecordUsername(invPreviousUsername);
            }

            return default;
        }

        var activeAccountRecord = await commonServices.AccountServices.TryGetActiveAccountRecord(command.Session).ConfigureAwait(false);
        if (activeAccountRecord == null)
        {
            return false;
        }

        var newUsername = command.NewUsername;
        var accountRecord = activeAccountRecord;
        if (activeAccountRecord.AccountType >= AccountType.Admin && command.AccountId > 0)
        {
            if (!DatabaseHelpers.IsValidAccountName(newUsername))
            {
                newUsername = $"User-{command.AccountId}";
            }

            accountRecord = await commonServices.AccountServices.TryGetAccountRecord(command.AccountId).ConfigureAwait(false);
        }
        else if (!DatabaseHelpers.IsValidAccountName(newUsername))
        {
            return false;
        }
        else if (accountRecord.Username.StartsWith("User-"))
        {
        }
        else if (accountRecord.UsernameChangedTime + commonServices.Config.UsernameChangeDelay > SystemClock.Instance.GetCurrentInstant())
        {
            return false;
        }

        if (accountRecord == null)
        {
            return false;
        }

        await using var database = await commonServices.DatabaseHub.CreateCommandDbContext(cancellationToken).ConfigureAwait(false);
        var usernameExists = await database.Accounts.AnyAsync(x => x.Username == newUsername, cancellationToken).ConfigureAwait(false);
        if (usernameExists)
        {
            return false;
        }

        var previousUsername = accountRecord.Username;
        database.Attach(accountRecord);
        accountRecord.Username = newUsername;
        accountRecord.UsernameSearchable = DatabaseHelpers.GetSearchableName(newUsername);
        accountRecord.UsernameChangedTime = SystemClock.Instance.GetCurrentInstant();

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
        {
            AccountId = accountRecord.Id,
            Type = AccountHistoryType.UsernameChanged
        }, cancellationToken).ConfigureAwait(false);

        context.Operation().Items.Set(new Account_InvalidateAccountRecord(accountRecord.Id, accountRecord.Username, accountRecord.FusionId));
        context.Operation().Items.Set(previousUsername);

        return true;
    }
}