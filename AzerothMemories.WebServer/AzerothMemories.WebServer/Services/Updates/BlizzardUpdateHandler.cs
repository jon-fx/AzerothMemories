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

    public async Task TryUpdateAccount(DatabaseConnection database, AccountRecord record)
    {
        await TryUpdate(database, record, () => BackgroundJob.Enqueue(() => OnAccountUpdate(record.Id, null)));
    }

    public async Task TryUpdateCharacter(DatabaseConnection database, CharacterRecord record)
    {
        await TryUpdate(database, record, () => BackgroundJob.Enqueue(() => OnCharacterUpdate1(record.Id, null)));
    }

    private async Task TryUpdate<TRecord>(DatabaseConnection database, TRecord record, Func<string> callback) where TRecord : class, IBlizzardGrainUpdateRecord
    {
        if (!RecordRequiresUpdate(record))
        {
            return;
        }

        var updateQuery = database.GetUpdateQuery(record, out _);

        updateQuery = updateQuery.Set(x => x.UpdateJob, record.UpdateJob = callback());
        updateQuery = updateQuery.Set(x => x.UpdateJobQueueTime, record.UpdateJobQueueTime = DateTimeOffset.UtcNow);
        updateQuery = updateQuery.Set(x => x.UpdateJobStartTime, record.UpdateJobStartTime = null);
        updateQuery = updateQuery.Set(x => x.UpdateJobEndTime, record.UpdateJobEndTime = null);

        var result = await updateQuery.UpdateAsync();
    }

    private bool RecordRequiresUpdate(IBlizzardGrainUpdateRecord record)
    {
        return true;
    }

    private async Task<bool> OnUpdateStarted<TRecord>(DatabaseConnection database, TRecord record, PerformContext context) where TRecord : class, IBlizzardGrainUpdateRecord
    {
        if (record.UpdateJob != context.BackgroundJob.Id)
        {
            return false;
        }

        var updateQuery = database.GetUpdateQuery(record, out _);
        updateQuery = updateQuery.Set(x => x.UpdateJob, record.UpdateJob = string.Empty);
        updateQuery = updateQuery.Set(x => x.UpdateJobQueueTime, record.UpdateJobQueueTime = null);
        updateQuery = updateQuery.Set(x => x.UpdateJobStartTime, record.UpdateJobStartTime = DateTimeOffset.UtcNow);
        updateQuery = updateQuery.Set(x => x.UpdateJobEndTime, record.UpdateJobEndTime = null);

        var result = await updateQuery.UpdateAsync();

        return result > 0;
    }

    private async Task OnUpdateFinished<TRecord>(DatabaseConnection database, TRecord record, HttpStatusCode updateResult) where TRecord : class, IBlizzardGrainUpdateRecord
    {
        Exceptions.ThrowIf(record.UpdateJob != string.Empty);
        Exceptions.ThrowIf(record.UpdateJobQueueTime != null);
        Exceptions.ThrowIf(record.UpdateJobStartTime == null);
        Exceptions.ThrowIf(record.UpdateJobEndTime != null);

        var updateQuery = database.GetUpdateQuery(record, out _);
        updateQuery = updateQuery.Set(x => x.UpdateJob, record.UpdateJob = null);
        updateQuery = updateQuery.Set(x => x.UpdateJobStartTime, record.UpdateJobStartTime = null);
        updateQuery = updateQuery.Set(x => x.UpdateJobQueueTime, record.UpdateJobQueueTime = null);
        updateQuery = updateQuery.Set(x => x.UpdateJobEndTime, record.UpdateJobEndTime = DateTimeOffset.UtcNow);
        updateQuery = updateQuery.Set(x => x.UpdateJobLastResult, record.UpdateJobLastResult = updateResult);

        var result = await updateQuery.UpdateAsync();
    }

    [Queue(AccountQueue1)]
    public async Task OnAccountUpdate(long id, PerformContext context)
    {
        var record = await _services.GetRequiredService<AccountServices>().TryGetAccountRecord(id);
        if (record == null || context == null)
        {
#if DEBUG
            return;
#endif

            throw new NotImplementedException();
        }

        await using var database = _databaseProvider.GetDatabase();
        var requiresUpdate = await OnUpdateStarted(database, record, context);
        if (!requiresUpdate)
        {
            return;
        }

        var result = await _accountUpdateHandler.TryUpdate(id, database, record);

        await OnUpdateFinished(database, record, result);
    }

    [Queue(CharacterQueue1)]
    public Task OnCharacterUpdate1(long id, PerformContext context)
    {
        return OnCharacterUpdate(id, context);
    }

    [Queue(CharacterQueue2)]
    public Task OnCharacterUpdate2(long id, PerformContext context)
    {
        return OnCharacterUpdate(id, context);
    }

    private async Task OnCharacterUpdate(long id, PerformContext context)
    {
        var record = await _services.GetRequiredService<CharacterServices>().TryGetCharacterRecord(id);
        if (record == null || context == null)
        {
#if DEBUG
            return;
#endif
            throw new NotImplementedException();
        }

        await using var database = _databaseProvider.GetDatabase();
        var requiresUpdate = await OnUpdateStarted(database, record, context);
        if (!requiresUpdate)
        {
            return;
        }

        var result = await _characterUpdateHandler.TryUpdate(id, database, record);

        await OnUpdateFinished(database, record, result);
    }
}