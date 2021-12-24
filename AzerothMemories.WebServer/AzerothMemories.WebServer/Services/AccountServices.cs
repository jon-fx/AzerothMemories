using Stl.Fusion.Operations;

namespace AzerothMemories.WebServer.Services;

[RegisterComputeService]
[RegisterAlias(typeof(IAccountServices))]
public class AccountServices : DbServiceBase<AppDbContext>, IAccountServices
{
    public AccountServices(IServiceProvider serviceProvider) : base(serviceProvider)
    {
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

        var sessionInfo = context.Operation().Items.Get<SessionInfo>();
        var userId = sessionInfo.UserId;

        var accountRecord = await TryGetAccountRecord(userId);
        if (accountRecord == null)
        {
            var dbContext = await CreateCommandDbContext(cancellationToken);

            accountRecord = new AccountRecord
            {
                FusionId = userId,
                //MoaRef = moaRef.Full,
                //BlizzardId = moaRef.Id,
                CreatedDateTime = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                //LastLoginDateTime = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
            };

            dbContext.Accounts.Add(accountRecord);

            await dbContext.SaveChangesAsync(cancellationToken);

            if (accountRecord.Id == 0)
            {
                throw new NotImplementedException();
            }

            using (var computed = Computed.Invalidate())
            {
                _ = TryGetAccountRecord(userId);
            }
        }
    }

    [ComputeMethod]
    protected virtual async Task<AccountRecord?> TryGetAccountRecord(string fusionId)
    {
        //if (Computed.IsInvalidating())
        //{
        //    return null;
        //}

        await using var dbContext = CreateDbContext();
        var user = await dbContext.Accounts.AsQueryable()
                .Where(a => a.FusionId == fusionId)
                .FirstOrDefaultAsync();

        return user;
    }
}