namespace AzerothMemories.WebServer.Services;

public class AccountServices : IAccountServices
{
    private readonly CommonServices _commonServices;

    public AccountServices(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    [ComputeMethod]
    public virtual async Task<AccountViewModel> GetAccount(Session session, long accountId)
    {
        var record = await TryGetAccountRecord(accountId);
        if (record == null)
        {
            return null;
        }

        return new AccountViewModel
        {
            Id = record.Id,
            Ref = record.MoaRef,
            Username = record.Username,
        };
    }

    [ComputeMethod]
    public virtual async Task<string?> OnLogin(string accountId, string newBattleTag, string token, long tokenExpiresAt)
    {
        if (Computed.IsInvalidating())
        {
            return null;
        }

        var record = await TryGetAccountRecord(accountId);
        if (record == null)
        {
            record = await CreateAccount(accountId);
        }

        await using var db = _commonServices.DatabaseProvider.GetDatabase();
        var updateQuery = db.Accounts.Where(x => x.Id == record.Id).AsUpdatable();

        string newUsername = null;
        var previousBattleTag = record.BattleTag ?? string.Empty;
        var changed = false;
        if (string.IsNullOrWhiteSpace(record.Username) || record.Username == previousBattleTag.Replace("#", string.Empty))
        {
            newUsername = newBattleTag.Replace("#", string.Empty);
        }

        if (CheckAndChange.Check(ref record.BattleTag, newBattleTag, ref changed))
        {
            updateQuery = updateQuery.Set(x => x.BattleTag, record.BattleTag);
        }

        if (!string.IsNullOrWhiteSpace(newUsername))
        {
            var result = await db.Accounts.CountAsync(x => x.Username == newUsername);
            while (result > 0)
            {
                newUsername = $"User{RandomGenerator.Instance.Next()}";
                result = await db.Accounts.CountAsync(x => x.Username == newUsername);
            }

            if (CheckAndChange.Check(ref record.Username, newUsername, ref changed))
            {
                updateQuery = updateQuery.Set(p => p.Username, record.Username);
            }
        }

        if (CheckAndChange.Check(ref record.AccessToken, token, ref changed))
        {
            updateQuery = updateQuery.Set(x => x.AccessToken, record.AccessToken);
        }

        if (CheckAndChange.Check(ref record.AccessTokenExpiresAt, tokenExpiresAt, ref changed))
        {
            updateQuery = updateQuery.Set(x => x.AccessTokenExpiresAt, record.AccessTokenExpiresAt);
        }

        if (CheckAndChange.Check(ref record.LastLoginDateTime, SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(), ref changed))
        {
            updateQuery = updateQuery.Set(x => x.LastLoginDateTime, record.LastLoginDateTime);
        }

        if (changed)
        {
            await updateQuery.UpdateAsync();

            using var computed = Computed.Invalidate();
            _ = TryGetAccountRecord(record.Id);
            _ = TryGetAccountRecord(record.MoaRef);
        }

        //_commonServices.Commander.Start(new Tests(UpdatePriority.Account, record.Id, RandomGenerator.Instance.Next()));
        //_commonServices.QueuedUpdateHandler.EnqueueUpdate(UpdatePriority.Account, record.Id, RandomGenerator.Instance.Next());

        //await PublishViewModelChanged();

        return string.Empty;
    }

    [ComputeMethod]
    protected virtual async Task<AccountGrainRecord> CreateAccount(string moaRef)
    {
        await using var db = _commonServices.DatabaseProvider.GetDatabase();

        var accountRecord = new AccountGrainRecord { MoaRef = moaRef };
        accountRecord.Id = await db.InsertWithInt64IdentityAsync(accountRecord);

        using var computed = Computed.Invalidate();
        _ = TryGetAccountRecord(moaRef);
        _ = TryGetAccountRecord(accountRecord.Id);

        return accountRecord;
    }

    [ComputeMethod]
    protected virtual async Task<AccountGrainRecord?> TryGetAccountRecord(long accountId)
    {
        //if (Computed.IsInvalidating())
        //{
        //    return null;
        //}

        await using var db = _commonServices.DatabaseProvider.GetDatabase();
        return await db.Accounts.Where(x => x.Id == accountId).FirstOrDefaultAsync();
    }

    [ComputeMethod]
    protected virtual async Task<AccountGrainRecord?> TryGetAccountRecord(string accountId)
    {
        //if (Computed.IsInvalidating())
        //{
        //    return null;
        //}

        await using var db = _commonServices.DatabaseProvider.GetDatabase();
        return await db.Accounts.Where(x => x.MoaRef == accountId).FirstOrDefaultAsync();
    }
}