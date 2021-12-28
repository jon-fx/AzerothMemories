namespace AzerothMemories.WebServer.Services;

[RegisterComputeService]
[RegisterAlias(typeof(IAccountServices))]
public class AccountServices : IAccountServices
{
    private readonly BlizzardUpdateHandler _updateHandler;
    private readonly DatabaseProvider _databaseProvider;

    public AccountServices(IServiceProvider serviceProvider)
    {
        _updateHandler = serviceProvider.GetRequiredService<BlizzardUpdateHandler>();
        _databaseProvider = serviceProvider.GetRequiredService<DatabaseProvider>();
    }

    [CommandHandler(IsFilter = true, Priority = 1)]
    protected virtual async Task OnSignIn(SignInCommand command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        await context.InvokeRemainingHandlers(cancellationToken);

        if (Computed.IsInvalidating())
        {
            return;
        }

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

        if (!command.User.Claims.TryGetValue("BattleNet-Region", out var battleNetRegion))
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

        var sessionInfo = context.Operation().Items.Get<SessionInfo>();
        var userId = sessionInfo.UserId;

        await using var database = _databaseProvider.GetDatabase();
        var accountRecord = await GetOrCreateAccount(database, userId);

        var updateQuery = database.GetUpdateQuery(accountRecord, out var changed);
        var blizzardRegion = BlizzardRegionExt.FromName(battleNetRegion);

        if (CheckAndChange.Check(ref accountRecord.BlizzardId, blizzardId, ref changed))
        {
            updateQuery = updateQuery.Set(x => x.BlizzardId, accountRecord.BlizzardId);
        }

        if (CheckAndChange.Check(ref accountRecord.BlizzardRegionId, blizzardRegion, ref changed))
        {
            updateQuery = updateQuery.Set(x => x.BlizzardRegionId, accountRecord.BlizzardRegionId);
        }

        if (CheckAndChange.Check(ref accountRecord.BattleNetToken, battleNetToken, ref changed))
        {
            updateQuery = updateQuery.Set(x => x.BattleNetToken, accountRecord.BattleNetToken);
        }

        if (CheckAndChange.Check(ref accountRecord.BattleNetTokenExpiresAt, DateTimeOffset.FromUnixTimeMilliseconds(battleNetTokenExpires), ref changed))
        {
            updateQuery = updateQuery.Set(x => x.BattleNetTokenExpiresAt, accountRecord.BattleNetTokenExpiresAt);
        }

        string newUsername = null;
        var previousBattleTag = accountRecord.BattleTag ?? string.Empty;
        if (string.IsNullOrWhiteSpace(accountRecord.Username) || accountRecord.Username == previousBattleTag.Replace("#", string.Empty))
        {
            newUsername = battleTag.Replace("#", string.Empty);
        }

        if (CheckAndChange.Check(ref accountRecord.BattleTag, battleTag, ref changed))
        {
            updateQuery = updateQuery.Set(x => x.BattleTag, accountRecord.BattleTag);
        }

        if (!string.IsNullOrWhiteSpace(newUsername))
        {
            var result = await database.Accounts.CountAsync(x => x.Username == newUsername, cancellationToken);
            newUsername = result > 0 ? $"User{accountRecord.Id}" : newUsername;

            if (CheckAndChange.Check(ref accountRecord.Username, newUsername, ref changed))
            {
                updateQuery = updateQuery.Set(p => p.Username, accountRecord.Username);
            }

            if (CheckAndChange.Check(ref accountRecord.UsernameSearchable, DatabaseHelpers.GetSearchableName(accountRecord.Username), ref changed))
            {
                updateQuery = updateQuery.Set(p => p.UsernameSearchable, accountRecord.UsernameSearchable);
            }
        }

        if (changed)
        {
            await updateQuery.UpdateAsync(cancellationToken);

            using var computed = Computed.Invalidate();
            _ = TryGetAccountRecord(userId);
            _ = TryGetAccountRecord(accountRecord.Id);
        }

        await _updateHandler.TryUpdateAccount(database, accountRecord);
    }

    private async Task<AccountRecord> GetOrCreateAccount(IDataContext database, string userId)
    {
        var accountRecord = await TryGetAccountRecord(userId);
        if (accountRecord == null)
        {
            accountRecord = new AccountRecord
            {
                FusionId = userId,
                //MoaRef = moaRef.Full,
                //BlizzardId = moaRef.Id,
                CreatedDateTime = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                //LastLoginDateTime = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
            };

            accountRecord.Id = await database.InsertWithInt64IdentityAsync(accountRecord);

            if (accountRecord.Id == 0)
            {
                throw new NotImplementedException();
            }

            using var computed = Computed.Invalidate();

            _ = TryGetAccountRecord(userId);
            _ = TryGetAccountRecord(accountRecord.Id);
        }

        return accountRecord;
    }

    [ComputeMethod]
    public virtual async Task<AccountRecord> TryGetAccountRecord(long id)
    {
        if (Computed.IsInvalidating())
        {
            //return null;
        }

        await using var dbContext = _databaseProvider.GetDatabase();
        var user = await dbContext.Accounts.Where(a => a.Id == id).FirstOrDefaultAsync();

        return user;
    }

    [ComputeMethod]
    public virtual async Task<AccountRecord> TryGetAccountRecord(string fusionId)
    {
        if (Computed.IsInvalidating())
        {
            //return null;
        }

        await using var dbContext = _databaseProvider.GetDatabase();
        var user = await dbContext.Accounts.Where(a => a.FusionId == fusionId).FirstOrDefaultAsync();
        return user;
    }
}