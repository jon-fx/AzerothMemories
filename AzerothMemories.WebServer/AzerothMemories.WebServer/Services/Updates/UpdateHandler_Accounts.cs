using System.Runtime.CompilerServices;

namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class UpdateHandler_Accounts : UpdateHandlerBaseResult<AuthTokenRecord, AccountProfileSummary>
{
    private readonly BlizzardRegion _blizzardRegion;
    private readonly BlizzardUpdateServices _blizzardUpdateServices;

    public UpdateHandler_Accounts(BlizzardUpdateType updateType, CommonServices commonServices, BlizzardUpdateServices blizzardUpdateServices, [CallerArgumentExpression("updateType")] string updateTypeString = null) : base(updateType, commonServices, updateTypeString)
    {
        _blizzardRegion = (BlizzardRegion)updateType;
        _blizzardUpdateServices = blizzardUpdateServices;
    }

    protected override async Task<RequestResult<AccountProfileSummary>> TryExecuteRequest(AuthTokenRecord record, Instant blizzardLastModified)
    {
        if (string.IsNullOrWhiteSpace(record.Token) || SystemClock.Instance.GetCurrentInstant() >= record.TokenExpiresAt)
        {
            return new RequestResult<AccountProfileSummary>(HttpStatusCode.Forbidden, null, blizzardLastModified, null);
        }

        using var client = CommonServices.HttpClientProvider.GetWarcraftClient(_blizzardRegion);
        return await client.GetAccountProfile(record.Token, blizzardLastModified).ConfigureAwait(false);
    }

    protected override async Task InternalExecuteWithResult(CommandContext context, AppDbContext database, AuthTokenRecord record, AccountProfileSummary requestResult)
    {
        var characters = await database.Characters.Where(x => x.AccountId == record.AccountId).ToDictionaryAsync(x => x.MoaRef, x => x).ConfigureAwait(false);
        var deletedCharactersSets = new Dictionary<string, CharacterRecord>(characters);

        foreach (var account in requestResult.WowAccounts)
        {
            foreach (var accountCharacter in account.Characters)
            {
                var characterRef = MoaRef.GetCharacterRef(_blizzardRegion, accountCharacter.Realm.Slug, accountCharacter.Name, accountCharacter.Id);
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
                    await _blizzardUpdateServices.ExecuteHandlersOnFirstLogin(context, database, record, characterRecord).ConfigureAwait(false);
                }

                characterRecord.AccountId = record.AccountId;
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

        context.Operation().Items.Set(new Updates_UpdateAccountInvalidate(record.Account.Id, record.Account.FusionId, record.Account.Username, characters.Values.Select(x => x.Id).ToHashSet()));
    }
}