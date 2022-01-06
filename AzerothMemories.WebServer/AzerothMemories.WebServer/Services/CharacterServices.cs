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
        var character = await database.Characters.Where(a => a.Id == id).FirstOrDefaultAsync();

        return character;
    }

    public async Task OnAccountUpdate(long accountId, string characterRef, AccountCharacter accountCharacter)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        var characterRecord = await database.Characters.Where(x => x.MoaRef == characterRef).FirstOrDefaultAsync();
        if (characterRecord == null)
        {
            characterRecord = new CharacterRecord
            {
                MoaRef = characterRef,
                CreatedDateTime = SystemClock.Instance.GetCurrentInstant()
            };

            characterRecord.Id = await database.InsertWithInt64IdentityAsync(characterRecord);
        }

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
            updateQuery = updateQuery.Set(x => x.BlizzardId, characterRecord.BlizzardId);
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

        await _commonServices.BlizzardUpdateHandler.TryUpdateCharacter(database, characterRecord);

        using var computed = Computed.Invalidate();
        _ = TryGetCharacterRecord(characterRecord.Id);
        _ = TryGetAllAccountCharacters(characterRecord.AccountId.GetValueOrDefault());
        _ = TryGetAllAccountCharacterIds(characterRecord.AccountId.GetValueOrDefault());
    }

    public Task OnCharacterDeleted(long accountId, long characterId, string characterRef)
    {
        throw new NotImplementedException();
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

    public void OnCharacterUpdate(CharacterRecord characterRecord)
    {
        OnCharacterUpdate(characterRecord.Id, characterRecord.AccountId.GetValueOrDefault());
    }

    private void OnCharacterUpdate(long characterId, long characterAccountId)
    {
        using var computed = Computed.Invalidate();
        _ = TryGetCharacterRecord(characterId);
        _ = TryGetAllAccountCharacters(characterAccountId);
        _ = TryGetAllAccountCharacterIds(characterAccountId);
    }

    public async Task<bool> TryChangeCharacterAccountSync(Session session, long characterId, bool newValue, CancellationToken cancellationToken = default)
    {
        var accountRecord = await _commonServices.AccountServices.GetCurrentSessionAccountRecord(session, cancellationToken);
        if (accountRecord == null)
        {
            return false;
        }

        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var updateResult = await database.Characters.Where(x => x.Id == characterId && x.AccountId == accountRecord.Id && x.AccountSync == !newValue).AsUpdatable()
            .Set(x => x.AccountSync, newValue)
            .UpdateAsync(cancellationToken);

        if (updateResult == 0)
        {
            return !newValue;
        }

        OnCharacterUpdate(characterId, accountRecord.Id);

        return newValue;
    }
}