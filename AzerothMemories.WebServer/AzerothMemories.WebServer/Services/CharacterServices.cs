namespace AzerothMemories.WebServer.Services;

[RegisterComputeService]
[RegisterAlias(typeof(ICharacterServices))]
public class CharacterServices : ICharacterServices
{
    private readonly CommonServices _commonServices;

    public CharacterServices(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    [ComputeMethod]
    public virtual async Task<CharacterRecord> TryGetCharacterRecord(long id)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var record = await database.Characters.Where(a => a.Id == id).FirstOrDefaultAsync();

        if (record != null)
        {
            var moaRef = new MoaRef(record.MoaRef);

            Exceptions.ThrowIf(!moaRef.IsValidCharacter);
            Exceptions.ThrowIf(moaRef.Id != record.BlizzardId);
        }

        return record;
    }

    [ComputeMethod]
    protected virtual async Task<CharacterRecord> GetOrCreateCharacterRecord(string refFull)
    {
        var moaRef = new MoaRef(refFull);
        //Exceptions.ThrowIf(moaRef.IsValidAccount);
        Exceptions.ThrowIf(moaRef.IsValidGuild);
        Exceptions.ThrowIf(moaRef.IsWildCard);

        Exceptions.ThrowIf(!moaRef.IsValidCharacter);

        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var characterId = await (from r in database.Characters
                                 where r.MoaRef == moaRef.Full
                                 select r.Id).FirstOrDefaultAsync();

        if (characterId == 0)
        {
            characterId = await database.InsertWithInt64IdentityAsync(new CharacterRecord
            {
                MoaRef = moaRef.Full,
                BlizzardId = moaRef.Id,
                BlizzardRegionId = moaRef.Region,
                CreatedDateTime = SystemClock.Instance.GetCurrentInstant()
            });
        }

        var characterRecord = await TryGetCharacterRecord(characterId);

        //await _commonServices.BlizzardUpdateHandler.TryUpdateCharacter(database, characterRecord);

        return characterRecord;
    }

    [ComputeMethod]
    public virtual async Task<CharacterRecord> GetOrCreateCharacterRecord(string refFull, BlizzardUpdatePriority priority)
    {
        Exceptions.ThrowIf(priority != BlizzardUpdatePriority.CharacterLow && priority != BlizzardUpdatePriority.CharacterMed && priority != BlizzardUpdatePriority.CharacterHigh);

        var characterRecord = await GetOrCreateCharacterRecord(refFull);

        await _commonServices.BlizzardUpdateHandler.TryUpdate(characterRecord, priority);

        return characterRecord;
    }

    public async Task OnAccountUpdate(long accountId, string characterRef, AccountCharacter accountCharacter)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        var characterRecord = await GetOrCreateCharacterRecord(characterRef, BlizzardUpdatePriority.CharacterHigh);
        var updateQuery = database.GetUpdateQuery(characterRecord, out var changed);
        if (CheckAndChange.Check(ref characterRecord.AccountId, accountId, ref changed))
        {
            updateQuery = updateQuery.Set(x => x.AccountId, characterRecord.AccountId);
        }

        if (CheckAndChange.Check(ref characterRecord.MoaRef, characterRef, ref changed))
        {
            updateQuery = updateQuery.Set(x => x.MoaRef, characterRecord.MoaRef);
        }

        if (CheckAndChange.Check(ref characterRecord.BlizzardId, accountCharacter.Id, ref changed))
        {
            throw new NotImplementedException();
        }

        if (CheckAndChange.Check(ref characterRecord.BlizzardRegionId, new MoaRef(characterRef).Region, ref changed))
        {
            updateQuery = updateQuery.Set(x => x.BlizzardRegionId, characterRecord.BlizzardRegionId);
        }

        if (CheckAndChange.Check(ref characterRecord.RealmId, accountCharacter.Realm.Id, ref changed))
        {
            updateQuery = updateQuery.Set(x => x.RealmId, characterRecord.RealmId);
        }

        if (CheckAndChange.Check(ref characterRecord.Name, accountCharacter.Name, ref changed))
        {
            updateQuery = updateQuery.Set(x => x.Name, characterRecord.Name);
        }

        if (CheckAndChange.Check(ref characterRecord.NameSearchable, DatabaseHelpers.GetSearchableName(characterRecord.Name), ref changed))
        {
            updateQuery = updateQuery.Set(x => x.NameSearchable, characterRecord.NameSearchable);
        }

        if (CheckAndChange.Check(ref characterRecord.Race, (byte)accountCharacter.PlayableRace.Id, ref changed))
        {
            updateQuery = updateQuery.Set(x => x.Race, characterRecord.Race);
        }

        if (CheckAndChange.Check(ref characterRecord.Class, (byte)accountCharacter.PlayableClass.Id, ref changed))
        {
            updateQuery = updateQuery.Set(x => x.Class, characterRecord.Class);
        }

        if (CheckAndChange.Check(ref characterRecord.Gender, accountCharacter.Gender.AsGender(), ref changed))
        {
            updateQuery = updateQuery.Set(x => x.Gender, characterRecord.Gender);
        }

        if (CheckAndChange.Check(ref characterRecord.Faction, accountCharacter.Faction.AsFaction(), ref changed))
        {
            updateQuery = updateQuery.Set(x => x.Faction, characterRecord.Faction);
        }

        if (CheckAndChange.Check(ref characterRecord.Level, (byte)accountCharacter.Level, ref changed))
        {
            updateQuery = updateQuery.Set(x => x.Level, characterRecord.Level);
        }

        if (changed)
        {
            await updateQuery.UpdateAsync();
        }

        await _commonServices.BlizzardUpdateHandler.TryUpdate(database, characterRecord, BlizzardUpdatePriority.CharacterHigh);

        OnCharacterUpdate(characterRecord.Id, characterRecord.AccountId.GetValueOrDefault());
    }

    [ComputeMethod]
    public virtual async Task<Dictionary<long, string>> TryGetAllAccountCharacterIds(long accountId)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        var query = database.Characters.Where(x => x.AccountId == accountId).Select(x => new { x.Id, x.MoaRef });
        var results = await query.ToDictionaryAsync(x => x.Id, x => x.MoaRef);

        return results;
    }

    [ComputeMethod]
    public virtual async Task<Dictionary<long, CharacterViewModel>> TryGetAllAccountCharacters(long accountId)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        var query = database.Characters.Where(x => x.AccountId == accountId);
        var results = await query.ToDictionaryAsync(x => x.Id, x => x.CreateViewModel());

        return results;
    }

    public async Task OnCharacterUpdate(DatabaseConnection database, CharacterRecord characterRecord)
    {
        if (characterRecord.AccountId.HasValue)
        {
            await _commonServices.AccountServices.TestingHistory(database, new AccountHistoryRecord
            {
                AccountId = characterRecord.AccountId.Value,
                CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                Type = AccountHistoryType.CharacterUpdated,
                TargetId = characterRecord.Id
            });
        }

        OnCharacterUpdate(characterRecord.Id, characterRecord.AccountId.GetValueOrDefault());
    }

    private void OnCharacterUpdate(long characterId, long characterAccountId)
    {
        using var computed = Computed.Invalidate();
        _ = TryGetCharacterRecord(characterId);
        _ = TryGetAllAccountCharacters(characterAccountId);
        _ = TryGetAllAccountCharacterIds(characterAccountId);
        _ = _commonServices.TagServices.TryGetUserTagInfo(PostTagType.Character, characterId);
    }

    public async Task<bool> TryChangeCharacterAccountSync(Session session, long characterId, bool newValue)
    {
        var accountId = await _commonServices.AccountServices.TryGetActiveAccountId(session);
        if (accountId == 0)
        {
            return false;
        }

        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var updateResult = await database.Characters.Where(x => x.Id == characterId && x.AccountId == accountId && x.AccountSync == !newValue).AsUpdatable()
            .Set(x => x.AccountSync, newValue)
            .UpdateAsync();

        if (updateResult == 0)
        {
            return !newValue;
        }

        OnCharacterUpdate(characterId, accountId);

        return newValue;
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

        //await using var database = _commonServices.DatabaseProvider.GetDatabase();
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

    public async Task<bool> TrySetCharacterDeleted(Session session, long characterId)
    {
        var accountId = await _commonServices.AccountServices.TryGetActiveAccountId(session);
        if (accountId == 0)
        {
            return false;
        }

        var characterRecord = await TryGetCharacterRecord(characterId);
        if (characterRecord == null)
        {
            return false;
        }

        if (characterRecord.AccountId != accountId)
        {
            return false;
        }

        if (characterRecord.CharacterStatus != CharacterStatus2.MaybeDeleted)
        {
            return false;
        }

        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        await database.GetUpdateQuery(characterRecord, out _).Set(x => x.CharacterStatus, CharacterStatus2.DeletePending).UpdateAsync();

        OnCharacterUpdate(characterRecord.Id, characterRecord.AccountId.GetValueOrDefault());

        return true;
    }

    public async Task<bool> TrySetCharacterRenamedOrTransferred(Session session, long oldCharacterId, long newCharacterId)
    {
        var accountId = await _commonServices.AccountServices.TryGetActiveAccountId(session);
        if (accountId == 0)
        {
            return false;
        }

        var oldCharacterRecord = await TryGetCharacterRecord(oldCharacterId);
        if (oldCharacterRecord == null || oldCharacterRecord.AccountId != accountId)
        {
            return false;
        }

        var newCharacterRecord = await TryGetCharacterRecord(newCharacterId);
        if (newCharacterRecord == null || newCharacterRecord.AccountId != accountId)
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

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        await database.GetUpdateQuery(oldCharacterRecord, out _).Set(x => x.CharacterStatus, CharacterStatus2.RenamedOrTransferred).UpdateAsync();

        var oldTag = PostTagInfo.GetTagString(PostTagType.Character, oldCharacterRecord.Id);
        var newTag = PostTagInfo.GetTagString(PostTagType.Character, newCharacterRecord.Id);

        await database.Posts.Where(x => x.PostAvatar == oldTag).Set(x => x.PostAvatar, newTag).UpdateAsync();
        await database.PostTags.Where(x => x.TagString == oldTag).Set(x => x.TagString, newTag).Set(x => x.TagId, newCharacterRecord.Id).UpdateAsync();

        transaction.Complete();

        OnCharacterUpdate(oldCharacterRecord.Id, oldCharacterRecord.AccountId.GetValueOrDefault());
        OnCharacterUpdate(newCharacterRecord.Id, newCharacterRecord.AccountId.GetValueOrDefault());

        return true;
    }

    [ComputeMethod]
    protected virtual async Task<MoaRef> GetFullCharacterRef(BlizzardRegion region, string realmSlug, string characterName)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        var moaRef = MoaRef.GetCharacterRef(region, realmSlug, characterName, -1);
        var query = from r in database.Characters
                    where Sql.Like(r.MoaRef, moaRef.GetLikeQuery())
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