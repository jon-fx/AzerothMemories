namespace AzerothMemories.WebServer.Services.Handlers;

internal static class AccountServices_OnSignInCommand
{
    public static async Task TryHandle(CommonServices commonServices, IDbSessionInfoRepo<AppDbContext, DbSessionInfo<string>, string> sessionRepo, SignInCommand command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();

        if (Computed.IsInvalidating())
        {
            await context.InvokeRemainingHandlers(cancellationToken).ConfigureAwait(false);

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

        await using var database = await commonServices.DatabaseHub.CreateCommandDbContext(cancellationToken).ConfigureAwait(false);
        var accountRecord = await GetCurrentAccount(database, command.Session, sessionRepo, commonServices.AccountServices, cancellationToken).ConfigureAwait(false);

        var authKey = command.AuthenticatedIdentity.Id.Value;
        var authRecord = await database.AuthTokens.FirstOrDefaultAsync(x => x.Key == authKey, cancellationToken).ConfigureAwait(false);

        bool canSignInResult;
        if (command.AuthenticatedIdentity.Schema.StartsWith("BattleNet"))
        {
            canSignInResult = CanBlizzardAccountSignIn(command, accountRecord, authRecord);
        }
        else if (command.AuthenticatedIdentity.Schema.StartsWith("Patreon"))
        {
            canSignInResult = CanPatreonAccountSignIn(command, accountRecord, authRecord);
        }
        else if (command.AuthenticatedIdentity.Schema.StartsWith("Default"))
        {
            canSignInResult = CanBlizzardAccountSignIn(command, accountRecord, authRecord);
        }
        else
        {
            throw new NotImplementedException();
        }

        Exceptions.ThrowIf(!canSignInResult);

        await context.InvokeRemainingHandlers(cancellationToken).ConfigureAwait(false);

        if (accountRecord == null)
        {
            var sessionInfo = context.Operation().Items.Get<SessionInfo>();
            if (sessionInfo == null)
            {
                throw new NotImplementedException();
            }

            accountRecord = await GetOrCreateAccount(commonServices.Commander, database, sessionInfo.UserId).ConfigureAwait(false);
        }

        if (string.IsNullOrWhiteSpace(accountRecord.Username))
        {
            var newUsername = $"User-{accountRecord.Id}";

            accountRecord.Username = newUsername;
            accountRecord.UsernameSearchable = DatabaseHelpers.GetSearchableName(newUsername);
        }

        if (authRecord != null)
        {
            if (authRecord.AccountId == null)
            {
                authRecord.AccountId = accountRecord.Id;
                accountRecord.AuthTokens.Add(authRecord);
            }
            else if (authRecord.AccountId != accountRecord.Id)
            {
                throw new NotImplementedException();
            }
        }

        accountRecord.TryUpdateLoginConsecutiveDaysCount();

        var blizzardToken = accountRecord.AuthTokens.FirstOrDefault(x => x.IsBlizzardAuthToken);
        if (blizzardToken != null)
        {
            Exceptions.ThrowIf(accountRecord.BlizzardId > 0 && accountRecord.BlizzardId != blizzardToken.IdLong);

            accountRecord.BlizzardId = blizzardToken.IdLong;
            accountRecord.BattleTag = blizzardToken.Name;
        }

        var patreonToken = accountRecord.AuthTokens.FirstOrDefault(x => x.IsPatreon);
        if (patreonToken != null)
        {
        }

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        context.Operation().Items.Set(new Account_InvalidateAccountRecord(accountRecord.Id, accountRecord.Username, accountRecord.FusionId));
    }

    private static async Task<AccountRecord> GetOrCreateAccount(ICommander commander, AppDbContext database, string userId)
    {
        var accountRecord = await database.Accounts.FirstOrDefaultAsync(a => a.FusionId == userId).ConfigureAwait(false);
        if (accountRecord == null)
        {
            accountRecord = new AccountRecord
            {
                FusionId = userId,
                AccountFlags = AccountFlags.AlphaUser,
                CreatedDateTime = SystemClock.Instance.GetCurrentInstant(),
            };

            await database.Accounts.AddAsync(accountRecord).ConfigureAwait(false);
            await database.SaveChangesAsync().ConfigureAwait(false);

            await commander.Call(new Account_AddNewHistoryItem
            {
                AccountId = accountRecord.Id,
                Type = AccountHistoryType.AccountCreated
            }).ConfigureAwait(false);
        }

        Exceptions.ThrowIf(accountRecord.Id == 0);

        return accountRecord;
    }

    private static async Task<AccountRecord> GetCurrentAccount(AppDbContext database, Session session, IDbSessionInfoRepo<AppDbContext, DbSessionInfo<string>, string> sessionRepo, AccountServices accountServices, CancellationToken cancellationToken)
    {
        var dbSessionInfo = await sessionRepo.Get(database, session.Id, false, cancellationToken).ConfigureAwait(false);
        if (dbSessionInfo != null)
        {
            return await database.Accounts.FirstOrDefaultAsync(a => a.FusionId == dbSessionInfo.UserId, cancellationToken).ConfigureAwait(false);
        }

        return null;
    }

    private static bool CanBlizzardAccountSignIn(SignInCommand command, AccountRecord tempAccount, AuthTokenRecord authToken)
    {
        if (tempAccount == null)
        {
            return true;
        }

        if (authToken == null)
        {
            return false;
        }

        if (authToken.AccountId == null)
        {
            return true;
        }

        if (authToken.AccountId == tempAccount.Id)
        {
            return true;
        }

        return false;
    }

    private static bool CanPatreonAccountSignIn(SignInCommand command, AccountRecord tempAccount, AuthTokenRecord authToken)
    {
        if (tempAccount == null)
        {
            return false;
        }

        if (authToken == null)
        {
            return false;
        }

        if (tempAccount.BlizzardId == 0)
        {
            return false;
        }

        if (authToken.AccountId == null)
        {
            return true;
        }

        if (authToken.AccountId == tempAccount.Id)
        {
            return true;
        }

        return false;
    }
}