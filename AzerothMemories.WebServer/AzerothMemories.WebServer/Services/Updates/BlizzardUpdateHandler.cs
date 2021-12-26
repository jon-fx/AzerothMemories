using Hangfire;
using Hangfire.Server;

namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class BlizzardUpdateHandler : DbServiceBase<AppDbContext>
{
    public const string AccountQueue1 = "a-account";
    public const string CharacterQueue1 = "b-character";
    public const string CharacterQueue2 = "c-character";
    public const string GuildQueue1 = "d-guild";

    private readonly IServiceProvider _services;
    private readonly BlizzardAccountUpdateHandler _accountUpdateHandler;
    private readonly BlizzardCharacterUpdateHandler _characterUpdateHandler;
    private readonly BlizzardGuildUpdateHandler _guildUpdateHandler;

    public BlizzardUpdateHandler(IServiceProvider services) : base(services)
    {
        _services = services;
        _accountUpdateHandler = new BlizzardAccountUpdateHandler(services);
        _characterUpdateHandler = new BlizzardCharacterUpdateHandler(services);
        _guildUpdateHandler = new BlizzardGuildUpdateHandler(services);
    }

    public void TryUpdateAccount(AccountRecord record)
    {
        if (string.IsNullOrWhiteSpace(record.UpdateJob))
        {
            record.UpdateJob = BackgroundJob.Enqueue(() => OnAccountUpdate(record.Id, null));
            record.UpdateJobStartTime = DateTimeOffset.MinValue;
        }
    }

    [Queue(AccountQueue1)]
    public async Task OnAccountUpdate(long id, PerformContext context)
    {
        var record = await _services.GetRequiredService<AccountServices>().TryGetAccountRecord(id);
        if (record == null)
        {
            throw new NotImplementedException();
        }

        if (context == null)
        {
            throw new NotImplementedException();
        }

        if (record.UpdateJob != context.BackgroundJob.Id)
        {
            return;
        }

        await using var dbContext = CreateDbContext(true);
        dbContext.Attach(record);

        record.UpdateJob = string.Empty;
        record.UpdateJobStartTime = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset();

        await dbContext.SaveChangesAsync();

        var updateResult = await _accountUpdateHandler.TryUpdate(id, dbContext, record);
    }
}