using Hangfire.Server;

namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class BlizzardUpdateHandler
{
    private const string Progress = "P-";

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
    private readonly Duration[] _durationsBetweenUpdates;

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

        _durationsBetweenUpdates = new Duration[(int)BlizzardUpdatePriority.Count];
        _durationsBetweenUpdates[(int)BlizzardUpdatePriority.Account] = Duration.FromMinutes(10);
        _durationsBetweenUpdates[(int)BlizzardUpdatePriority.CharacterHigh] = Duration.FromHours(1);
        _durationsBetweenUpdates[(int)BlizzardUpdatePriority.CharacterMed] = Duration.FromHours(5);
        _durationsBetweenUpdates[(int)BlizzardUpdatePriority.CharacterLow] = Duration.FromHours(10);
        _durationsBetweenUpdates[(int)BlizzardUpdatePriority.Guild] = Duration.FromDays(1);
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
        if (!RecordRequiresUpdate<TRecord>(database, record.Id, priority))
        {
            return;
        }

        var jobId = callback(record.Id);
        var updateQuery = database.GetTable<TRecord>().Where(x => x.Id == record.Id && x.UpdateJob == null).Set(x => x.UpdateJob, jobId);
        var result = await updateQuery.UpdateAsync();
        if (result == 0)
        {
            return;
        }

        record.UpdateJob = jobId;
    }

    private bool RecordRequiresUpdate<TRecord>(DatabaseConnection database, long id, BlizzardUpdatePriority blizzardUpdatePriority) where TRecord : class, IBlizzardUpdateRecord
    {
        var now = SystemClock.Instance.GetCurrentInstant();
        var duration = _durationsBetweenUpdates[(int)blizzardUpdatePriority];

        var record = (from r in database.GetTable<TRecord>()
                      where r.Id == id
                      select new { r.Id, r.UpdateJob, r.UpdateJobEndTime, r.UpdateJobLastResult }).First();

        if (record.UpdateJob == null)
        {
            if (record.UpdateJobLastResult.IsFailure())
            {
                duration /= 2;
            }

            if (now > record.UpdateJobEndTime + duration)
            {
                return true;
            }
        }
        else if (record.UpdateJob.StartsWith(Progress) && now > record.UpdateJobEndTime + duration / 2)
        {
            return ShouldRequeue(record.UpdateJob.Replace(Progress, ""));
        }
        else if (now > record.UpdateJobEndTime + duration * 2)
        {
            return ShouldRequeue(record.UpdateJob);
        }

        return false;
    }

    private bool ShouldRequeue(string updateJob)
    {
        var connection = JobStorage.Current.GetConnection();
        var jobData = connection.GetJobData(updateJob);
        if (jobData == null)
        {
            return true;
        }

        var stateName = jobData.State;
        if (stateName == Hangfire.States.SucceededState.StateName)
        {
            return true;
        }

        if (stateName == Hangfire.States.FailedState.StateName)
        {
            return true;
        }

        if (stateName == Hangfire.States.DeletedState.StateName)
        {
            return true;
        }

        return false;
    }

    private async Task<bool> OnUpdateStarted<TRecord>(DatabaseConnection database, TRecord record, PerformContext context) where TRecord : class, IBlizzardUpdateRecord
    {
        var jobId = context.BackgroundJob.Id;
        if (record.UpdateJob != jobId)
        {
            return false;
        }

        var updateQuery = database.GetTable<TRecord>().Where(x => x.Id == record.Id && x.UpdateJob == jobId).Set(x => x.UpdateJob, $"{Progress}{jobId}");
        var result = await updateQuery.UpdateAsync();
        if (result == 0)
        {
            return false;
        }

        record.UpdateJob = $"{Progress}{jobId}";
        return true;
    }

    private async Task OnUpdateFinished<TRecord>(DatabaseConnection database, PerformContext context, TRecord record, HttpStatusCode updateResult) where TRecord : class, IBlizzardUpdateRecord
    {
        Exceptions.ThrowIf(record.UpdateJob != $"{Progress}{context.BackgroundJob.Id}");

        var updateQuery = database.GetUpdateQuery(record, out _);
        updateQuery = updateQuery.Set(x => x.UpdateJob, record.UpdateJob = null);
        updateQuery = updateQuery.Set(x => x.UpdateJobEndTime, record.UpdateJobEndTime = SystemClock.Instance.GetCurrentInstant());
        updateQuery = updateQuery.Set(x => x.UpdateJobLastResult, record.UpdateJobLastResult = updateResult);

        var result = await updateQuery.UpdateAsync();
    }

    [Queue(AccountQueue1)]
    public async Task OnAccountUpdate(long id, PerformContext context)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var record = await database.Accounts.FirstOrDefaultAsync(x => x.Id == id);
        if (record == null || context == null)
        {
            return;
        }

        var requiresUpdate = await OnUpdateStarted(database, record, context);
        if (requiresUpdate)
        {
            var result = await _accountUpdateHandler.TryUpdate(id, database, record);
            await OnUpdateFinished(database, context, record, result);
        }
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
        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var record = await database.Characters.FirstOrDefaultAsync(x => x.Id == id);
        if (record == null || context == null)
        {
            return;
        }

        var requiresUpdate = await OnUpdateStarted(database, record, context);
        if (requiresUpdate)
        {
            var result = await _characterUpdateHandler.TryUpdate(id, database, record);
            await OnUpdateFinished(database, context, record, result);
        }
    }

    [Queue(GuildQueue1)]
    public async Task OnGuildUpdate(long id, PerformContext context)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var record = await database.Guilds.FirstOrDefaultAsync(x => x.Id == id);
        if (record == null || context == null)
        {
            return;
        }

        var requiresUpdate = await OnUpdateStarted(database, record, context);
        if (requiresUpdate)
        {
            var result = await _guildUpdateHandler.TryUpdate(id, database, record);
            await OnUpdateFinished(database, context, record, result);
        }
    }
}