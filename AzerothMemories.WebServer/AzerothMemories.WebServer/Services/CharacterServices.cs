using AzerothMemories.WebServer.Blizzard.Models.ProfileApi;
using AzerothMemories.WebServer.Services.Updates;

namespace AzerothMemories.WebServer.Services;

[RegisterComputeService]
[RegisterAlias(typeof(IAccountServices))]
public class CharacterServices : ICharacterServices
{
    private readonly BlizzardUpdateHandler _updateHandler;

    public CharacterServices(IServiceProvider serviceProvider)
    {
        _updateHandler = serviceProvider.GetRequiredService<BlizzardUpdateHandler>();
    }

    [ComputeMethod]
    public virtual async Task<CharacterRecord> TryGetCharacterRecord(long id)
    {
        //await using var dbContext = CreateDbContext();
        //var character = await dbContext.Characters.AsQueryable()
        //    .Where(a => a.Id == id)
        //    .FirstOrDefaultAsync();

        //return character;

        throw new NotImplementedException();
    }

    public async Task OnAccountUpdate(long accountId, string characterRef, AccountCharacter accountCharacter)
    {
        //await using var dbContext = CreateDbContext(true);

        //var characterRecord = await dbContext.Characters.AsQueryable().Where(x => x.MoaRef == characterRef).FirstOrDefaultAsync();
        //if (characterRecord == null)
        //{
        //    characterRecord = new CharacterRecord
        //    {
        //        CreatedDateTime = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset()
        //    };

        //    await dbContext.Characters.AddAsync(characterRecord);
        //}

        //characterRecord.AccountId = accountId;
        //characterRecord.MoaRef = characterRef;
        //characterRecord.BlizzardId = accountCharacter.Id;
        //characterRecord.RealmId = accountCharacter.Realm.Id;
        //characterRecord.Name = accountCharacter.Name;
        //characterRecord.SearchableName = DatabaseHelpers.GetSearchableName(characterRecord.Name);
        //characterRecord.Race = (byte)accountCharacter.PlayableRace.Id;
        //characterRecord.Class = (byte)accountCharacter.PlayableClass.Id;
        //characterRecord.Gender = accountCharacter.Gender.AsGender();
        //characterRecord.Faction = accountCharacter.Faction.AsFaction();
        //characterRecord.Level = (byte)accountCharacter.Level;

        //await dbContext.SaveChangesAsync();
        //await _updateHandler.TryUpdateCharacter(dbContext, characterRecord);

        //using var computed = Computed.Invalidate();
        //_ = TryGetCharacterRecord(characterRecord.Id);
        //_ = TryGetAllAccountCharacterIds(characterRecord.AccountId);
        throw new NotImplementedException();
    }

    public Task OnCharacterDeleted(long accountId, long characterId, string characterRef)
    {
        throw new NotImplementedException();
    }

    [ComputeMethod]
    public virtual async Task<Dictionary<long, string>> TryGetAllAccountCharacterIds(long accountId)
    {
        //await using var dbContext = CreateDbContext();

        //var query = dbContext.Characters.AsQueryable().Where(x => x.AccountId == accountId).Select(x => new { x.Id, x.MoaRef });
        //var results = await query.ToDictionaryAsync(x => x.Id, x => x.MoaRef);

        //return results;
        throw new NotImplementedException();
    }
}