namespace AzerothMemories.WebServer.Services.Updates;

public class BlizzardUpdateServices : IComputeService
{
    private readonly CommonServices _commonServices;
    private readonly ILogger<BlizzardUpdateServices> _logger;

    private readonly UpdateHandlerBase<AccountRecord>[] _accountHandlers;
    private readonly UpdateHandlerBase<CharacterRecord>[] _characterHandlers;
    private readonly UpdateHandlerBase<GuildRecord>[] _guildHandlers;
    private readonly Duration _defaultDuration = Duration.FromSeconds(2.5);

    public BlizzardUpdateServices(CommonServices commonServices, ILogger<BlizzardUpdateServices> logger)
    {
        _logger = logger;
        _commonServices = commonServices;

        _accountHandlers = new UpdateHandlerBase<AccountRecord>[(int)BlizzardUpdateType.Account_Count];
        AddUpdateHandler(ref _accountHandlers, new UpdateHandlerBase<AccountRecord>(GetBlizzardUpdateHandlerInfo("Account", null)));

        foreach (var blizzardRegion in new[] { BlizzardRegion.China, BlizzardRegion.Europe, BlizzardRegion.Korea, BlizzardRegion.Taiwan, BlizzardRegion.UnitedStates })
        {
            foreach (var realmVersion in BlizzardRealmVersionExt.AllRealmVersions)
            {
                AddUpdateHandler(ref _accountHandlers, new UpdateHandler_Accounts_Blizzard(GetBlizzardUpdateHandlerInfo($"Account_{blizzardRegion.ToString()}", realmVersion), blizzardRegion, realmVersion, this));
            }
        }

        AddUpdateHandler(ref _accountHandlers, new UpdateHandler_Accounts_Patreon(GetBlizzardUpdateHandlerInfo("Account_Patreon", null)));

        _characterHandlers = new UpdateHandlerBase<CharacterRecord>[(int)BlizzardUpdateType.Character_Count];

        AddUpdateHandler(ref _characterHandlers, new UpdateHandler_Characters(GetBlizzardUpdateHandlerInfo("Character", null)));
        AddUpdateHandler(ref _characterHandlers, new UpdateHandler_Characters_Renders(GetBlizzardUpdateHandlerInfo("Character_Renders", null)));
        AddUpdateHandler(ref _characterHandlers, new UpdateHandler_Characters_Achievements(GetBlizzardUpdateHandlerInfo("Character_Achievements", null)));
        AddUpdateHandler(ref _characterHandlers, new UpdateHandler_Characters_Mounts(GetBlizzardUpdateHandlerInfo("Character_Mounts", null)));
        AddUpdateHandler(ref _characterHandlers, new UpdateHandler_Characters_AchievementStatistics(GetBlizzardUpdateHandlerInfo("Character_AchievementStatistics", null)));

        _guildHandlers = new UpdateHandlerBase<GuildRecord>[(int)BlizzardUpdateType.Guild_Count];

        AddUpdateHandler(ref _guildHandlers, new UpdateHandler_Guilds(GetBlizzardUpdateHandlerInfo("Guild", null)));
        AddUpdateHandler(ref _guildHandlers, new UpdateHandler_Guilds_Roster(GetBlizzardUpdateHandlerInfo("Guild_Roster", null)));
        AddUpdateHandler(ref _guildHandlers, new UpdateHandler_Guilds_Achievements(GetBlizzardUpdateHandlerInfo("Guild_Achievements", null)));

        void AddUpdateHandler<TRecord>(ref UpdateHandlerBase<TRecord>[] array, UpdateHandlerBase<TRecord> updateHandler) where TRecord : IBlizzardUpdateRecord
        {
            Exceptions.ThrowIf(array[(int)updateHandler.UpdateType] != null!);
            array[(int)updateHandler.UpdateType] = updateHandler;
        }

        Exceptions.ThrowIf(_accountHandlers.Any(x => x == null!));
        Exceptions.ThrowIf(_characterHandlers.Any(x => x == null!));
        Exceptions.ThrowIf(_guildHandlers.Any(x => x == null!));
    }

    private UpdateHandlerInfo GetBlizzardUpdateHandlerInfo(string prefix, BlizzardRealmVersion? realmVersion)
    {
        var blizzardNamespaceString = string.Empty;
        if (realmVersion != null)
        {
            blizzardNamespaceString = realmVersion.Value.ToUpdateTypePart();
        }

        var enumString = $"{prefix}{blizzardNamespaceString}";
        if (!Enum.TryParse(enumString, out BlizzardUpdateType updateType))
        {
            throw new NotImplementedException();
        }

        var updateTypeString = $"BlizzardUpdateType.{enumString}";

        return new UpdateHandlerInfo(updateType, updateTypeString, _commonServices, _logger);
    }

    public int AccountHandlerCount => _accountHandlers.Length;

    public int CharacterHandlerCount => _characterHandlers.Length;

    public int GuildHandlerCount => _guildHandlers.Length;

    public async Task ExecuteHandlersOnFirstLogin(AppDbContext database, AccountRecord accountRecord, CharacterRecord characterRecord)
    {
        foreach (var characterHandler in _characterHandlers)
        {
            if (characterHandler is IRequiresExecuteOnFirstLogin handler)
            {
                await handler.OnFirstLogin(database, accountRecord, characterRecord).ConfigureAwait(false);
            }
        }
    }

    [CommandHandler]
    public virtual async Task<HttpStatusCode> UpdateCommandHandler(Updates_UpdateRecordCommand command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Invalidation.IsActive)
        {
            var invRecord = context.Operation.Items.Get<Updates_UpdateInvalidateMany>();
            invRecord?.Invalidate(_commonServices);

            return HttpStatusCode.NoContent;
        }

        var temp = new List<int?> { command.AccountId, command.CharacterId, command.GuildId };
        Exceptions.ThrowIf(temp.All(x => x == null));
        Exceptions.ThrowIf(temp.FirstOrDefault(x => x != null) == null);

        using var _ = new MethodTimeLogger(_logger);
        await using var database = await _commonServices.DatabaseHub.CreateOperationDbContext(cancellationToken).ConfigureAwait(false);

        IBlizzardUpdateRecord? mainRecord = null;
        if (command.AccountId.HasValue)
        {
            mainRecord = await database.Accounts.FirstOrDefaultAsync(x => x.Id == command.AccountId.Value, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        else if (command.CharacterId.HasValue)
        {
            var characterRecord = await database.Characters.FirstOrDefaultAsync(x => x.Id == command.CharacterId.Value, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (characterRecord != null && (characterRecord.CharacterStatus == CharacterStatus2.Deleted || characterRecord.CharacterStatus == CharacterStatus2.DeletePending))
            {
                _logger.LogInformation("TryUpdate: Update Failed Id: {RecordId} UpdateRecordId: {UpdateRecordId} Character has status {CharacterStatus}", characterRecord.Id, characterRecord.UpdateRecord?.Id ?? -1, characterRecord.CharacterStatus);
            }
            else
            {
                mainRecord = characterRecord;
            }
        }
        else if (command.GuildId.HasValue)
        {
            mainRecord = await database.Guilds.FirstOrDefaultAsync(x => x.Id == command.GuildId.Value, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        if (mainRecord == null)
        {
            return HttpStatusCode.NoContent;
        }

        var updateTime = _defaultDuration;
        if (mainRecord.UpdateRecord == null || mainRecord.UpdateRecord.Children == null || mainRecord.UpdateRecord.Children.Count < command.RequiredChildRecordCount)
        {
        }
        else if (RecordRequiresUpdate(mainRecord.UpdateRecord, command.ForcedUpdate))
        {
        }
        else
        {
            return HttpStatusCode.NotModified;
        }

        mainRecord.UpdateRecord ??= new BlizzardUpdateRecord();
        mainRecord.UpdateRecord.UpdateStatus = BlizzardUpdateStatus.Queued;
        mainRecord.UpdateRecord.UpdateLastModified = SystemClock.Instance.GetCurrentInstant();

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("TryUpdate: Update Required Id: {RecordId} UpdateRecordId: {UpdateRecordId}", mainRecord.Id, mainRecord.UpdateRecord.Id);

        context.Operation.Items.Set(new Updates_UpdateInvalidateMany(command.AccountId, command.CharacterId, command.GuildId));
        context.Operation.AddEvent(mainRecord.UpdateRecord.GetUpdateCommand(), updateTime.ToTimeSpan());

        return HttpStatusCode.OK;
    }

    private bool RecordRequiresUpdate(BlizzardUpdateRecord? updateRecord, bool forcedUpdate)
    {
        if (updateRecord == null)
        {
            return true;
        }

        if (forcedUpdate)
        {
            return true;
        }

        return updateRecord.UpdateStatus == BlizzardUpdateStatus.None;
    }

    [CommandHandler]
    public virtual async Task<HttpStatusCode> ResetUpdateStatusCommand(Updates_UpdateRecordResetStatusCommand command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Invalidation.IsActive)
        {
            var invRecord = context.Operation.Items.Get<Updates_UpdateInvalidateMany>();
            invRecord?.Invalidate(_commonServices);

            return HttpStatusCode.NoContent;
        }

        var temp = new List<int?> { command.AccountId, command.CharacterId, command.GuildId };
        Exceptions.ThrowIf(temp.All(x => x == null));
        Exceptions.ThrowIf(temp.FirstOrDefault(x => x != null) == null);

        using var _ = new MethodTimeLogger(_logger);
        await using var database = await _commonServices.DatabaseHub.CreateOperationDbContext(cancellationToken).ConfigureAwait(false);

        IBlizzardUpdateRecord? mainRecord = null;
        if (command.AccountId.HasValue)
        {
            mainRecord = await database.Accounts.FirstOrDefaultAsync(x => x.Id == command.AccountId.Value, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        else if (command.CharacterId.HasValue)
        {
            mainRecord = await database.Characters.FirstOrDefaultAsync(x => x.Id == command.CharacterId.Value, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        else if (command.GuildId.HasValue)
        {
            mainRecord = await database.Guilds.FirstOrDefaultAsync(x => x.Id == command.GuildId.Value, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        if (mainRecord == null || mainRecord.UpdateRecord == null)
        {
            return HttpStatusCode.NoContent;
        }

        if (mainRecord.UpdateRecord.UpdateStatus != BlizzardUpdateStatus.Done)
        {
            return HttpStatusCode.NoContent;
        }

        mainRecord.UpdateRecord.UpdateStatus = BlizzardUpdateStatus.None;
        mainRecord.UpdateRecord.UpdateLastModified = SystemClock.Instance.GetCurrentInstant();

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("ResetUpdateStatusCommand: Id: {RecordId} UpdateRecordId: {UpdateRecordId}", mainRecord.Id, mainRecord.UpdateRecord.Id);

        context.Operation.Items.Set(new Updates_UpdateInvalidateMany(command.AccountId, command.CharacterId, command.GuildId));

        return HttpStatusCode.OK;
    }

    [CommandHandler]
    public virtual async Task<HttpStatusCode> UpdateAccount(Updates_UpdateAccountCommand command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Invalidation.IsActive)
        {
            var invRecord = context.Operation.Items.Get<Updates_UpdateAccountInvalidate>();
            if (invRecord != null)
            {
                _ = _commonServices.AdminServices.GetAccountCount();

                _ = _commonServices.AccountServices.DependsOnAccountRecord(invRecord.AccountId);
                _ = _commonServices.AccountServices.DependsOnAccountAchievements(invRecord.AccountId);
                _ = _commonServices.CharacterServices.TryGetAllAccountCharacters(invRecord.AccountId);

                foreach (var characterId in invRecord.CharacterIds.SafeEnumerable())
                {
                    _ = _commonServices.CharacterServices.DependsOnCharacterRecord(characterId);
                }
            }

            return default;
        }

        using var __ = new MethodTimeLogger(_logger);
        await using var database = await _commonServices.DatabaseHub.CreateOperationDbContext(cancellationToken).ConfigureAwait(false);
        var record = await database.Accounts.FirstOrDefaultAsync(x => x.Id == command.AccountId, cancellationToken).ConfigureAwait(false);
        if (record == null)
        {
            return default;
        }

        var resultStatusCode = await RunUpdateHandlers(_accountHandlers, context, database, record, cancellationToken).ConfigureAwait(false);

        var characters = await database.Characters.Where(x => x.AccountId == record.Id).ToDictionaryAsync(x => x.MoaRef, x => x, cancellationToken: cancellationToken).ConfigureAwait(false);
        context.Operation.Items.Set(new Updates_UpdateAccountInvalidate(record.Id, record.FusionId, record.Username, characters.Values.Select(x => x.Id).ToHashSet()));

        return resultStatusCode;
    }

    [CommandHandler]
    public virtual async Task<HttpStatusCode> UpdateCharacter(Updates_UpdateCharacterCommand command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Invalidation.IsActive)
        {
            var invRecord = context.Operation.Items.Get<Character_InvalidateCharacterRecord>();
            if (invRecord != null)
            {
                _ = _commonServices.AdminServices.GetCharacterCount();

                if (invRecord.CharacterId > 0)
                {
                    _ = _commonServices.CharacterServices.DependsOnCharacterRecord(invRecord.CharacterId);
                }

                if (invRecord.AccountId > 0)
                {
                    _ = _commonServices.AccountServices.DependsOnAccountAchievements(invRecord.AccountId);
                    _ = _commonServices.CharacterServices.TryGetAllAccountCharacters(invRecord.AccountId);
                }
            }

            return default;
        }

        using var __ = new MethodTimeLogger(_logger);
        await using var database = await _commonServices.DatabaseHub.CreateOperationDbContext(cancellationToken).ConfigureAwait(false);
        var record = await database.Characters.FirstOrDefaultAsync(x => x.Id == command.CharacterId, cancellationToken).ConfigureAwait(false);
        if (record == null)
        {
            return default;
        }

        var resultStatusCode = await RunUpdateHandlers(_characterHandlers, context, database, record, cancellationToken).ConfigureAwait(false);

        if (record.AccountId.HasValue && resultStatusCode.IsSuccess2())
        {
            await _commonServices.Commander.Call(new Account_AddNewHistoryItem
            {
                AccountId = record.AccountId.Value,
                Type = AccountHistoryType.CharacterUpdated,
                TargetId = record.Id
            }, cancellationToken).ConfigureAwait(false);
        }

        context.Operation.Items.Set(new Character_InvalidateCharacterRecord(record.Id, record.AccountId.GetValueOrDefault()));

        return resultStatusCode;
    }

    [CommandHandler]
    public virtual async Task<HttpStatusCode> UpdateGuild(Updates_UpdateGuildCommand command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Invalidation.IsActive)
        {
            var invRecord = context.Operation.Items.Get<Guild_InvalidateGuildRecord>();
            if (invRecord != null)
            {
                _ = _commonServices.AdminServices.GetGuildCount();
                _ = _commonServices.AdminServices.GetCharacterCount();

                _ = _commonServices.GuildServices.DependsOnGuildRecord(invRecord.GuildId);

                foreach (var characterId in invRecord.CharacterIds)
                {
                    _ = _commonServices.CharacterServices.DependsOnCharacterRecord(characterId);
                }
            }

            return default;
        }

        using var __ = new MethodTimeLogger(_logger);
        await using var database = await _commonServices.DatabaseHub.CreateOperationDbContext(cancellationToken).ConfigureAwait(false);
        var record = await database.Guilds.FirstOrDefaultAsync(x => x.Id == command.GuildId, cancellationToken).ConfigureAwait(false);
        if (record == null)
        {
            return default;
        }

        var resultStatusCode = await RunUpdateHandlers(_guildHandlers, context, database, record, cancellationToken).ConfigureAwait(false);

        var characterQuery = from r in database.Characters
                             where r.GuildId == r.Id
                             select r.Id;

        var characterIds = await characterQuery.ToArrayAsync(cancellationToken).ConfigureAwait(false);

        context.Operation.Items.Set(new Guild_InvalidateGuildRecord(record.Id, characterIds.ToHashSet()));

        return resultStatusCode;
    }

    private async Task<HttpStatusCode> RunUpdateHandlers<TRecord>(UpdateHandlerBase<TRecord>[] allHandlers, CommandContext context, AppDbContext database, TRecord record, CancellationToken cancellationToken) where TRecord : class, IBlizzardUpdateRecord, new()
    {
        if (record.UpdateRecord == null || record.UpdateRecord.Children == null)
        {
            return HttpStatusCode.FailedDependency;
        }

        if (record.UpdateRecord.UpdateStatus != BlizzardUpdateStatus.Queued)
        {
            _logger.LogInformation("RunUpdateHandlers: Run Update Handlers Required Id: {RecordId} UpdateRecordId: {UpdateRecordId} has a status of {UpdateStatus}", record.Id, record.UpdateRecord.Id, record.UpdateRecord.UpdateStatus);
        }

        var requiredChildrenCount = allHandlers.Length;
        var sortedRecords = new BlizzardUpdateChildRecord?[requiredChildrenCount];
        foreach (var childRecord in record.UpdateRecord.Children)
        {
            sortedRecords[(int)childRecord.UpdateType] = childRecord;
        }

        for (var i = 0; i < sortedRecords.Length; i++)
        {
            if (sortedRecords[i] == null)
            {
                sortedRecords[i] = new BlizzardUpdateChildRecord { UpdateType = allHandlers[i].UpdateType, UpdateTypeString = allHandlers[i].UpdateTypeString };
                record.UpdateRecord.Children.Add(sortedRecords[i].ThrowIfNull());
            }
        }

        for (var i = 0; i < allHandlers.Length; i++)
        {
            var updateHandler = allHandlers[i];
            var updateChildRecord = sortedRecords[i].ThrowIfNull();

            Exceptions.ThrowIf(updateChildRecord.UpdateType != updateHandler.UpdateType);

            var updateStatusCode = await updateHandler.TryExecuteOn(database, record, updateChildRecord).ConfigureAwait(false);
            if (updateStatusCode.IsSuccess())
            {
            }
            else if (updateStatusCode == HttpStatusCode.NotModified)
            {
            }
        }

        record.UpdateRecord.UpdateLastModified = SystemClock.Instance.GetCurrentInstant();
        record.UpdateRecord.UpdateJobLastEndTime = record.UpdateRecord.UpdateLastModified;
        record.UpdateRecord.UpdateStatus = BlizzardUpdateStatus.Done;

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        context.Operation.AddEvent(new Updates_UpdateRecordResetStatusCommand(record.UpdateRecord.AccountId, record.UpdateRecord.CharacterId, record.UpdateRecord.GuildId), GetResetTime(record.UpdateRecord));

        return HttpStatusCode.OK;
    }

    private static TimeSpan GetResetTime(BlizzardUpdateRecord updateRecord)
    {
        var durationBetweenUpdates = TimeSpan.FromHours(2);
        if (updateRecord.AccountId.HasValue)
        {
        }
        else if (updateRecord.CharacterId.HasValue)
        {
            durationBetweenUpdates = TimeSpan.FromHours(12);
        }
        else if (updateRecord.GuildId.HasValue)
        {
            durationBetweenUpdates = TimeSpan.FromHours(12);
        }

        if (updateRecord.Children != null && updateRecord.Children.Count > 0 && updateRecord.Children.Any(x => !x.UpdateJobLastResult.IsSuccess()))
        {
            durationBetweenUpdates /= 2;
        }

        return durationBetweenUpdates;
    }
}