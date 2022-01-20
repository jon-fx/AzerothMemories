namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class BlizzardAccountUpdateHandler
{
    private readonly CommonServices _commonServices;

    public BlizzardAccountUpdateHandler(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    public async Task<HttpStatusCode> TryUpdate(long id, DatabaseConnection database, AccountRecord record)
    {
        var result = await TryUpdateInternal(id, database, record);

        _commonServices.AccountServices.OnAccountUpdate(record);

        return result;
    }

    private async Task<HttpStatusCode> TryUpdateInternal(long id, DatabaseConnection database, AccountRecord record)
    {
        var tasks = new List<Task>();
        var characters = await _commonServices.CharacterServices.TryGetAllAccountCharacterIds(id);
        var dbCharactersSet = characters.Values.ToHashSet();
        var apiCharactersSet = new HashSet<string>();

        using var client = _commonServices.WarcraftClientProvider.Get(record.BlizzardRegionId);
        var accountSummaryResult = await client.GetAccountProfile(record.BattleNetToken, 0 /*record.BlizzardAccountLastModified*/).ConfigureAwait(false);
        if (accountSummaryResult.IsSuccess)
        {
            foreach (var account in accountSummaryResult.ResultData.WowAccounts)
            {
                foreach (var accountCharacter in account.Characters)
                {
                    var characterRef = MoaRef.GetCharacterRef(record.BlizzardRegionId, accountCharacter.Realm.Slug, accountCharacter.Name, accountCharacter.Id);

                    apiCharactersSet.Add(characterRef.Full);
                    tasks.Add(_commonServices.CharacterServices.OnAccountUpdate(database, id, characterRef.Full, accountCharacter));
                }
            }

            //var newCharacters = new HashSet<string>(apiCharactersSet);
            //newCharacters.ExceptWith(dbCharactersSet);

            var deletedCharacters = new HashSet<string>(dbCharactersSet);
            deletedCharacters.ExceptWith(apiCharactersSet);

            foreach (var deletedCharacter in deletedCharacters)
            {
                await database.Characters.Where(x => x.MoaRef == deletedCharacter && x.CharacterStatus == CharacterStatus2.None)
                                         .Set(x => x.CharacterStatus, CharacterStatus2.MaybeDeleted)
                                         .UpdateAsync();
            }
        }
        else if (accountSummaryResult.IsNotModified)
        {
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return accountSummaryResult.ResultCode;
    }
}