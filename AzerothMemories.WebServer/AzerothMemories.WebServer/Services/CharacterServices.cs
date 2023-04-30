namespace AzerothMemories.WebServer.Services;

[RegisterComputeService]
[RegisterAlias(typeof(ICharacterServices))]
public class CharacterServices : ICharacterServices
{
    private readonly ILogger<CharacterServices> _logger;
    private readonly CommonServices _commonServices;

    public CharacterServices(ILogger<CharacterServices> logger, CommonServices commonServices)
    {
        _logger = logger;
        _commonServices = commonServices;
    }

    [ComputeMethod]
    public virtual Task<int> DependsOnCharacterRecord(int characterId)
    {
        return Task.FromResult(characterId);
    }

    [ComputeMethod]
    public virtual async Task<CharacterRecord> TryGetCharacterRecord(int id)
    {
        await using var database = _commonServices.DatabaseHub.CreateDbContext();
        var record = await database.Characters.FirstOrDefaultAsync(a => a.Id == id).ConfigureAwait(false);

        if (record != null)
        {
            var moaRef = new MoaRef(record.MoaRef);

            Exceptions.ThrowIf(!moaRef.IsValidCharacter);
            Exceptions.ThrowIf(moaRef.Id != record.BlizzardId);

            await DependsOnCharacterRecord(record.Id).ConfigureAwait(false);
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

        await using var database = _commonServices.DatabaseHub.CreateDbContext(true);
        var characterRecord = await (from r in database.Characters
                                     where r.MoaRef == moaRef.Full
                                     select r).FirstOrDefaultAsync().ConfigureAwait(false);

        if (characterRecord == null)
        {
            characterRecord = new CharacterRecord
            {
                MoaRef = moaRef.Full,
                BlizzardId = moaRef.Id,
                BlizzardRegionId = moaRef.Region,
                CreatedDateTime = SystemClock.Instance.GetCurrentInstant()
            };

            await database.Characters.AddAsync(characterRecord).ConfigureAwait(false);
            await database.SaveChangesAsync().ConfigureAwait(false);
        }

        Exceptions.ThrowIf(characterRecord.Id == 0);

        await DependsOnCharacterRecord(characterRecord.Id).ConfigureAwait(false);
        return characterRecord;
    }

    [ComputeMethod]
    public virtual async Task<CharacterRecord> GetOrCreateCharacterRecord(string refFull, BlizzardUpdatePriority priority)
    {
        Exceptions.ThrowIf(priority != BlizzardUpdatePriority.CharacterLow && priority != BlizzardUpdatePriority.CharacterMed && priority != BlizzardUpdatePriority.CharacterHigh);

        var characterRecord = await GetOrCreateCharacterRecord(refFull).ConfigureAwait(false);

        if (_commonServices.Config.UpdateSkipCharactersOnLowPriority && priority == BlizzardUpdatePriority.CharacterLow)
        {
        }
        else
        {
            await _commonServices.BlizzardUpdateHandler.TryUpdate(characterRecord, priority).ConfigureAwait(false);
        }

        return characterRecord;
    }

    [ComputeMethod]
    public virtual async Task<Dictionary<int, CharacterViewModel>> TryGetAllAccountCharacters(int accountId)
    {
        //await _commonServices.AccountServices.DependsOnAccountRecord(accountId);

        await using var database = _commonServices.DatabaseHub.CreateDbContext();

        var allCharacters = await database.Characters.Where(x => x.AccountId == accountId).ToArrayAsync().ConfigureAwait(false);
        var results = new Dictionary<int, CharacterViewModel>();
        foreach (var characterRecord in allCharacters)
        {
            await DependsOnCharacterRecord(characterRecord.Id).ConfigureAwait(false);
            await _commonServices.BlizzardUpdateHandler.TryUpdate(characterRecord, BlizzardUpdatePriority.CharacterHigh).ConfigureAwait(false);

            results.Add(characterRecord.Id, characterRecord.CreateViewModel());
        }

        return results;
    }

    [CommandHandler]
    public virtual async Task<bool> TryChangeCharacterAccountSync(Character_TryChangeCharacterAccountSync command, CancellationToken cancellationToken = default)
    {
        return await CharacterServices_TryChangeCharacterAccountSync.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<CharacterAccountViewModel> TryGetCharacter(Session session, int characterId)
    {
        var results = new CharacterAccountViewModel();
        var characterRecord = await TryGetCharacterRecord(characterId).ConfigureAwait(false);
        if (characterRecord == null)
        {
        }
        else if (characterRecord.AccountSync && characterRecord.AccountId is > 0)
        {
            results.AccountViewModel = await _commonServices.AccountServices.TryGetAccountById(session, characterRecord.AccountId.Value).ConfigureAwait(false);
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

        var validRealmSlug = await _commonServices.TagServices.IsValidRealmSlug(realmSlug).ConfigureAwait(false);
        if (!validRealmSlug)
        {
            return null;
        }

        if (characterName.Length > 50)
        {
            return null;
        }

        var characterRef = await GetFullCharacterRef(region, realmSlug, characterName).ConfigureAwait(false);
        if (characterRef == null)
        {
            return null;
        }

        var characterRecord = await GetOrCreateCharacterRecord(characterRef.Full, BlizzardUpdatePriority.CharacterMed).ConfigureAwait(false);

        return await TryGetCharacter(session, characterRecord.Id).ConfigureAwait(false);
    }

    public async Task<bool> TryEnqueueUpdate(Session session, BlizzardRegion region, string realmSlug, string characterName)
    {
        var characterRef = await GetFullCharacterRef(region, realmSlug, characterName).ConfigureAwait(false);
        if (characterRef == null)
        {
            return false;
        }

        await GetOrCreateCharacterRecord(characterRef.Full, BlizzardUpdatePriority.CharacterMed).ConfigureAwait(false);

        return true;
    }

    [CommandHandler]
    public virtual async Task<bool> TrySetCharacterDeleted(Character_TrySetCharacterDeleted command, CancellationToken cancellationToken = default)
    {
        return await CharacterServices_TrySetCharacterDeleted.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<bool> TrySetCharacterRenamedOrTransferred(Character_TrySetCharacterRenamedOrTransferred command, CancellationToken cancellationToken = default)
    {
        return await CharacterServices_TrySetCharacterRenamedOrTransferred.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [ComputeMethod]
    protected virtual async Task<MoaRef> GetFullCharacterRef(BlizzardRegion region, string realmSlug, string characterName)
    {
        await using var database = _commonServices.DatabaseHub.CreateDbContext();

        var moaRef = MoaRef.GetCharacterRef(region, realmSlug, characterName, -1);
        var query = from r in database.Characters
                    where r.MoaRef.StartsWith(moaRef.GetLikeQuery())
                    select new { r.Id, r.AccountId, r.MoaRef, r.CharacterStatus };

        var allResults = await query.ToArrayAsync().ConfigureAwait(false);
        if (allResults.Length == 0)
        {
        }
        else
        {
            var firstOrDefault = allResults.FirstOrDefault(x => x.CharacterStatus == CharacterStatus2.None);
            if (firstOrDefault != null)
            {
                return new MoaRef(firstOrDefault.MoaRef);
            }
        }

        using var client = _commonServices.HttpClientProvider.GetWarcraftClient(region);
        var statusResult = await client.GetCharacterStatusAsync(realmSlug, characterName).ConfigureAwait(false);
        if (statusResult.IsSuccess && statusResult.ResultData != null && statusResult.ResultData.IsValid && statusResult.ResultData.Id > 0)
        {
            return MoaRef.GetCharacterRef(region, realmSlug, characterName, statusResult.ResultData.Id);
        }

        if (allResults.Length > 0)
        {
            var sortedResults = allResults.OrderByDescending(x => x.Id).ToArray();

            return new MoaRef(sortedResults[0].MoaRef);
        }

        return null;
    }
}