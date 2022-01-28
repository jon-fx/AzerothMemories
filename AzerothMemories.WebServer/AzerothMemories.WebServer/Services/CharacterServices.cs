namespace AzerothMemories.WebServer.Services;

[RegisterComputeService]
[RegisterAlias(typeof(ICharacterServices))]
public class CharacterServices : DbServiceBase<AppDbContext>, ICharacterServices
{
    private readonly CommonServices _commonServices;

    public CharacterServices(IServiceProvider services, CommonServices commonServices) : base(services)
    {
        _commonServices = commonServices;
    }

    [ComputeMethod]
    public virtual async Task<CharacterRecord> TryGetCharacterRecord(long id)
    {
        await using var database = CreateDbContext();
        var record = await database.Characters.FirstOrDefaultAsync(a => a.Id == id);

        if (record != null)
        {
            var moaRef = new MoaRef(record.MoaRef);

            Exceptions.ThrowIf(!moaRef.IsValidCharacter);
            Exceptions.ThrowIf(moaRef.Id != record.BlizzardId);
        }

        return record;
    }

    [ComputeMethod]
    public virtual async Task<CharacterRecord> GetOrCreateCharacterRecord(string refFull)
    {
        var moaRef = new MoaRef(refFull);
        Exceptions.ThrowIf(moaRef.IsValidGuild);
        Exceptions.ThrowIf(moaRef.IsWildCard);
        Exceptions.ThrowIf(!moaRef.IsValidCharacter);

        await using var database = CreateDbContext(true);
        var characterId = await (from r in database.Characters
                                 where r.MoaRef == moaRef.Full
                                 select r.Id).FirstOrDefaultAsync();

        if (characterId == 0)
        {
            var record = new CharacterRecord
            {
                MoaRef = moaRef.Full,
                BlizzardId = moaRef.Id,
                BlizzardRegionId = moaRef.Region,
                CreatedDateTime = SystemClock.Instance.GetCurrentInstant()
            };

            await database.Characters.AddAsync(record);
            await database.SaveChangesAsync();

            characterId = record.Id;
        }

        var characterRecord = await TryGetCharacterRecord(characterId);
        return characterRecord;
    }

    [ComputeMethod]
    public virtual async Task<CharacterRecord> GetOrCreateCharacterRecord(string refFull, BlizzardUpdatePriority priority)
    {
        Exceptions.ThrowIf(priority != BlizzardUpdatePriority.CharacterLow && priority != BlizzardUpdatePriority.CharacterMed && priority != BlizzardUpdatePriority.CharacterHigh);

        var characterRecord = await GetOrCreateCharacterRecord(refFull);

        if (CommandContext.Current == null)
        {
            throw new NotImplementedException();
        }

        await _commonServices.BlizzardUpdateHandler.TryUpdate(characterRecord, priority);

        return characterRecord;
    }

    //[ComputeMethod]
    //public virtual async Task<Dictionary<long, string>> TryGetAllAccountCharacterIds(long accountId)
    //{
    //    await using var database = CreateDbContext();

    //    var query = database.Characters.Where(x => x.AccountId == accountId).Select(x => new { x.Id, x.MoaRef });
    //    var results = await query.ToDictionaryAsync(x => x.Id, x => x.MoaRef);

    //    return results;
    //}

    [ComputeMethod]
    public virtual async Task<Dictionary<long, CharacterViewModel>> TryGetAllAccountCharacters(long accountId)
    {
        await using var database = CreateDbContext();

        var allCharacters = await database.Characters.Where(x => x.AccountId == accountId).ToArrayAsync();
        var results = new Dictionary<long, CharacterViewModel>();
        foreach (var characterRecord in allCharacters)
        {
            await _commonServices.BlizzardUpdateHandler.TryUpdate(characterRecord, BlizzardUpdatePriority.CharacterHigh);

            results.Add(characterRecord.Id, characterRecord.CreateViewModel());
        }

        return results;
    }

    [CommandHandler]
    public virtual async Task<bool> TryChangeCharacterAccountSync(Character_TryChangeCharacterAccountSync command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Character_InvalidateCharacterRecord>();
            InvalidateHelpers.InvalidateRecord(_commonServices, invRecord);

            return default;
        }

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session);
        if (activeAccount == null)
        {
            return false;
        }

        await using var database = await CreateCommandDbContext(cancellationToken);
        var updateResult = await database.Characters.Where(x => x.Id == command.CharacterId && x.AccountId == activeAccount.Id && x.AccountSync == !command.NewValue)
                                                    .UpdateAsync(r => new CharacterRecord { AccountSync = command.NewValue }, cancellationToken);
        if (updateResult == 0)
        {
            return !command.NewValue;
        }

        context.Operation().Items.Set(new Character_InvalidateCharacterRecord(command.CharacterId, activeAccount.Id));

        return command.NewValue;
    }

    [ComputeMethod]
    public virtual async Task<CharacterAccountViewModel> TryGetCharacter(Session session, long characterId)
    {
        var results = new CharacterAccountViewModel();
        var characterRecord = await TryGetCharacterRecord(characterId);
        if (characterRecord == null)
        {
        }
        else if (characterRecord.AccountSync && characterRecord.AccountId is > 0)
        {
            results.AccountViewModel = await _commonServices.AccountServices.TryGetAccountById(session, characterRecord.AccountId.Value);
            results.CharacterViewModel = results.AccountViewModel.GetCharactersSafe().FirstOrDefault(x => x.Id == characterRecord.Id);
        }
        else
        {
            results.CharacterViewModel = characterRecord.CreateViewModel();
        }

        return results;
    }

    [ComputeMethod]
    public virtual async Task<CharacterAccountViewModel> TryGetCharacter(Session session, BlizzardRegion region, string realmSlug, string characterName)
    {
        if (region is <= 0 or >= BlizzardRegion.Count || string.IsNullOrWhiteSpace(realmSlug) || string.IsNullOrWhiteSpace(characterName))
        {
            return null;
        }

        var realmId = await _commonServices.TagServices.TryGetRealmId(realmSlug);
        if (realmId == 0)
        {
            return null;
        }

        if (characterName.Length > 50)
        {
            return null;
        }

        var characterRef = await GetFullCharacterRef(region, realmSlug, characterName);
        if (characterRef == null)
        {
            return null;
        }

        var characterRecord = await GetOrCreateCharacterRecord(characterRef.Full, BlizzardUpdatePriority.CharacterMed);

        return await TryGetCharacter(session, characterRecord.Id);
    }

    public async Task<bool> TryEnqueueUpdate(Session session, BlizzardRegion region, string realmSlug, string characterName)
    {
        var characterRef = await GetFullCharacterRef(region, realmSlug, characterName);
        if (characterRef == null)
        {
            return false;
        }

        await GetOrCreateCharacterRecord(characterRef.Full, BlizzardUpdatePriority.CharacterMed);

        return true;
    }

    [CommandHandler]
    public virtual async Task<bool> TrySetCharacterDeleted(Character_TrySetCharacterDeleted command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Character_InvalidateCharacterRecord>();
            InvalidateHelpers.InvalidateRecord(_commonServices, invRecord);

            return default;
        }

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session);
        if (activeAccount == null)
        {
            return false;
        }

        var characterRecord = await TryGetCharacterRecord(command.CharacterId);
        if (characterRecord == null)
        {
            return false;
        }

        if (characterRecord.AccountId != activeAccount.Id)
        {
            return false;
        }

        if (characterRecord.CharacterStatus != CharacterStatus2.MaybeDeleted)
        {
            return false;
        }

        await using var database = await CreateCommandDbContext(cancellationToken);
        await database.Characters.Where(x => x.Id == command.CharacterId && x.AccountId == activeAccount.Id).UpdateAsync(r => new CharacterRecord { CharacterStatus = CharacterStatus2.DeletePending }, cancellationToken);

        context.Operation().Items.Set(new Character_InvalidateCharacterRecord(command.CharacterId, characterRecord.AccountId.GetValueOrDefault()));

        return true;
    }

    [CommandHandler]
    public virtual async Task<bool> TrySetCharacterRenamedOrTransferred(Character_TrySetCharacterRenamedOrTransferred command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Character_TrySetCharacterRenamedOrTransferredInvalidate>();
            if (invRecord != null)
            {
                InvalidateHelpers.InvalidateRecord(_commonServices, new Character_InvalidateCharacterRecord(invRecord.OldCharacterId, invRecord.OldAccountId));
                InvalidateHelpers.InvalidateRecord(_commonServices, new Character_InvalidateCharacterRecord(invRecord.NewCharacterId, invRecord.NewAccountId));

                if (invRecord.PostIds != null)
                {
                    foreach (var postId in invRecord.PostIds)
                    {
                        _commonServices.PostServices.InvalidatePostRecordAndTags(postId);
                    }
                }
            }

            return default;
        }

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session);
        if (activeAccount == null)
        {
            return false;
        }

        var oldCharacterRecord = await TryGetCharacterRecord(command.OldCharacterId);
        if (oldCharacterRecord == null || oldCharacterRecord.AccountId != activeAccount.Id)
        {
            return false;
        }

        var newCharacterRecord = await TryGetCharacterRecord(command.NewCharacterId);
        if (newCharacterRecord == null || newCharacterRecord.AccountId != activeAccount.Id)
        {
            return false;
        }

        if (oldCharacterRecord.AccountId != newCharacterRecord.AccountId)
        {
            return false;
        }

        if (oldCharacterRecord.Class != newCharacterRecord.Class)
        {
            return false;
        }

        await using var database = await CreateCommandDbContext(cancellationToken);

        await database.Characters.Where(x => x.Id == oldCharacterRecord.Id && x.AccountId == activeAccount.Id).UpdateAsync(r => new CharacterRecord { CharacterStatus = CharacterStatus2.RenamedOrTransferred }, cancellationToken);

        var oldTag = PostTagInfo.GetTagString(PostTagType.Character, oldCharacterRecord.Id);
        var newTag = PostTagInfo.GetTagString(PostTagType.Character, newCharacterRecord.Id);

        var allPostIds = await database.Posts.Where(x => x.PostAvatar == oldTag).Select(x => x.Id).ToArrayAsync(cancellationToken);
        var allTagsPostIds = await database.PostTags.Where(x => x.TagString == oldTag).Select(x => x.PostId).ToArrayAsync(cancellationToken);

        await database.Posts.Where(x => x.PostAvatar == oldTag).UpdateAsync(r => new PostRecord { PostAvatar = newTag }, cancellationToken);
        await database.PostTags.Where(x => x.TagString == oldTag).UpdateAsync(r => new PostTagRecord { TagString = newTag, TagId = newCharacterRecord.Id }, cancellationToken);

        var hashSet = new HashSet<long>(allPostIds);
        hashSet.UnionWith(allTagsPostIds);

        var item = new Character_TrySetCharacterRenamedOrTransferredInvalidate(oldCharacterRecord.AccountId.GetValueOrDefault(), oldCharacterRecord.Id, newCharacterRecord.AccountId.GetValueOrDefault(), newCharacterRecord.Id, hashSet);
        context.Operation().Items.Set(item);

        return true;
    }

    [ComputeMethod]
    protected virtual async Task<MoaRef> GetFullCharacterRef(BlizzardRegion region, string realmSlug, string characterName)
    {
        await using var database = CreateDbContext();

        var moaRef = MoaRef.GetCharacterRef(region, realmSlug, characterName, -1);
        var query = from r in database.Characters
                    where EF.Functions.Like(r.MoaRef, moaRef.GetLikeQuery())
                    select new { r.Id, r.AccountId, r.MoaRef };

        var result = await query.FirstOrDefaultAsync();
        if (result != null)
        {
            return new MoaRef(result.MoaRef);
        }

        using var client = _commonServices.WarcraftClientProvider.Get(region);
        var statusResult = await client.GetCharacterStatusAsync(realmSlug, characterName);
        if (statusResult.IsSuccess && statusResult.ResultData != null && statusResult.ResultData.IsValid && statusResult.ResultData.Id > 0)
        {
            return MoaRef.GetCharacterRef(region, realmSlug, characterName, statusResult.ResultData.Id);
        }

        return null;
    }
}