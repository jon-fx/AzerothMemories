using AzerothMemories.Database;
using AzerothMemories.WebServer.Blizzard.Models.ProfileApi;
using AzerothMemories.WebServer.Services.Updates;

namespace AzerothMemories.WebServer.Services;

[RegisterComputeService]
[RegisterAlias(typeof(IAccountServices))]
public class CharacterServices : ICharacterServices
{
    private readonly DatabaseProvider _databaseProvider;
    private readonly BlizzardUpdateHandler _updateHandler;

    public CharacterServices(IServiceProvider serviceProvider)
    {
        _databaseProvider = serviceProvider.GetRequiredService<DatabaseProvider>();
        _updateHandler = serviceProvider.GetRequiredService<BlizzardUpdateHandler>();
    }

    [ComputeMethod]
    public virtual async Task<CharacterRecord> TryGetCharacterRecord(long id)
    {
        await using var database = _databaseProvider.GetDatabase();
        var character = await database.Characters.Where(a => a.Id == id).FirstOrDefaultAsync();

        return character;
    }

    public async Task OnAccountUpdate(long accountId, string characterRef, AccountCharacter accountCharacter)
    {
        await using var database = _databaseProvider.GetDatabase();

        var characterRecord = await database.Characters.Where(x => x.MoaRef == characterRef).FirstOrDefaultAsync();
        if (characterRecord == null)
        {
            characterRecord = new CharacterRecord
            {
                MoaRef = characterRef,
                CreatedDateTime = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset()
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

        await _updateHandler.TryUpdateCharacter(database, characterRecord);

        using var computed = Computed.Invalidate();
        _ = TryGetCharacterRecord(characterRecord.Id);
        _ = TryGetAllAccountCharacterIds(characterRecord.AccountId);
    }

    public Task OnCharacterDeleted(long accountId, long characterId, string characterRef)
    {
        throw new NotImplementedException();
    }

    [ComputeMethod]
    public virtual async Task<Dictionary<long, string>> TryGetAllAccountCharacterIds(long accountId)
    {
        await using var database = _databaseProvider.GetDatabase();

        var query = database.Characters.Where(x => x.AccountId == accountId).Select(x => new { x.Id, x.MoaRef });
        var results = await query.ToDictionaryAsync(x => x.Id, x => x.MoaRef);

        return results;
    }
}