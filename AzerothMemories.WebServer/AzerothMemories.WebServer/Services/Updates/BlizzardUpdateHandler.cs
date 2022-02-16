using Hangfire;
using Hangfire.Server;

namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class BlizzardUpdateHandler : DbServiceBase<AppDbContext>
{
    private const string Progress = "P-";
    private const string Queued = "Q-";

    public const string AbstractQueue = "a-abstract";
    public const string AccountQueue1 = "a-account";
    public const string CharacterQueue1 = "b-character";
    public const string CharacterQueue2 = "c-character";
    public const string CharacterQueue3 = "d-character";
    public const string GuildQueue1 = "e-guild";
    public static readonly string[] AllQueues = { AbstractQueue, AccountQueue1, CharacterQueue1, CharacterQueue2, CharacterQueue3, GuildQueue1 };

    private readonly CommonServices _commonServices;

    private readonly Type[] _validRecordTypes;
    private readonly Func<long, string>[] _callbacks;
    private readonly Duration[] _durationsBetweenUpdates;

    public BlizzardUpdateHandler(IServiceProvider services, CommonServices commonServices, IBackgroundJobClient backgroundJob, IRecurringJobManager recurringJobManager) : base(services)
    {
        _commonServices = commonServices;

        _callbacks = new Func<long, string>[(int)BlizzardUpdatePriority.Count];
        _callbacks[(int)BlizzardUpdatePriority.Account] = id => backgroundJob.Enqueue(() => OnAccountUpdate(id, null));
        _callbacks[(int)BlizzardUpdatePriority.CharacterHigh] = id => backgroundJob.Enqueue(() => OnCharacterUpdate1(id, null));
        _callbacks[(int)BlizzardUpdatePriority.CharacterMed] = id => backgroundJob.Enqueue(() => OnCharacterUpdate2(id, null));
        _callbacks[(int)BlizzardUpdatePriority.CharacterLow] = id => backgroundJob.Enqueue(() => OnCharacterUpdate3(id, null));
        _callbacks[(int)BlizzardUpdatePriority.Guild] = id => backgroundJob.Enqueue(() => OnGuildUpdate(id, null));

        _validRecordTypes = new Type[(int)BlizzardUpdatePriority.Count];
        _validRecordTypes[(int)BlizzardUpdatePriority.Account] = typeof(AccountRecord);
        _validRecordTypes[(int)BlizzardUpdatePriority.CharacterHigh] = typeof(CharacterRecord);
        _validRecordTypes[(int)BlizzardUpdatePriority.CharacterMed] = typeof(CharacterRecord);
        _validRecordTypes[(int)BlizzardUpdatePriority.CharacterLow] = typeof(CharacterRecord);
        _validRecordTypes[(int)BlizzardUpdatePriority.Guild] = typeof(GuildRecord);

        _durationsBetweenUpdates = new Duration[(int)BlizzardUpdatePriority.Count];
        _durationsBetweenUpdates[(int)BlizzardUpdatePriority.Account] = _commonServices.Config.UpdateAccountDelay;
        _durationsBetweenUpdates[(int)BlizzardUpdatePriority.CharacterHigh] = _commonServices.Config.UpdateCharacterHighDelay;
        _durationsBetweenUpdates[(int)BlizzardUpdatePriority.CharacterMed] = _commonServices.Config.UpdateCharacterMedDelay;
        _durationsBetweenUpdates[(int)BlizzardUpdatePriority.CharacterLow] = _commonServices.Config.UpdateCharacterLowDelay;
        _durationsBetweenUpdates[(int)BlizzardUpdatePriority.Guild] = _commonServices.Config.UpdateGuildDelay;
    }

    public async Task TryUpdate<TRecord>(TRecord record, BlizzardUpdatePriority updatePriority) where TRecord : class, IBlizzardUpdateRecord, new()
    {
        if (!RecordRequiresUpdate(record, updatePriority))
        {
            return;
        }

        if (typeof(TRecord) != _validRecordTypes[(int)updatePriority])
        {
            throw new NotImplementedException();
        }

        await using var database = CreateDbContext(true);

        var jobName = $"{Queued}{SystemClock.Instance.GetCurrentInstant().ToUnixTimeMilliseconds()}";

        var tempRecord = await database.Set<TRecord>().FirstOrDefaultAsync(x => x.Id == record.Id);
        if (tempRecord == null)
        {
            return;
        }

        if (tempRecord.UpdateJob == record.UpdateJob)
        {
            tempRecord.UpdateJob = jobName;

            await database.SaveChangesAsync();

            _callbacks[(int)updatePriority](record.Id);
        }
    }

    private bool RecordRequiresUpdate<TRecord>(TRecord record, BlizzardUpdatePriority blizzardUpdatePriority) where TRecord : class, IBlizzardUpdateRecord, new()
    {
        var now = SystemClock.Instance.GetCurrentInstant();
        var duration = _durationsBetweenUpdates[(int)blizzardUpdatePriority];
        if (record == null)
        {
            return false;
        }

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
        else if (record.UpdateJob.StartsWith(Queued))
        {
            if (long.TryParse(record.UpdateJob.Split('-')[1], out var timeStamp) && now > Instant.FromUnixTimeMilliseconds(timeStamp) + duration * 2)
            {
                return true;
            }
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

    private async Task<bool> OnUpdateStarted<TRecord>(DbSet<TRecord> dbSet, long id, PerformContext context) where TRecord : class, IBlizzardUpdateRecord, new()
    {
        var jobId = context.BackgroundJob.Id;
        var result = await dbSet.FirstOrDefaultAsync(x => x.Id == id);
        if (result == null)
        {
            return false;
        }

        if (!result.UpdateJob.StartsWith(Queued))
        {
            return false;
        }

        result.UpdateJob = $"{Progress}{jobId}";
        return true;
    }

    [Queue(AccountQueue1)]
    public async Task OnAccountUpdate(long id, PerformContext context)
    {
        await using var database = CreateDbContext(true);

        var requiresUpdate = await OnUpdateStarted(database.Accounts, id, context);
        if (requiresUpdate)
        {
            await _commonServices.Commander.Call(new Updates_UpdateAccountCommand(id));
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
        await using var database = CreateDbContext(true);

        var requiresUpdate = await OnUpdateStarted(database.Characters, id, context);
        if (requiresUpdate)
        {
            await _commonServices.Commander.Call(new Updates_UpdateCharacterCommand(id));
        }
    }

    [Queue(GuildQueue1)]
    public async Task OnGuildUpdate(long id, PerformContext context)
    {
        await using var database = CreateDbContext(true);

        var requiresUpdate = await OnUpdateStarted(database.Guilds, id, context);
        if (requiresUpdate)
        {
            await _commonServices.Commander.Call(new Updates_UpdateGuildCommand(id));
        }
    }
}