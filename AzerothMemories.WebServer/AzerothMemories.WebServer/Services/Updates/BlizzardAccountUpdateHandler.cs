namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class BlizzardAccountUpdateHandler
{
    private readonly IServiceProvider _services;
    private readonly WarcraftClientProvider _warcraftClientProvider;

    public BlizzardAccountUpdateHandler(IServiceProvider services)
    {
        _services = services;
        _warcraftClientProvider = _services.GetRequiredService<WarcraftClientProvider>();
    }

    public async Task<HttpStatusCode> TryUpdate(long id, DatabaseConnection database, AccountRecord record)
    {
        var tasks = new List<Task>();
        var characterServices = _services.GetRequiredService<CharacterServices>();
        var characters = await characterServices.TryGetAllAccountCharacterIds(id);
        var dbCharactersSet = characters.Values.ToHashSet();
        //var result = record.LastUpdateHttpResult;
        var apiCharactersSet = new HashSet<string>();

        using var client = _warcraftClientProvider.Get(record.BlizzardRegionId);
        var accountSummaryResult = await client.GetAccountProfile(record.BattleNetToken, 0 /*record.BlizzardAccountLastModified*/).ConfigureAwait(false);
        if (accountSummaryResult.IsSuccess)
        {
            //await accountGrain.OnAccountUpdate(accountSummaryResult.ResultLastModified.ToUnixTimeMilliseconds());

            foreach (var account in accountSummaryResult.ResultData.WowAccounts)
            {
                foreach (var accountCharacter in account.Characters)
                {
                    var characterRef = MoaRef.GetCharacterRef(record.BlizzardRegionId, accountCharacter.Realm.Slug, accountCharacter.Name, accountCharacter.Id);

                    apiCharactersSet.Add(characterRef.Full);
                    tasks.Add(characterServices.OnAccountUpdate(id, characterRef.Full, accountCharacter));
                }
            }

            dbCharactersSet.ExceptWith(apiCharactersSet);

            if (dbCharactersSet.Count > 0)
            {
                foreach (var characterRef in dbCharactersSet)
                {
                    tasks.Add(characterServices.OnCharacterDeleted(id, new MoaRef(characterRef).Id, characterRef));
                }
            }
        }
        else if (accountSummaryResult.IsNotModified)
        {
        }

        //foreach (var characterRef in dbCharactersSet)
        //{
        //    //var characterGrain = commonServices.ClusterClient.GetGrain<ICharacterGrain>(characterRef);
        //    //tasks.Add(characterGrain.WakeUp());
        //}

        await Task.WhenAll(tasks).ConfigureAwait(false);

        //await accountGrain.OnCharactersChanged();

        return accountSummaryResult.ResultCode;
    }
}