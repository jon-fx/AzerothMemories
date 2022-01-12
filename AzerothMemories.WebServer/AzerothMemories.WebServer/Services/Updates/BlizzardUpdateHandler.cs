using Hangfire.Server;

namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class BlizzardUpdateHandler
{
    public const string AccountQueue1 = "a-account";
    public const string CharacterQueue1 = "b-character";
    public const string CharacterQueue2 = "c-character";
    public const string CharacterQueue3 = "d-character";
    public const string GuildQueue1 = "e-guild";
    public static readonly string[] AllQueues = { AccountQueue1, CharacterQueue1, CharacterQueue2, CharacterQueue3, GuildQueue1 };

    private readonly CommonServices _commonServices;
    private readonly BlizzardAccountUpdateHandler _accountUpdateHandler;
    private readonly BlizzardCharacterUpdateHandler _characterUpdateHandler;
    private readonly BlizzardGuildUpdateHandler _guildUpdateHandler;

    private readonly Type[] _validRecordTypes;
    private readonly Func<long, string>[] _callbacks;

    public BlizzardUpdateHandler(CommonServices commonServices)
    {
        _commonServices = commonServices;
        _accountUpdateHandler = new BlizzardAccountUpdateHandler(commonServices);
        _characterUpdateHandler = new BlizzardCharacterUpdateHandler(commonServices);
        _guildUpdateHandler = new BlizzardGuildUpdateHandler(commonServices);

        _callbacks = new Func<long, string>[(int)BlizzardUpdatePriority.Count];
        _callbacks[(int)BlizzardUpdatePriority.Account] = id => BackgroundJob.Enqueue(() => OnAccountUpdate(id, null));
        _callbacks[(int)BlizzardUpdatePriority.CharacterHigh] = id => BackgroundJob.Enqueue(() => OnCharacterUpdate1(id, null));
        _callbacks[(int)BlizzardUpdatePriority.CharacterMed] = id => BackgroundJob.Enqueue(() => OnCharacterUpdate2(id, null));
        _callbacks[(int)BlizzardUpdatePriority.CharacterLow] = id => BackgroundJob.Enqueue(() => OnCharacterUpdate3(id, null));
        _callbacks[(int)BlizzardUpdatePriority.Guild] = id => BackgroundJob.Enqueue(() => OnGuildUpdate(id, null));

        _validRecordTypes = new Type[(int)BlizzardUpdatePriority.Count];
        _validRecordTypes[(int)BlizzardUpdatePriority.Account] = typeof(AccountRecord);
        _validRecordTypes[(int)BlizzardUpdatePriority.CharacterHigh] = typeof(CharacterRecord);
        _validRecordTypes[(int)BlizzardUpdatePriority.CharacterMed] = typeof(CharacterRecord);
        _validRecordTypes[(int)BlizzardUpdatePriority.CharacterLow] = typeof(CharacterRecord);
        _validRecordTypes[(int)BlizzardUpdatePriority.Guild] = typeof(GuildRecord);
    }

    public async Task TryUpdate<TRecord>(TRecord record, BlizzardUpdatePriority priority) where TRecord : class, IBlizzardUpdateRecord
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        await TryUpdate(database, record, priority);
    }

    public async Task TryUpdate<TRecord>(DatabaseConnection database, TRecord record, BlizzardUpdatePriority priority) where TRecord : class, IBlizzardUpdateRecord
    {
        if (typeof(TRecord) != _validRecordTypes[(int)priority])
        {
            throw new NotImplementedException();
        }

        var callback = _callbacks[(int)priority];
        await TryUpdate(database, record, priority, callback);
    }

    private async Task TryUpdate<TRecord>(DatabaseConnection database, TRecord record, BlizzardUpdatePriority priority, Func<long, string> callback) where TRecord : class, IBlizzardUpdateRecord
    {
        if (!RecordRequiresUpdate(record))
        {
            return;
        }

        var updateQuery = database.GetUpdateQuery(record, out _);

        updateQuery = updateQuery.Set(x => x.UpdateJob, record.UpdateJob = callback(record.Id));
        updateQuery = updateQuery.Set(x => x.UpdateJobQueueTime, record.UpdateJobQueueTime = SystemClock.Instance.GetCurrentInstant());
        updateQuery = updateQuery.Set(x => x.UpdateJobStartTime, record.UpdateJobStartTime = null);
        updateQuery = updateQuery.Set(x => x.UpdateJobEndTime, record.UpdateJobEndTime = null);

        var result = await updateQuery.UpdateAsync();
    }

    private bool RecordRequiresUpdate(IBlizzardUpdateRecord record)
    {
        var now = SystemClock.Instance.GetCurrentInstant();
        //var isAccount = record is AccountRecord;
        //var isCharacter = record is AccountRecord;
        //var isGuild= record is GuildRecord;

        if (string.IsNullOrWhiteSpace(record.UpdateJob))
        {
            if (!record.UpdateJobQueueTime.HasValue && !record.UpdateJobStartTime.HasValue && !record.UpdateJobEndTime.HasValue)
            {
                return true;
            }

            if (record.UpdateJobLastResult.IsFailure() && record.UpdateJobEndTime.HasValue && now > record.UpdateJobEndTime.Value + Duration.FromHours(1))
            {
                return true;
            }

            if (record.UpdateJobEndTime.HasValue && now > record.UpdateJobEndTime.Value + Duration.FromHours(2))
            {
                return true;
            }
        }
        else if (record.UpdateJobQueueTime.HasValue && now > record.UpdateJobQueueTime.Value + Duration.FromHours(6))
        {
            return true;
        }

        return false;
    }

    private async Task<bool> OnUpdateStarted<TRecord>(DatabaseConnection database, TRecord record, PerformContext context) where TRecord : class, IBlizzardUpdateRecord
    {
        if (record.UpdateJob != context.BackgroundJob.Id)
        {
            return false;
        }

        var updateQuery = database.GetUpdateQuery(record, out _);
        updateQuery = updateQuery.Set(x => x.UpdateJob, record.UpdateJob = string.Empty);
        updateQuery = updateQuery.Set(x => x.UpdateJobQueueTime, record.UpdateJobQueueTime = null);
        updateQuery = updateQuery.Set(x => x.UpdateJobStartTime, record.UpdateJobStartTime = SystemClock.Instance.GetCurrentInstant());
        updateQuery = updateQuery.Set(x => x.UpdateJobEndTime, record.UpdateJobEndTime = null);

        var result = await updateQuery.UpdateAsync();

        return result > 0;
    }

    private async Task OnUpdateFinished<TRecord>(DatabaseConnection database, TRecord record, HttpStatusCode updateResult) where TRecord : class, IBlizzardUpdateRecord
    {
        Exceptions.ThrowIf(record.UpdateJob != string.Empty);
        Exceptions.ThrowIf(record.UpdateJobQueueTime != null);
        Exceptions.ThrowIf(record.UpdateJobStartTime == null);
        Exceptions.ThrowIf(record.UpdateJobEndTime != null);

        var updateQuery = database.GetUpdateQuery(record, out _);
        updateQuery = updateQuery.Set(x => x.UpdateJob, record.UpdateJob = null);
        updateQuery = updateQuery.Set(x => x.UpdateJobStartTime, record.UpdateJobStartTime = null);
        updateQuery = updateQuery.Set(x => x.UpdateJobQueueTime, record.UpdateJobQueueTime = null);
        updateQuery = updateQuery.Set(x => x.UpdateJobEndTime, record.UpdateJobEndTime = SystemClock.Instance.GetCurrentInstant());
        updateQuery = updateQuery.Set(x => x.UpdateJobLastResult, record.UpdateJobLastResult = updateResult);

        var result = await updateQuery.UpdateAsync();
    }

    [Queue(AccountQueue1)]
    public async Task OnAccountUpdate(long id, PerformContext context)
    {
        var record = await _commonServices.AccountServices.TryGetAccountRecord(id);
        if (record == null || context == null)
        {
            return;
        }

        await using var database = _commonServices.DatabaseProvider.GetDatabase();
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

    [Queue(CharacterQueue3)]
    public Task OnCharacterUpdate3(long id, PerformContext context)
    {
        return OnCharacterUpdate(id, context);
    }

    private async Task OnCharacterUpdate(long id, PerformContext context)
    {
        var record = await _commonServices.CharacterServices.TryGetCharacterRecord(id);
        if (record == null || context == null)
        {
            return;
        }

        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var requiresUpdate = await OnUpdateStarted(database, record, context);
        if (!requiresUpdate)
        {
            return;
        }

        var result = await _characterUpdateHandler.TryUpdate(id, database, record);

        await OnUpdateFinished(database, record, result);
    }

    [Queue(GuildQueue1)]
    public async Task OnGuildUpdate(long id, PerformContext context)
    {
        var record = await _commonServices.GuildServices.TryGetGuildRecord(id);
        if (record == null || context == null)
        {
            return;
        }

        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var requiresUpdate = await OnUpdateStarted(database, record, context);
        if (!requiresUpdate)
        {
            return;
        }

        var result = await _guildUpdateHandler.TryUpdate(id, database, record);

        await OnUpdateFinished(database, record, result);
    }
}