using AzerothMemories.WebServer.Services.Updates;
using Stl.Fusion.Operations;

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

        //if (Computed.IsInvalidating())
        //{
        //    return;
        //}

        var sessionInfo = context.Operation().Items.Get<SessionInfo>();
        var userId = sessionInfo.UserId;

        var accountRecord = await TryGetAccountRecord(userId);
        await using var dbContext = _databaseProvider.GetDatabase();

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

            accountRecord.Id = await dbContext.InsertWithInt64IdentityAsync(accountRecord, token: cancellationToken);

            if (accountRecord.Id == 0)
            {
                throw new NotImplementedException();
            }

            using var computed = Computed.Invalidate();
            _ = TryGetAccountRecord(userId);
            _ = TryGetAccountRecord(accountRecord.Id);
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

        //dbContext.Attach(accountRecord);

        var blizzardRegion = BlizzardRegionExt.FromName(battleNetRegion);

        //accountRecord.BlizzardId = blizzardId;
        //accountRecord.BlizzardRegionId = blizzardRegion;
        //accountRecord.BattleNetToken = battleNetToken;
        //accountRecord.BattleNetTokenExpiresAt = DateTimeOffset.FromUnixTimeMilliseconds(battleNetTokenExpires);

        //string newUsername = null;
        //var previousBattleTag = accountRecord.BattleTag ?? string.Empty;
        //if (string.IsNullOrWhiteSpace(accountRecord.Username) || accountRecord.Username == previousBattleTag.Replace("#", string.Empty))
        //{
        //    newUsername = battleTag.Replace("#", string.Empty);
        //}

        //accountRecord.BattleTag = battleTag;

        //if (!string.IsNullOrWhiteSpace(newUsername))
        //{
        //    var result = await dbContext.Accounts.CountAsync(x => x.Username == newUsername, cancellationToken);
        //    accountRecord.Username = result > 0 ? $"User{accountRecord.Id}" : newUsername;
        //}

        //await dbContext.SaveChangesAsync(cancellationToken);
        //await _updateHandler.TryUpdateAccount(dbContext, accountRecord);
        throw new NotImplementedException();
    }

    [ComputeMethod]
    public virtual async Task<AccountRecord> TryGetAccountRecord(long id)
    {
        //if (Computed.IsInvalidating())
        //{
        //    return null;
        //}

        await using var dbContext = _databaseProvider.GetDatabase();
        var user = await dbContext.Accounts.AsQueryable()
                .Where(a => a.Id == id)
            .FirstOrDefaultAsync();

        return user;
    }

    [ComputeMethod]
    public virtual async Task<AccountRecord> TryGetAccountRecord(string fusionId)
    {
        //if (Computed.IsInvalidating())
        //{
        //    return null;
        //}

        await using var dbContext = _databaseProvider.GetDatabase();
        var user = await dbContext.Accounts.AsQueryable()
                .Where(a => a.FusionId == fusionId)
                .FirstOrDefaultAsync();

        return user;
    }
}