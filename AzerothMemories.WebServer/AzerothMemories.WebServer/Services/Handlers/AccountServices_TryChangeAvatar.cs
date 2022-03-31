namespace AzerothMemories.WebServer.Services.Handlers;

internal static class AccountServices_TryChangeAvatar
{
    public static async Task<string> TryHandle(CommonServices commonServices, IDatabaseContextProvider databaseContextProvider, Account_TryChangeAvatar command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Account_InvalidateAccountRecord>();
            if (invRecord != null)
            {
                _ = commonServices.AccountServices.DependsOnAccountRecord(invRecord.Id);
                _ = commonServices.AccountServices.DependsOnAccountAvatar(invRecord.Id);
                _ = commonServices.MediaServices.TryGetUserAvatar($"{ZExtensions.AvatarBlobFilePrefix}{invRecord.Id}-0.jpg");
                _ = commonServices.MediaServices.TryGetUserAvatar($"{ZExtensions.AvatarBlobFilePrefix}{invRecord.Id}-1.jpg");
            }

            return default;
        }

        var activeAccountViewModel = await commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccountViewModel == null)
        {
            return null;
        }

        if (!activeAccountViewModel.CanChangeAvatar())
        {
            return null;
        }

        var accountViewModel = activeAccountViewModel;
        if (activeAccountViewModel.CanChangeAnyUsersAvatar() && command.AccountId > 0)
        {
            accountViewModel = await commonServices.AccountServices.TryGetAccountById(command.Session, command.AccountId).ConfigureAwait(false);
        }

        if (accountViewModel == null)
        {
            return null;
        }

        var newAvatar = command.NewAvatar;
        if (accountViewModel.Avatar == newAvatar)
        {
            return accountViewModel.Avatar;
        }

        if (string.IsNullOrWhiteSpace(newAvatar))
        {
            newAvatar = null;
        }
        else if (newAvatar.StartsWith($"{ZExtensions.CustomUserAvatarPathPrefix}{accountViewModel.Id}-"))
        {
        }
        else if (newAvatar.StartsWith("https://render") && newAvatar.Contains(".worldofwarcraft.com"))
        {
            var character = accountViewModel.GetAllCharactersSafe().FirstOrDefault(x => x.AvatarLink == newAvatar);
            if (character == null)
            {
                return null;
            }
        }
        else
        {
            return null;
        }

        var accountRecord = await commonServices.AccountServices.TryGetAccountRecord(accountViewModel.Id).ConfigureAwait(false);
        await using var database = await databaseContextProvider.CreateCommandDbContextNow(cancellationToken).ConfigureAwait(false);
        database.Attach(accountRecord);
        accountRecord.Avatar = newAvatar;

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        context.Operation().Items.Set(new Account_InvalidateAccountRecord(accountRecord.Id, accountRecord.Username, accountRecord.FusionId));

        return newAvatar;
    }
}