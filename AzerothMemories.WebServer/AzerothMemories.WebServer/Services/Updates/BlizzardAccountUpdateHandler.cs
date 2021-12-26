using System.Net;

namespace AzerothMemories.WebServer.Services.Updates
{
    internal sealed class BlizzardAccountUpdateHandler
    {
        private readonly WarcraftClientProvider _warcraftClientProvider;

        public BlizzardAccountUpdateHandler(IServiceProvider services)
        {
            _warcraftClientProvider = services.GetRequiredService<WarcraftClientProvider>();
        }

        public async Task<HttpStatusCode> TryUpdate(long id, DbContext dbContext, AccountRecord record)
        {
            //var characters = await db.Characters.Select(x => new { x.Id, x.AccountId, Ref = x.MoaRef }).Where(x => x.AccountId == record.Id).ToDictionaryAsync(x => x.Id, x => x.Ref);
            //var dbCharactersSet = characters.Values.ToHashSet(
            //var result = record.LastUpdateHttpResult;
            //var apiCharactersSet = new HashSet<string>();

            using var client = _warcraftClientProvider.Get(record.BlizzardRegion);
            var accountSummaryResult = await client.GetAccountProfile(record.BattleNetToken, 0 /*record.BlizzardAccountLastModified*/).ConfigureAwait(false);
            if (accountSummaryResult.IsSuccess)
            {
                //await accountGrain.OnAccountUpdate(accountSummaryResult.ResultLastModified.ToUnixTimeMilliseconds());

                //foreach (var account in accountSummaryResult.ResultData.WowAccounts)
                //{
                //    foreach (var accountCharacter in account.Characters)
                //    {
                //        var characterRef = MoaRef.GetCharacterRef(record.RegionId, accountCharacter.Realm.Slug, accountCharacter.Name, accountCharacter.Id);
                //        var characterGrain = commonServices.ClusterClient.GetGrain<ICharacterGrain>(characterRef.Full);

                //        apiCharactersSet.Add(characterRef.Full);
                //        tasks.Add(characterGrain.OnAccountUpdate(accountGrain, record.Id, accountCharacter));
                //    }
                //}

                //dbCharactersSet.ExceptWith(apiCharactersSet);

                //if (dbCharactersSet.Count > 0)
                //{
                //    foreach (var characterRef in dbCharactersSet)
                //    {
                //        var characterGrain = commonServices.ClusterClient.GetGrain<ICharacterGrain>(characterRef);
                //        tasks.Add(characterGrain.OnCharacterDeleted(accountGrain, record.Id));
                //    }
                //}
            }
            else if (accountSummaryResult.IsNotModified)
            {
            }

            //foreach (var characterRef in dbCharactersSet)
            //{
            //    var characterGrain = commonServices.ClusterClient.GetGrain<ICharacterGrain>(characterRef);
            //    tasks.Add(characterGrain.WakeUp());
            //}

            //await Task.WhenAll(tasks).ConfigureAwait(false);

            //await accountGrain.OnCharactersChanged();

            return accountSummaryResult.ResultCode;
        }
    }
}