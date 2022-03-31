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
            var regionStr = command.AuthenticatedIdentity.Schema.Replace("BattleNet-", "");
            var blizzardRegion = BlizzardRegionExt.FromName(regionStr);

            if (!command.User.Claims.TryGetValue("BattleNet-Id", out var blizzardIdClaim))
            {
                throw new NotImplementedException();
            }

            if (!long.TryParse(blizzardIdClaim, out var blizzardId))
            {
                throw new NotImplementedException();
            }

            if (!command.User.Claims.TryGetValue("BattleNet-Tag", out var battleTag))
            {
                throw new NotImplementedException();
            }

            if (!command.User.Claims.TryGetValue("BattleNet-Token", out var battleNetToken))
            {
                throw new NotImplementedException();
            }

            if (!command.User.Claims.TryGetValue("BattleNet-TokenExpires", out var battleNetTokenExpiresStr) || !long.TryParse(battleNetTokenExpiresStr, out var battleNetTokenExpires))
            {
                throw new NotImplementedException();
            }

            await using var database = await databaseContextProvider.CreateCommandDbContextNow(cancellationToken).ConfigureAwait(false);

            var dbSessionInfo = await sessionRepo.Get(database, command.Session.Id, false, cancellationToken).ConfigureAwait(false);
            if (dbSessionInfo != null)
            {
                var tempAccount = await commonServices.AccountServices.TryGetAccountRecordFusionId(dbSessionInfo.UserId).ConfigureAwait(false);
                if (tempAccount != null && tempAccount.BlizzardRegionId != BlizzardRegion.None && tempAccount.BlizzardRegionId != blizzardRegion)
                {
                    return;
                }
            }

            await context.InvokeRemainingHandlers(cancellationToken).ConfigureAwait(false);

            await OnSignIn(commonServices.AccountServices, context, database, blizzardId, blizzardRegion, battleTag, battleNetToken, Instant.FromUnixTimeMilliseconds(battleNetTokenExpires), cancellationToken).ConfigureAwait(false);
        }
        else if (command.AuthenticatedIdentity.Schema.StartsWith("ToDo-"))
        {
            await context.InvokeRemainingHandlers(cancellationToken).ConfigureAwait(false);

            throw new NotImplementedException();
        }
#if DEBUG
        else if (command.AuthenticatedIdentity.Schema.StartsWith("Default"))
        {
            await context.InvokeRemainingHandlers(cancellationToken).ConfigureAwait(false);

            command.User.Claims.TryGetValue("BattleNet-Id", out var blizzardIdClaim);
            command.User.Claims.TryGetValue("BattleNet-Tag", out var battleTag);

            long.TryParse(blizzardIdClaim, out var blizzardId);

            await using var database = await databaseContextProvider.CreateCommandDbContextNow(cancellationToken).ConfigureAwait(false);

            await OnSignIn(commonServices.AccountServices, context, database, blizzardId, BlizzardRegion.Europe, battleTag, null, Instant.FromUnixTimeMilliseconds(0), cancellationToken).ConfigureAwait(false);
        }
#endif
        else
        {
            await context.InvokeRemainingHandlers(cancellationToken).ConfigureAwait(false);

            throw new NotImplementedException();
        }
    }

    private static async Task OnSignIn(AccountServices accountServices, CommandContext context, AppDbContext database, long blizzardId, BlizzardRegion blizzardRegion, string battleTag, string battleNetToken, Instant battleNetTokenExpires, CancellationToken cancellationToken)
    {
        var sessionInfo = context.Operation().Items.Get<SessionInfo>();
        if (sessionInfo == null)
        {
            throw new NotImplementedException();
        }

        var userId = sessionInfo.UserId;
        var accountRecord = await accountServices.GetOrCreateAccount(userId).ConfigureAwait(false);

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
}