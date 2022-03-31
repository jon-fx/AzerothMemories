namespace AzerothMemories.WebServer.Services.Handlers;

internal static class AccountServices_TryChangeSocialLink
{
    public static async Task<string> TryHandle(CommonServices commonServices, IDatabaseContextProvider databaseContextProvider, Account_TryChangeSocialLink command, CancellationToken cancellationToken)
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
            return null;
        }

        var newValue = command.NewValue;
        if (!string.IsNullOrWhiteSpace(newValue) && accountRecord.AccountType < AccountPermissionExt.Permission_CanChangeSocialLinks)
        {
            return null;
        }

        if (accountRecord.AccountType >= AccountType.Admin && command.AccountId > 0)
        {
            accountRecord = await commonServices.AccountServices.TryGetAccountRecord(command.AccountId).ConfigureAwait(false);
        }

        var helper = SocialHelpers.All[command.LinkId];
        var previous = ServerSocialHelpers.GetterFunc[helper.LinkId](accountRecord);
        if (!string.IsNullOrWhiteSpace(newValue) && !helper.ValidatorFunc(newValue))
        {
            return previous;
        }

        if (previous == newValue)
        {
            return previous;
        }

        await using var database = await databaseContextProvider.CreateCommandDbContextNow(cancellationToken).ConfigureAwait(false);
        database.Attach(accountRecord);

        ServerSocialHelpers.SetterFunc[helper.LinkId](accountRecord, newValue);

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        context.Operation().Items.Set(new Account_InvalidateAccountRecord(accountRecord.Id, accountRecord.Username, accountRecord.FusionId));

        return newValue;
    }
}