using NodaTime;
using Stl.CommandR;
using Stl.CommandR.Configuration;
using Stl.Fusion.Authentication.Commands;
using Stl.RegisterAttributes;

namespace AzerothMemories.WebServer.Services;

[RegisterComputeService]
[RegisterAlias(typeof(IAccountServices))]
public class AccountServices : IAccountServices
{
    private readonly CommonServices _commonServices;

    public AccountServices(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    [CommandHandler(IsFilter = true, Priority = 1)]
    protected virtual async Task OnSignIn(SignInCommand command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        await context.InvokeRemainingHandlers(cancellationToken);
    }

    [ComputeMethod]
    public virtual async Task<long> GetAccountId(string accountRef)
    {
        var accountRecord = await TryGetAccountRecord(accountRef);
        if (accountRecord == null)
        {
            await using var db = _commonServices.DatabaseProvider.GetDatabase();

            var moaRef = new MoaRef(accountRef);
            if (moaRef.IsValidAccount)
            {
                throw new NotImplementedException();
            }

            accountRecord = new AccountGrainRecord
            {
                MoaRef = moaRef.Full,
                BlizzardId = moaRef.Id,
                CreatedDateTime = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                LastLoginDateTime = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
            };

            accountRecord.Id = await db.InsertWithInt64IdentityAsync(accountRecord);

            using var computed = Computed.Invalidate();
            _ = TryGetAccount(accountRecord.Id);
            _ = TryGetAccountRecord(accountRecord.Id);
            _ = TryGetAccountRecord(accountRecord.MoaRef);
        }

        if (accountRecord.Id == 0)
        {
            throw new NotImplementedException();
        }

        return accountRecord.Id;
    }

    [ComputeMethod]
    public virtual async Task<AccountViewModel> TryGetAccount(Session session, long accountId, CancellationToken cancellationToken = default)
    {
        var accountViewModel = await TryGetAccount(accountId);
        if (accountViewModel == null)
        {
            return null;
        }

        return accountViewModel;
    }

    [ComputeMethod]
    public virtual async Task<AccountViewModel> TryGetAccount(long accountId)
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

    public async Task<string> TryChangeUsername(Session session, string newUsername, CancellationToken cancellationToken = default)
    {
        await using var db = _commonServices.DatabaseProvider.GetDatabase();

        var user = await _commonServices.Auth.GetSessionUser(session, cancellationToken);
        user.MustBeAuthenticated();

        if (!user.Claims.TryGetValue("MoaRef", out var moaRef))
        {
            throw new NotImplementedException();
        }

        var accountQuery = db.Accounts.Where(x => x.MoaRef == moaRef);
        var accountRecord = await accountQuery.FirstOrDefaultAsync(cancellationToken);
        if (accountRecord == null)
        {
            throw new NotImplementedException();
        }

        var usernameExists = db.Accounts.Any(x => x.Username == newUsername);
        if (usernameExists)
        {
            throw new NotImplementedException();
        }

        accountRecord.Username = newUsername;
        accountRecord.UsernameChangeTime = (SystemClock.Instance.GetCurrentInstant() + Duration.FromMinutes(1)).ToDateTimeOffset();

        var updateQuery = accountQuery.AsUpdatable();
        updateQuery = updateQuery.Set(x => x.Username, accountRecord.Username);
        updateQuery = updateQuery.Set(x => x.UsernameChangeTime, accountRecord.UsernameChangeTime);

        var updateCount = await updateQuery.UpdateAsync(cancellationToken);
        if (updateCount > 0)
        {
            using var computed = Computed.Invalidate();
            _ = TryGetAccount(accountRecord.Id);
            _ = TryGetAccountRecord(accountRecord.Id);
            _ = TryGetAccountRecord(accountRecord.MoaRef);
        }

        return accountRecord.Username;
    }

    [ComputeMethod]
    public virtual async Task<string?> OnLogin(long accountId, string newBattleTag, string token, long tokenExpiresAt)
    {
        //if (Computed.IsInvalidating())
        //{
        //    return null;
        //}

        var record = await TryGetAccountRecord(accountId);
        if (record == null)
        {
            throw new NotImplementedException();
        }

        await using var db = _commonServices.DatabaseProvider.GetDatabase();
        var updateQuery = db.Accounts.Where(x => x.Id == record.Id).AsUpdatable();

        string? newUsername = null;
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
            _ = TryGetAccount(record.Id);
            _ = TryGetAccountRecord(record.Id);
            _ = TryGetAccountRecord(record.MoaRef);
        }

        //_commonServices.Commander.Start(new Tests(UpdatePriority.Account, record.Id, RandomGenerator.Instance.Next()));
        //_commonServices.QueuedUpdateHandler.EnqueueUpdate(UpdatePriority.Account, record.Id, RandomGenerator.Instance.Next());

        //await PublishViewModelChanged();

        return string.Empty;
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
    protected virtual async Task<AccountGrainRecord?> TryGetAccountRecord(string moaRef)
    {
        //if (Computed.IsInvalidating())
        //{
        //    return null;
        //}

        await using var db = _commonServices.DatabaseProvider.GetDatabase();
        return await db.Accounts.Where(x => x.MoaRef == moaRef).FirstOrDefaultAsync();
    }
}