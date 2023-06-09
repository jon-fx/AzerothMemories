namespace AzerothMemories.WebServer.Services.Updates;

[RegisterComputeService]
public class BlizzardUpdateServices : DbServiceBase<AppDbContext>, IComputeService
{
    private readonly CommonServices _commonServices;

    private readonly UpdateHandlerBase<AccountRecord>[] _accountHandlers;
    private readonly UpdateHandlerBase<CharacterRecord>[] _characterHandlers;
    private readonly UpdateHandlerBase<GuildRecord>[] _guildHandlers;

    public BlizzardUpdateServices(IServiceProvider services, CommonServices commonServices) : base(services)
    {
        _commonServices = commonServices;

        _accountHandlers = new UpdateHandlerBase<AccountRecord>[(int)BlizzardUpdateType.Account_Count];
        AddUpdateHandler(ref _accountHandlers, new UpdateHandlerBase<AccountRecord>(BlizzardUpdateType.Account, _commonServices));
        AddUpdateHandler(ref _accountHandlers, new UpdateHandler_Accounts_Blizzard(BlizzardUpdateType.Account_China, _commonServices, this));
        AddUpdateHandler(ref _accountHandlers, new UpdateHandler_Accounts_Blizzard(BlizzardUpdateType.Account_Europe, _commonServices, this));
        AddUpdateHandler(ref _accountHandlers, new UpdateHandler_Accounts_Blizzard(BlizzardUpdateType.Account_Korea, _commonServices, this));
        AddUpdateHandler(ref _accountHandlers, new UpdateHandler_Accounts_Blizzard(BlizzardUpdateType.Account_Taiwan, _commonServices, this));
        AddUpdateHandler(ref _accountHandlers, new UpdateHandler_Accounts_Blizzard(BlizzardUpdateType.Account_UnitedStates, _commonServices, this));
        AddUpdateHandler(ref _accountHandlers, new UpdateHandler_Accounts_Patreon(_commonServices));

        _characterHandlers = new UpdateHandlerBase<CharacterRecord>[(int)BlizzardUpdateType.Character_Count];
        AddUpdateHandler(ref _characterHandlers, new UpdateHandler_Characters(_commonServices));
        AddUpdateHandler(ref _characterHandlers, new UpdateHandler_Characters_Renders(_commonServices));
        AddUpdateHandler(ref _characterHandlers, new UpdateHandler_Characters_Achievements(_commonServices));

        _guildHandlers = new UpdateHandlerBase<GuildRecord>[(int)BlizzardUpdateType.Guild_Count];
        AddUpdateHandler(ref _guildHandlers, new UpdateHandler_Guilds(_commonServices));
        AddUpdateHandler(ref _guildHandlers, new UpdateHandler_Guilds_Roster(_commonServices));
        AddUpdateHandler(ref _guildHandlers, new UpdateHandler_Guilds_Achievements(_commonServices));

        void AddUpdateHandler<TRecord>(ref UpdateHandlerBase<TRecord>[] array, UpdateHandlerBase<TRecord> updateHandler) where TRecord : IBlizzardUpdateRecord
        {
            Exceptions.ThrowIf(array[(int)updateHandler.UpdateType] != null);
            array[(int)updateHandler.UpdateType] = updateHandler;
        }

        Exceptions.ThrowIf(_accountHandlers.Any(x => x == null));
        Exceptions.ThrowIf(_characterHandlers.Any(x => x == null));
        Exceptions.ThrowIf(_guildHandlers.Any(x => x == null));
    }

    public int AccountHandlerCount => _accountHandlers.Length;

    public int CharacterHandlerCount => _characterHandlers.Length;

    public int GuildHandlerCount => _guildHandlers.Length;

    public async Task ExecuteHandlersOnFirstLogin(CommandContext context, AppDbContext database, AccountRecord accountRecord, CharacterRecord characterRecord)
    {
        foreach (var characterHandler in _characterHandlers)
        {
            if (characterHandler is IRequiresExecuteOnFirstLogin handler)
            {
                await handler.OnFirstLogin(context, database, accountRecord, characterRecord).ConfigureAwait(false);
            }
        }
    }

    [CommandHandler]
    public virtual async Task<HttpStatusCode> UpdateAccount(Updates_UpdateAccountCommand command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Updates_UpdateAccountInvalidate>();
            if (invRecord != null)
            {
                _ = _commonServices.AdminServices.GetAccountCount();

                _ = _commonServices.AccountServices.DependsOnAccountRecord(invRecord.AccountId);
                _ = _commonServices.AccountServices.DependsOnAccountAchievements(invRecord.AccountId);
                _ = _commonServices.CharacterServices.TryGetAllAccountCharacters(invRecord.AccountId);

                foreach (var characterId in invRecord.CharacterIds)
                {
                    _ = _commonServices.CharacterServices.DependsOnCharacterRecord(characterId);
                }
            }

            return default;
        }

        await using var database = await CreateCommandDbContext(cancellationToken).ConfigureAwait(false);
        var record = await database.Accounts.FirstOrDefaultAsync(x => x.Id == command.AccountId, cancellationToken).ConfigureAwait(false);
        if (record == null)
        {
            return default;
        }

        var resultStatusCode = await RunUpdateHandlers(_accountHandlers, context, database, record, cancellationToken).ConfigureAwait(false);
        return resultStatusCode;
    }

    [CommandHandler]
    public virtual async Task<HttpStatusCode> UpdateCharacter(Updates_UpdateCharacterCommand command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Character_InvalidateCharacterRecord>();
            if (invRecord != null)
            {
                _ = _commonServices.AdminServices.GetCharacterCount();

                if (invRecord.CharacterId > 0)
                {
                    _ = _commonServices.CharacterServices.DependsOnCharacterRecord(invRecord.CharacterId);
                    _ = _commonServices.CharacterServices.TryGetCharacterRecord(invRecord.CharacterId);
                }

                if (invRecord.AccountId > 0)
                {
                    _ = _commonServices.AccountServices.DependsOnAccountAchievements(invRecord.AccountId);
                    _ = _commonServices.CharacterServices.TryGetAllAccountCharacters(invRecord.AccountId);
                }
            }

            return default;
        }

        await using var database = await CreateCommandDbContext(cancellationToken).ConfigureAwait(false);
        var record = await database.Characters.FirstOrDefaultAsync(x => x.Id == command.CharacterId, cancellationToken).ConfigureAwait(false);
        if (record == null)
        {
            return default;
        }

        var resultStatusCode = await RunUpdateHandlers(_characterHandlers, context, database, record, cancellationToken).ConfigureAwait(false);

        if (record.AccountId.HasValue)
        {
            await _commonServices.Commander.Call(new Account_AddNewHistoryItem
            {
                AccountId = record.AccountId.Value,
                Type = AccountHistoryType.CharacterUpdated,
                TargetId = record.Id
            }, cancellationToken).ConfigureAwait(false);
        }

        context.Operation().Items.Set(new Character_InvalidateCharacterRecord(record.Id, record.AccountId.GetValueOrDefault()));

        return resultStatusCode;
    }

    [CommandHandler]
    public virtual async Task<HttpStatusCode> UpdateGuild(Updates_UpdateGuildCommand command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Guild_InvalidateGuildRecord>();
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

        await using var database = await CreateCommandDbContext(cancellationToken).ConfigureAwait(false);
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

        context.Operation().Items.Set(new Guild_InvalidateGuildRecord(record.Id, characterIds.ToHashSet()));

        return resultStatusCode;
    }

    private async Task<HttpStatusCode> RunUpdateHandlers<TRecord>(UpdateHandlerBase<TRecord>[] allHandlers, CommandContext context, AppDbContext database, TRecord record, CancellationToken cancellationToken) where TRecord : class, IBlizzardUpdateRecord, new()
    {
        if (!_commonServices.BlizzardUpdateHandler.RecordRequiresUpdate(record.UpdateRecord, record.UpdateRecord.UpdatePriority, true))
        {
            return HttpStatusCode.LoopDetected;
        }

        var requiredChildrenCount = allHandlers.Length;
        var sortedRecords = new BlizzardUpdateChildRecord[requiredChildrenCount];
        foreach (var childRecord in record.UpdateRecord.Children)
        {
            sortedRecords[(int)childRecord.UpdateType] = childRecord;
        }

        for (var i = 0; i < sortedRecords.Length; i++)
        {
            if (sortedRecords[i] == null)
            {
                sortedRecords[i] = new BlizzardUpdateChildRecord { UpdateType = allHandlers[i].UpdateType, UpdateTypeString = allHandlers[i].UpdateTypeString, };
                record.UpdateRecord.Children.Add(sortedRecords[i]);
            }
        }

        var updateStatusCode = HttpStatusCode.OK;
        for (var i = 0; i < allHandlers.Length; i++)
        {
            var updateHandler = allHandlers[i];
            var updateChildRecord = sortedRecords[i];

            Exceptions.ThrowIf(updateChildRecord.UpdateType != updateHandler.UpdateType);

            updateStatusCode = await updateHandler.TryExecuteOn(context, database, record, updateChildRecord).ConfigureAwait(false);

            if (updateStatusCode.IsSuccess())
            {
            }
            else if (updateStatusCode == HttpStatusCode.NotModified)
            {
            }
        }

        record.UpdateRecord.UpdateLastModified = SystemClock.Instance.GetCurrentInstant();
        record.UpdateRecord.UpdateJobLastEndTime = record.UpdateRecord.UpdateLastModified;
        record.UpdateRecord.UpdateStatus = BlizzardUpdateStatus.None;
        record.UpdateRecord.UpdatePriority = BlizzardUpdatePriority.None;

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return updateStatusCode;
    }
}