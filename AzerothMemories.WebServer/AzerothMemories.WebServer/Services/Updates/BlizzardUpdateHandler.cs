using System.Collections.Concurrent;

namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class BlizzardUpdateHandler
{
    private readonly CommonServices _commonServices;
    private readonly ILogger<BlizzardUpdateHandler> _logger;
    private readonly BlizzardUpdateServices _blizzardUpdateServices;
    private readonly Duration[] _durationsBetweenUpdates;

    public BlizzardUpdateHandler(CommonServices commonServices, ILogger<BlizzardUpdateHandler> logger, BlizzardUpdateServices blizzardUpdateServices)
    {
        _logger = logger;
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
            await using var database = await _commonServices.DatabaseHub.CreateDbContext(true).ConfigureAwait(false);
            database.Attach(record);

            record.UpdateRecord ??= new BlizzardUpdateRecord();
            record.UpdateRecord.UpdatePriority = updatePriority;
            record.UpdateRecord.UpdateStatus = BlizzardUpdateStatus.Queued;
            record.UpdateRecord.UpdateLastModified = SystemClock.Instance.GetCurrentInstant();

            await database.SaveChangesAsync().ConfigureAwait(false);

            _logger.LogInformation("TryUpdate: Update Required Id: {RecordId} UpdateRecordId: {UpdateRecordId} UpdatePriority: {UpdatePriority}", record.Id, record.UpdateRecord.Id, record.UpdateRecord.UpdatePriority);
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

            return now > updateRecord.UpdateJobLastEndTime + duration;
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
        using var _ = new MethodTimeLogger(_logger);
        await using var database = await _commonServices.DatabaseHub.CreateDbContext(true).ConfigureAwait(false);

        var updateRecords = await database.BlizzardUpdates.Where(x => x.UpdateStatus == BlizzardUpdateStatus.Progress).OrderBy(x => x.UpdateLastModified).ToArrayAsync().ConfigureAwait(false);

        await RunUpdatesOn(database, updateRecords).ConfigureAwait(false);
    }

    public async Task OnUpdating()
    {
        using var _ = new MethodTimeLogger(_logger);
        await using var database = await _commonServices.DatabaseHub.CreateDbContext(true).ConfigureAwait(false);

        var updateRecords = await database.BlizzardUpdates.Where(x => x.UpdateStatus == BlizzardUpdateStatus.Queued).OrderBy(x => x.UpdatePriority).ThenBy(x => x.UpdateLastModified).Take(5).ToArrayAsync().ConfigureAwait(false);

        await RunUpdatesOn(database, updateRecords).ConfigureAwait(false);
    }

    private async Task RunUpdatesOn(AppDbContext database, BlizzardUpdateRecord[] updateRecords)
    {
        using var _ = new MethodTimeLogger(_logger);
        if (updateRecords == null || updateRecords.Length == 0)
        {
            return;
        }

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

            _logger.LogInformation("RunUpdatesOn: Update Required UpdateRecordId: {UpdateRecordId} UpdatePriority: {UpdatePriority}", record.Id, record.UpdatePriority);
        }

        if (queue.IsEmpty)
        {
            return;
        }

        await database.SaveChangesAsync().ConfigureAwait(false);

        var tasks = new Task[1];
        for (var i = 0; i < tasks.Length; i++)
        {
            tasks[i] = RunUpdatesOn(queue);
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private async Task RunUpdatesOn(ConcurrentQueue<ICommand<HttpStatusCode>> commandQueue)
    {
        using var _ = new MethodTimeLogger(_logger);
        while (commandQueue.TryDequeue(out var command))
        {
            await _commonServices.Commander.Call(command).ConfigureAwait(false);
        }
    }
}