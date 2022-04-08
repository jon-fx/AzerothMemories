namespace AzerothMemories.WebServer.Services.Handlers;

internal static class AccountServices_OnSignInCommand
{
    public static async Task TryHandle(CommonServices commonServices, IDbSessionInfoRepo<AppDbContext, DbSessionInfo<string>, string> sessionRepo, IDatabaseContextProvider databaseContextProvider, SignInCommand command, CancellationToken cancellationToken)
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

        if (command.AuthenticatedIdentity.Schema.StartsWith("BattleNet-"))
        {
            await OnBlizzardSignIn(commonServices, sessionRepo, context, command, cancellationToken).ConfigureAwait(false);
        }
        else if (command.AuthenticatedIdentity.Schema.StartsWith("Patreon"))
        { 
            throw new NotImplementedException();
        }
#if DEBUG
        else if (command.AuthenticatedIdentity.Schema.StartsWith("Default"))
        {
            await OnBlizzardSignIn(commonServices, sessionRepo, context, command, cancellationToken).ConfigureAwait(false);
        }
#endif
        else
        {
            await context.InvokeRemainingHandlers(cancellationToken).ConfigureAwait(false);

            throw new NotImplementedException();
        }
    }

    private static bool GetTokensFromClaims(SignInCommand command, string prefix, out long id, out string name, out string token, out Instant tokenExpiresAt)
    {
        id = -1;
        name = null;
        token = null;
        tokenExpiresAt = Instant.FromUnixTimeMilliseconds(0);

        if (!command.User.Claims.TryGetValue($"{prefix}-Id", out var blizzardIdClaim))
        {
            return false;
        }

        if (!long.TryParse(blizzardIdClaim, out id))
        {
            return false;
        }

        if (!command.User.Claims.TryGetValue($"{prefix}-Tag", out name))
        {
        }

        if (!command.User.Claims.TryGetValue($"{prefix}-Token", out token))
        {
            return false;
        }

        if (!command.User.Claims.TryGetValue($"{prefix}-TokenExpires", out var battleNetTokenExpiresStr) || !long.TryParse(battleNetTokenExpiresStr, out var battleNetTokenExpires))
        {
            return false;
        }

        tokenExpiresAt = Instant.FromUnixTimeMilliseconds(battleNetTokenExpires);

        return true;
    }

    private static async Task OnBlizzardSignIn(CommonServices commonServices, IDbSessionInfoRepo<AppDbContext, DbSessionInfo<string>, string> sessionRepo, CommandContext context, SignInCommand command, CancellationToken cancellationToken)
    {
        var regionStr = command.AuthenticatedIdentity.Schema.Split('-');
        var blizzardRegion = BlizzardRegion.Europe;
        if (regionStr.Length > 1)
        {
            blizzardRegion = BlizzardRegionExt.FromName(regionStr[1]);
        }

        if (!GetTokensFromClaims(command, "BattleNet", out var blizzardId, out var battleTag, out var battleNetToken, out var battleNetTokenExpires))
        {
            throw new NotImplementedException();
        }

        await using var database = await commonServices.AccountServices.CreateCommandDbContextNow(cancellationToken).ConfigureAwait(false);
        var tempAccount = await GetCurrentAccount(database, command.Session, sessionRepo, commonServices.AccountServices, cancellationToken).ConfigureAwait(false);
        if (tempAccount != null && tempAccount.BlizzardRegionId != BlizzardRegion.None && tempAccount.BlizzardRegionId != blizzardRegion)
        {
            return;
        }
        
        await context.InvokeRemainingHandlers(cancellationToken).ConfigureAwait(false);

        var sessionInfo = context.Operation().Items.Get<SessionInfo>();
        if (sessionInfo == null)
        {
            throw new NotImplementedException();
        }

        var userId = sessionInfo.UserId;
        var accountRecord = await commonServices.AccountServices.GetOrCreateAccount(userId).ConfigureAwait(false);

        database.Attach(accountRecord);

        accountRecord.BlizzardId = blizzardId;
        accountRecord.BlizzardRegionId = blizzardRegion;
        accountRecord.BattleTag = battleTag;
        accountRecord.BattleNetToken = battleNetToken;
        accountRecord.BattleNetTokenExpiresAt = battleNetTokenExpires;

        if (string.IsNullOrWhiteSpace(accountRecord.Username))
        {
            var newUsername = $"User-{accountRecord.Id}";

            accountRecord.Username = newUsername;
            accountRecord.UsernameSearchable = DatabaseHelpers.GetSearchableName(newUsername);
        }

        accountRecord.TryUpdateLoginConsecutiveDaysCount();

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        context.Operation().Items.Set(new Account_InvalidateAccountRecord(accountRecord.Id, accountRecord.Username, accountRecord.FusionId));
    }

    private static async Task<AccountRecord> GetCurrentAccount(AppDbContext database, Session session, IDbSessionInfoRepo<AppDbContext, DbSessionInfo<string>, string> sessionRepo, AccountServices accountServices, CancellationToken cancellationToken)
    {
        var dbSessionInfo = await sessionRepo.Get(database, session.Id, false, cancellationToken).ConfigureAwait(false);
        if (dbSessionInfo != null)
        {
            return await accountServices.TryGetAccountRecordFusionId(dbSessionInfo.UserId).ConfigureAwait(false);
        }

        return null;
    }
}