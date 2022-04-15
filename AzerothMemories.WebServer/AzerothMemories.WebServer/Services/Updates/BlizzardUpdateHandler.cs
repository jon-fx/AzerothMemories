using System.Collections.Concurrent;

namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class BlizzardUpdateHandler : DbServiceBase<AppDbContext>
{
    private readonly CommonServices _commonServices;
    private readonly BlizzardUpdateServices _blizzardUpdateServices;
    private readonly Duration[] _durationsBetweenUpdates;

    public BlizzardUpdateHandler(IServiceProvider services, CommonServices commonServices, BlizzardUpdateServices blizzardUpdateServices) : base(services)
    {
        _commonServices = commonServices;
        _blizzardUpdateServices = blizzardUpdateServices;

        _durationsBetweenUpdates = new Duration[(int)BlizzardUpdatePriority.Count];
        _durationsBetweenUpdates[(int)BlizzardUpdatePriority.Account] = _commonServices.Config.UpdateAccountDelay;
        _durationsBetweenUpdates[(int)BlizzardUpdatePriority.CharacterHigh] = _commonServices.Config.UpdateCharacterHighDelay;
        _durationsBetweenUpdates[(int)BlizzardUpdatePriority.CharacterMed] = _commonServices.Config.UpdateCharacterMedDelay;
        _durationsBetweenUpdates[(int)BlizzardUpdatePriority.CharacterLow] = _commonServices.Config.UpdateCharacterLowDelay;
        _durationsBetweenUpdates[(int)BlizzardUpdatePriority.Guild] = _commonServices.Config.UpdateGuildDelay;
    }

    public async Task TryUpdate(AccountRecord accountRecord)
    {
        if (accountRecord.UpdateRecord != null && accountRecord.UpdateRecord.UpdateStatus == BlizzardUpdateStatus.None && accountRecord.AuthTokens != null && accountRecord.AuthTokens.Count > 0)
        {
            var mostRecentlyChanged = accountRecord.AuthTokens.Max(x => x.LastUpdateTime);
            var authTokensChanged = mostRecentlyChanged > accountRecord.UpdateRecord.UpdateJobLastEndTime;
            if (authTokensChanged)
            {
                accountRecord.UpdateRecord.UpdateStatus = BlizzardUpdateStatus.Required;
            }
        }

        await TryUpdate(accountRecord, BlizzardUpdatePriority.Account, _blizzardUpdateServices.AccountHandlerCount).ConfigureAwait(false);
    }

    public async Task TryUpdate(CharacterRecord characterRecord, BlizzardUpdatePriority updatePriority)
    {
        await TryUpdate(characterRecord, updatePriority, _blizzardUpdateServices.CharacterHandlerCount).ConfigureAwait(false);
    }

    public async Task TryUpdate(GuildRecord guildRecord, BlizzardUpdatePriority updatePriority)
    {
        await TryUpdate(guildRecord, updatePriority, _blizzardUpdateServices.GuildHandlerCount).ConfigureAwait(false);
    }

    private async Task TryUpdate<TRecord>(TRecord record, BlizzardUpdatePriority updatePriority, int requiredChildrenCount) where TRecord : class, IBlizzardUpdateRecord, new()
    {
        if (record == null)
        {
            return;
        }

        var requiresUpdate = record.UpdateRecord == null || record.UpdateRecord.Children == null || record.UpdateRecord.Children.Count < requiredChildrenCount;
        if (requiresUpdate)
        {
            
        }
        else
        {
            requiresUpdate = RecordRequiresUpdate(record.UpdateRecord, updatePriority, false);
        }

        if (requiresUpdate)
        {
            await using var database = CreateDbContext(true);
            database.Attach(record);

            record.UpdateRecord ??= new BlizzardUpdateRecord();
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

        if (updateRecord.UpdateStatus == BlizzardUpdateStatus.Required)
        {
            return true;
        }

        var now = SystemClock.Instance.GetCurrentInstant();
        var duration = _durationsBetweenUpdates[(int)updatePriority];
        if (updateRecord.UpdateStatus == BlizzardUpdateStatus.None)
        {
            if (updateRecord.Children != null && updateRecord.Children.Count > 0 && updateRecord.Children.Any(x => !x.UpdateJobLastResult.IsSuccess()))
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