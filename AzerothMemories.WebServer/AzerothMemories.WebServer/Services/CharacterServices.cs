namespace AzerothMemories.WebServer.Services;

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
    public virtual async Task<CharacterRecord?> TryGetCharacterRecord(int id)
    {
        using var _ = new MethodTimeLogger(_logger);
        await DependsOnCharacterRecord(id).ConfigureAwait(false);

        await using var database = await _commonServices.DatabaseHub.CreateDbContext().ConfigureAwait(false);
        var record = await database.Characters.FirstOrDefaultAsync(a => a.Id == id).ConfigureAwait(false);

        if (record != null)
        {
            var moaRef = new MoaRef(record.MoaRef);

            Exceptions.ThrowIf(!moaRef.IsValidCharacter);
            Exceptions.ThrowIf(moaRef.Id != record.BlizzardId);

            //await _commonServices.BlizzardUpdateHandler.TryUpdate(record).ConfigureAwait(false);
        }

        return record;
    }

    [ComputeMethod]
    public virtual async Task<CharacterRecord?> TryGetCharacterRecord(int id, bool enqueueUpdate)
    {
        using var _ = new MethodTimeLogger(_logger);

        var record = await TryGetCharacterRecord(id).ConfigureAwait(false);
        if (record != null)
        {
            var moaRef = new MoaRef(record.MoaRef);

            Exceptions.ThrowIf(!moaRef.IsValidCharacter);
            Exceptions.ThrowIf(moaRef.Id != record.BlizzardId);

            if (enqueueUpdate)
            {
                await _commonServices.BlizzardUpdateHandler.TryUpdate(record).ConfigureAwait(false);
            }
        }

        return record;
    }

    [ComputeMethod]
    public virtual async Task<CharacterRecord> GetOrCreateCharacterRecord(string refFull)
    {
        using var _ = new MethodTimeLogger(_logger);
        var moaRef = new MoaRef(refFull);
        Exceptions.ThrowIf(moaRef.IsValidGuild);
        Exceptions.ThrowIf(moaRef.IsWildCard);
        Exceptions.ThrowIf(!moaRef.IsValidCharacter);

        await using var database = await _commonServices.DatabaseHub.CreateDbContext(true).ConfigureAwait(false);
        var characterRecord = await (from r in database.Characters
                                     where r.MoaRef == moaRef.Full
                                     select r).FirstOrDefaultAsync().ConfigureAwait(false);

        if (characterRecord == null)
        {
            characterRecord = new CharacterRecord
            {
                MoaRef = moaRef.Full,
                Name = moaRef.Name,
                NameSearchable = DatabaseHelpers.GetSearchableName(moaRef.Name),
                BlizzardId = moaRef.Id,
                BlizzardRegionId = moaRef.Region,
                BlizzardRealmVersionId = moaRef.RealmVersion,
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
    public virtual async Task<CharacterRecord> GetOrCreateCharacterRecord(string refFull, bool enqueueUpdate)
    {
        using var _ = new MethodTimeLogger(_logger);

        var characterRecord = await GetOrCreateCharacterRecord(refFull).ConfigureAwait(false);

        if (enqueueUpdate)
        {
            await _commonServices.BlizzardUpdateHandler.TryUpdate(characterRecord).ConfigureAwait(false);
        }

        return characterRecord;
    }

    [ComputeMethod]
    public virtual async Task<Dictionary<int, CharacterViewModel>> TryGetAllAccountCharacters(int accountId)
    {
        using var _ = new MethodTimeLogger(_logger);
        //await _commonServices.AccountServices.DependsOnAccountRecord(accountId);

        await using var database = await _commonServices.DatabaseHub.CreateDbContext().ConfigureAwait(false);

        var allCharacters = await database.Characters.Where(x => x.AccountId == accountId).ToArrayAsync().ConfigureAwait(false);
        var results = new Dictionary<int, CharacterViewModel>();
        foreach (var characterRecord in allCharacters)
        {
            await DependsOnCharacterRecord(characterRecord.Id).ConfigureAwait(false);
            await _commonServices.BlizzardUpdateHandler.TryUpdate(characterRecord).ConfigureAwait(false);

            results.Add(characterRecord.Id, characterRecord.CreateViewModel());
        }

        return results;
    }

    [CommandHandler]
    public virtual async Task<bool> TryChangeCharacterAccountSync(Character_TryChangeCharacterAccountSync command, CancellationToken cancellationToken = default)
    {
        using var _ = new MethodTimeLogger(_logger);
        return await CharacterServices_TryChangeCharacterAccountSync.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<CharacterAccountViewModel> TryGetCharacter(Session session, int characterId, bool enqueueUpdate)
    {
        using var _ = new MethodTimeLogger(_logger);
        var results = new CharacterAccountViewModel();

        var characterRecord = await TryGetCharacterRecord(characterId, enqueueUpdate).ConfigureAwait(false);
        if (characterRecord == null)
        {
        }
        else if (characterRecord.AccountSync && characterRecord.AccountId is > 0)
        {
            results.AccountViewModel = await _commonServices.AccountServices.TryGetAccountById(session, characterRecord.AccountId.Value).ConfigureAwait(false);
            results.CharacterViewModel = results.AccountViewModel.GetCharactersSafeAllVersions().FirstOrDefault(x => x.Id == characterRecord.Id);
        }
        else
        {
            results.CharacterViewModel = characterRecord.CreateViewModel();
        }

        return results;
    }

    [ComputeMethod]
    public virtual async Task<CharacterAccountViewModel?> TryGetCharacter(Session session, BlizzardRegion region, BlizzardRealmVersion realmVersion, string realmSlug, string characterName, bool enqueueUpdate)
    {
        using var _ = new MethodTimeLogger(_logger);
        if (region is <= 0 or >= BlizzardRegion.Count || string.IsNullOrWhiteSpace(realmSlug) || string.IsNullOrWhiteSpace(characterName))
        {
            return null;
        }

        var validRealm = await _commonServices.TagServices.IsValidRealmInfo(region, realmVersion, realmSlug).ConfigureAwait(false);
        if (!validRealm)
        {
            return null;
        }

        if (characterName.Length > 50)
        {
            return null;
        }

        var characterRef = await GetFullCharacterRef(region, realmVersion, realmSlug, characterName).ConfigureAwait(false);
        if (characterRef == null)
        {
            return null;
        }

        var characterRecord = await GetOrCreateCharacterRecord(characterRef.Full, enqueueUpdate).ConfigureAwait(false);
        if (characterRecord == null)
        {
            return null;
        }

        return await TryGetCharacter(session, characterRecord.Id, enqueueUpdate).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<bool> TrySetCharacterDeleted(Character_TrySetCharacterDeleted command, CancellationToken cancellationToken = default)
    {
        using var _ = new MethodTimeLogger(_logger);
        return await CharacterServices_TrySetCharacterDeleted.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<bool> TrySetCharacterRenamedOrTransferred(Character_TrySetCharacterRenamedOrTransferred command, CancellationToken cancellationToken = default)
    {
        using var _ = new MethodTimeLogger(_logger);
        return await CharacterServices_TrySetCharacterRenamedOrTransferred.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [ComputeMethod]
    protected virtual async Task<MoaRef?> GetFullCharacterRef(BlizzardRegion region, BlizzardRealmVersion realmVersion, string realmSlug, string characterName)
    {
        using var _ = new MethodTimeLogger(_logger);
        await using var database = await _commonServices.DatabaseHub.CreateDbContext().ConfigureAwait(false);

        var validRealm = await _commonServices.TagServices.IsValidRealmInfo(region, realmVersion, realmSlug).ConfigureAwait(false);
        if (!validRealm)
        {
            return null;
        }

        var moaRef = MoaRef.GetCharacterRef(region, realmVersion, realmSlug, characterName, -1);
        var query = from r in database.Characters
                    where r.MoaRef.StartsWith(moaRef.GetLikeQuery())
                    select new { r.Id, r.AccountId, r.MoaRef, r.CharacterStatus, r.BlizzardRealmVersionId };

        var allResults = await query.IgnoreAutoIncludes().AsNoTracking().ToArrayAsync().ConfigureAwait(false);
        if (allResults.Length == 0)
        {
        }
        else
        {
            var firstOrDefault = allResults.FirstOrDefault(x => x.CharacterStatus == CharacterStatus2.None && x.BlizzardRealmVersionId == realmVersion);
            if (firstOrDefault != null)
            {
                return new MoaRef(firstOrDefault.MoaRef);
            }
        }

        using var client = _commonServices.HttpClientProvider.GetWarcraftClient(region);
        var statusResult = await client.GetCharacterStatusAsync(realmVersion, realmSlug, characterName).ConfigureAwait(false);
        if (statusResult.IsSuccess && statusResult.ResultData != null && statusResult.ResultData.IsValid && statusResult.ResultData.Id > 0)
        {
            return MoaRef.GetCharacterRef(region, realmVersion, realmSlug, characterName, statusResult.ResultData.Id);
        }

        var sortedResults = allResults.Where(x => x.BlizzardRealmVersionId == realmVersion).OrderByDescending(x => x.Id).ToArray();
        if (sortedResults.Length > 0)
        {
            return new MoaRef(sortedResults[0].MoaRef);
        }

        return null;
    }
}