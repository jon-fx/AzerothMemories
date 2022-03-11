using System.Collections.Concurrent;

namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class BlizzardUpdateHandler : DbServiceBase<AppDbContext>
{
    private readonly CommonServices _commonServices;

    private readonly Type[] _validRecordTypes;
    private readonly int[] _requiredChildrenCount;
    private readonly Duration[] _durationsBetweenUpdates;

    public BlizzardUpdateHandler(IServiceProvider services, CommonServices commonServices) : base(services)
    {
        _commonServices = commonServices;

        _validRecordTypes = new Type[(int)BlizzardUpdatePriority.Count];
        _validRecordTypes[(int)BlizzardUpdatePriority.Account] = typeof(AccountRecord);
        _validRecordTypes[(int)BlizzardUpdatePriority.CharacterHigh] = typeof(CharacterRecord);
        _validRecordTypes[(int)BlizzardUpdatePriority.CharacterMed] = typeof(CharacterRecord);
        _validRecordTypes[(int)BlizzardUpdatePriority.CharacterLow] = typeof(CharacterRecord);
        _validRecordTypes[(int)BlizzardUpdatePriority.Guild] = typeof(GuildRecord);

        _requiredChildrenCount = new int[(int)BlizzardUpdatePriority.Count];
        _requiredChildrenCount[(int)BlizzardUpdatePriority.Account] = (int)BlizzardUpdateType.Account_Count;
        _requiredChildrenCount[(int)BlizzardUpdatePriority.CharacterHigh] = (int)BlizzardUpdateType.Character_Count;
        _requiredChildrenCount[(int)BlizzardUpdatePriority.CharacterMed] = (int)BlizzardUpdateType.Character_Count;
        _requiredChildrenCount[(int)BlizzardUpdatePriority.CharacterLow] = (int)BlizzardUpdateType.Character_Count;
        _requiredChildrenCount[(int)BlizzardUpdatePriority.Guild] = (int)BlizzardUpdateType.Guild_Count;

        _durationsBetweenUpdates = new Duration[(int)BlizzardUpdatePriority.Count];
        _durationsBetweenUpdates[(int)BlizzardUpdatePriority.Account] = _commonServices.Config.UpdateAccountDelay;
        _durationsBetweenUpdates[(int)BlizzardUpdatePriority.CharacterHigh] = _commonServices.Config.UpdateCharacterHighDelay;
        _durationsBetweenUpdates[(int)BlizzardUpdatePriority.CharacterMed] = _commonServices.Config.UpdateCharacterMedDelay;
        _durationsBetweenUpdates[(int)BlizzardUpdatePriority.CharacterLow] = _commonServices.Config.UpdateCharacterLowDelay;
        _durationsBetweenUpdates[(int)BlizzardUpdatePriority.Guild] = _commonServices.Config.UpdateGuildDelay;
    }

    public Type[] ValidRecordTypes => _validRecordTypes;

    public int[] RequiredChildrenCount => _requiredChildrenCount;

    public async Task TryUpdate<TRecord>(TRecord record, BlizzardUpdatePriority updatePriority) where TRecord : class, IBlizzardUpdateRecord, new()
    {
        if (record == null)
        {
            return;
        }

#if DEBUG
        if (typeof(TRecord) != _validRecordTypes[(int)updatePriority])
        {
            throw new NotImplementedException();
        }
#endif

        await using var database = CreateDbContext(true);
        database.Attach(record);

        var requiresUpdate = false;
        if (record.UpdateRecord == null)
        {
            requiresUpdate = true;
            record.UpdateRecord = new BlizzardUpdateRecord();
        }

        if (!requiresUpdate)
        {
            requiresUpdate = RecordRequiresUpdate(record.UpdateRecord, updatePriority, false);
        }

        if (requiresUpdate)
        {
            record.UpdateRecord.UpdatePriority = updatePriority;
            record.UpdateRecord.UpdateStatus = BlizzardUpdateStatus.Queued;
            record.UpdateRecord.UpdateLastModified = SystemClock.Instance.GetCurrentInstant();

            await database.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    public bool RecordRequiresUpdate(BlizzardUpdateRecord updateRecord, BlizzardUpdatePriority updatePriority, bool inUpdateLoop)
    {
        if (updateRecord == null)
        {
            return false;
        }

        var now = SystemClock.Instance.GetCurrentInstant();
        var duration = _durationsBetweenUpdates[(int)updatePriority];
        if (updateRecord.UpdateStatus == BlizzardUpdateStatus.None)
        {
            if (updateRecord.UpdateJobLastResult.IsFailure())
            {
                duration /= 2;
            }

            if (now > updateRecord.UpdateJobLastEndTime + duration)
            {
                return true;
            }

            return false;
        }

        if (inUpdateLoop)
        {
            return true;
        }

        if (updateRecord.UpdateStatus == BlizzardUpdateStatus.Queued)
        {
            if (updatePriority < updateRecord.UpdatePriority)
            {
                return true;
            }

            return false;
        }

        if (updateRecord.UpdateStatus == BlizzardUpdateStatus.Progress)
        {
            if (now > updateRecord.UpdateLastModified + Duration.FromMinutes(5))
            {
                return true;
            }

            return false;
        }

        throw new NotImplementedException();
    }

    public async Task OnStarting()
    {
        await using var database = CreateDbContext(true);

        var updateRecords = await database.BlizzardUpdates.Where(x => x.UpdateStatus == BlizzardUpdateStatus.Progress).OrderBy(x => x.UpdateLastModified).ToArrayAsync().ConfigureAwait(false);

        await RunUpdatesOn(database, updateRecords).ConfigureAwait(false);
    }

    public async Task OnUpdating()
    {
        await using var database = CreateDbContext(true);

        var updateRecords = await database.BlizzardUpdates.Where(x => x.UpdateStatus == BlizzardUpdateStatus.Queued).OrderBy(x => x.UpdatePriority).ThenBy(x => x.UpdateLastModified).Take(25).ToArrayAsync().ConfigureAwait(false);

        await RunUpdatesOn(database, updateRecords).ConfigureAwait(false);
    }

    private async Task RunUpdatesOn(AppDbContext database, BlizzardUpdateRecord[] updateRecords)
    {
        var queue = new ConcurrentQueue<ICommand<HttpStatusCode>>();
        foreach (var record in updateRecords)
        {
            if (!RecordRequiresUpdate(record, record.UpdatePriority, true))
            {
                continue;
            }

            record.UpdateStatus = BlizzardUpdateStatus.Progress;
            record.UpdateLastModified = SystemClock.Instance.GetCurrentInstant();

            queue.Enqueue(record.GetUpdateCommand());
        }

        await database.SaveChangesAsync().ConfigureAwait(false);

        var tasks = new Task[Environment.ProcessorCount];
        for (var i = 0; i < tasks.Length; i++)
        {
            tasks[i] = RunUpdatesOn(queue);
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private async Task RunUpdatesOn(ConcurrentQueue<ICommand<HttpStatusCode>> commandQueue)
    {
        while (commandQueue.TryDequeue(out var command))
        {
            await _commonServices.Commander.Call(command).ConfigureAwait(false);
        }
    }
}