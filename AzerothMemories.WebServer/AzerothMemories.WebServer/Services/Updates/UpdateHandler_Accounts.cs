namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class UpdateHandler_Accounts : UpdateHandlerBase<AccountRecord, AccountProfileSummary>
{
    public UpdateHandler_Accounts(CommonServices commonServices) : base(BlizzardUpdateType.Account, commonServices)
    {
    }

    protected override async Task<RequestResult<AccountProfileSummary>> TryExecuteRequest(AccountRecord record, Instant blizzardLastModified)
    {
        if (string.IsNullOrWhiteSpace(record.BattleNetToken) || SystemClock.Instance.GetCurrentInstant() >= record.BattleNetTokenExpiresAt.GetValueOrDefault(Instant.FromUnixTimeMilliseconds(0)))
        {
            return new RequestResult<AccountProfileSummary>(HttpStatusCode.Forbidden, null, blizzardLastModified, null);
        }

        using var client = CommonServices.WarcraftClientProvider.Get(record.BlizzardRegionId);
        return await client.GetAccountProfile(record.BattleNetToken, blizzardLastModified).ConfigureAwait(false);
    }

    protected override async Task InternalExecute(CommandContext context, AppDbContext database, AccountRecord record, AccountProfileSummary requestResult)
    {
        var characters = await database.Characters.Where(x => x.AccountId == record.Id).ToDictionaryAsync(x => x.MoaRef, x => x).ConfigureAwait(false);
        var deletedCharactersSets = new Dictionary<string, CharacterRecord>(characters);

        foreach (var account in requestResult.WowAccounts)
        {
            foreach (var accountCharacter in account.Characters)
            {
                var characterRef = MoaRef.GetCharacterRef(record.BlizzardRegionId, accountCharacter.Realm.Slug, accountCharacter.Name, accountCharacter.Id);
                if (!characters.TryGetValue(characterRef.Full, out var characterRecord))
                {
                    characterRecord = await CommonServices.CharacterServices.GetOrCreateCharacterRecord(characterRef.Full).ConfigureAwait(false);

                    database.Characters.Attach(characterRecord);
                }

                if (characterRecord.AccountId.HasValue)
                {
                }
                else
                {
                    await database.Database.ExecuteSqlRawAsync($"UPDATE \"Characters_Achievements\" SET \"AccountId\" = {record.Id} WHERE \"CharacterId\" = {characterRecord.Id} AND \"AccountId\" IS NULL").ConfigureAwait(false);
                }

                characterRecord.AccountId = record.Id;
                characterRecord.MoaRef = characterRef.Full;
                characterRecord.BlizzardId = accountCharacter.Id;
                characterRecord.BlizzardAccountId = account.Id;
                characterRecord.BlizzardRegionId = characterRef.Region;
                characterRecord.CharacterStatus = CharacterStatus2.None;
                characterRecord.RealmId = accountCharacter.Realm.Id;
                characterRecord.Name = accountCharacter.Name;
                characterRecord.NameSearchable = DatabaseHelpers.GetSearchableName(accountCharacter.Name);
                characterRecord.Race = (byte)accountCharacter.PlayableRace.Id;
                characterRecord.Class = (byte)accountCharacter.PlayableClass.Id;
                characterRecord.Gender = accountCharacter.Gender.AsGender();
                characterRecord.Faction = accountCharacter.Faction.AsFaction();
                characterRecord.Level = (byte)accountCharacter.Level;

                deletedCharactersSets.Remove(characterRecord.MoaRef);
            }
        }

        foreach (var character in deletedCharactersSets.Values)
        {
            character.CharacterStatus = CharacterStatus2.MaybeDeleted;
        }

        context.Operation().Items.Set(new Updates_UpdateAccountInvalidate(record.Id, record.FusionId, record.Username, characters.Values.Select(x => x.Id).ToHashSet()));
    }
}