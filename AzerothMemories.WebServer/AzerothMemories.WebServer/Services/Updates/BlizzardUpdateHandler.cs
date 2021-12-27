using Hangfire;
using Hangfire.Server;

namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class BlizzardUpdateHandler
{
    public const string AccountQueue1 = "a-account";
    public const string CharacterQueue1 = "b-character";
    public const string CharacterQueue2 = "c-character";
    public const string GuildQueue1 = "d-guild";

    private readonly IServiceProvider _services;
    private readonly DatabaseProvider _databaseProvider;
    private readonly BlizzardAccountUpdateHandler _accountUpdateHandler;
    private readonly BlizzardCharacterUpdateHandler _characterUpdateHandler;
    private readonly BlizzardGuildUpdateHandler _guildUpdateHandler;

    public BlizzardUpdateHandler(IServiceProvider services)
    {
        _services = services;
        _databaseProvider = _services.GetRequiredService<DatabaseProvider>();
        _accountUpdateHandler = new BlizzardAccountUpdateHandler(services);
        _characterUpdateHandler = new BlizzardCharacterUpdateHandler(services);
        _guildUpdateHandler = new BlizzardGuildUpdateHandler(services);
    }

    public async Task TryUpdateAccount(AppDbContext dbContext, AccountRecord record)
    {
        if (!RecordRequiresUpdate(record))
        {
            return;
        }

        record.UpdateJob = BackgroundJob.Enqueue(() => OnAccountUpdate(record.Id, null));
        record.UpdateJobQueueTime = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset();
        record.UpdateJobStartTime = null;

        await dbContext.SaveChangesAsync();
    }

    public async Task TryUpdateCharacter(AppDbContext dbContext, CharacterRecord record)
    {
        if (!RecordRequiresUpdate(record))
        {
            return;
        }

        record.UpdateJob = BackgroundJob.Enqueue(() => OnCharacterUpdate1(record.Id, null));
        record.UpdateJobQueueTime = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset();
        record.UpdateJobStartTime = null;

        await dbContext.SaveChangesAsync();
    }

    private static bool RecordRequiresUpdate(IBlizzardGrainUpdateRecord record)
    {
        return true;
    }

    private static bool RecordRequiresUpdateNow(IBlizzardGrainUpdateRecord record, PerformContext context)
    {
        if (record.UpdateJob != context.BackgroundJob.Id)
        {
            return false;
        }

        return true;
    }

    //private static async Task OnUpdateStarted(DatabaseConnection dbContext, IBlizzardGrainUpdateRecord record)
    //{
    //    Exceptions.ThrowIf(string.IsNullOrEmpty(record.UpdateJob));
    //    Exceptions.ThrowIf(record.UpdateJobQueueTime == null);
    //    Exceptions.ThrowIf(record.UpdateJobStartTime != null);
    //    //Exceptions.ThrowIf(record.UpdateJobEndTime != null);

    //    //dbContext.Attach(record);

    //    record.UpdateJob = string.Empty;
    //    record.UpdateJobStartTime = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset();

    //    //await dbContext.SaveChangesAsync();
    //}

    //private static async Task OnUpdateFinished(DatabaseConnection dbContext, IBlizzardGrainUpdateRecord record, HttpStatusCode updateResult)
    //{
    //    Exceptions.ThrowIf(record.UpdateJob != string.Empty);
    //    Exceptions.ThrowIf(record.UpdateJobQueueTime == null);
    //    Exceptions.ThrowIf(record.UpdateJobStartTime == null);
    //    //Exceptions.ThrowIf(record.UpdateJobEndTime != null);

    //    //dbContext.Attach(record);

    //    record.UpdateJob = null;
    //    record.UpdateJobStartTime = null;
    //    record.UpdateJobQueueTime = null;
    //    record.UpdateJobEndTime = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset();
    //    record.UpdateJobLastResult = updateResult;

    //    //await dbContext.SaveChangesAsync();
    //}

    [Queue(AccountQueue1)]
    public async Task OnAccountUpdate(long id, PerformContext context)
    {
        //var record = await _services.GetRequiredService<AccountServices>().TryGetAccountRecord(id);
        //if (record == null || context == null)
        //{
        //    throw new NotImplementedException();
        //}

        //if (!RecordRequiresUpdateNow(record, context))
        //{
        //    return;
        //}

        //await using var dbContext = _databaseProvider.GetDatabase();
        //await OnUpdateStarted(dbContext, record);
        //await OnUpdateFinished(dbContext, record, await _accountUpdateHandler.TryUpdate(id, dbContext, record));
    }

    [Queue(CharacterQueue1)]
    public Task OnCharacterUpdate1(long id, PerformContext context)
    {
        return OnCharacterUpdate(id, context);
    }

    //[Queue(CharacterQueue2)]
    //public Task OnCharacterUpdate2(long id, PerformContext context)
    //{
    //    return OnCharacterUpdate(id, context);
    //}

    private async Task OnCharacterUpdate(long id, PerformContext context)
    {
        //var record = await _services.GetRequiredService<CharacterServices>().TryGetCharacterRecord(id);
        //if (record == null || context == null)
        //{
        //    throw new NotImplementedException();
        //}

        //if (!RecordRequiresUpdateNow(record, context))
        //{
        //    return;
        //}

        //await using var dbContext = _databaseProvider.GetDatabase();
        //await OnUpdateStarted(dbContext, record);
        //await OnUpdateFinished(dbContext, record, await _characterUpdateHandler.TryUpdate(id, dbContext, record));
    }
}