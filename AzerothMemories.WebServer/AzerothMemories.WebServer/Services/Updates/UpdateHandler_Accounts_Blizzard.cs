using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class UpdateHandler_Accounts_Blizzard : UpdateHandlerBaseResult<AccountRecord, AccountProfileSummary>
{
    private readonly BlizzardRegion _blizzardRegion;
    private readonly BlizzardUpdateServices _blizzardUpdateServices;

    public UpdateHandler_Accounts_Blizzard(BlizzardUpdateType updateType, CommonServices commonServices, BlizzardUpdateServices blizzardUpdateServices, ILogger<BlizzardUpdateServices> logger, [CallerArgumentExpression("updateType")] string? updateTypeString = null) : base(updateType, commonServices, logger, updateTypeString)
    {
        _blizzardRegion = (BlizzardRegion)updateType;
        _blizzardUpdateServices = blizzardUpdateServices;
    }

    protected override bool ShouldExecuteOn(CommandContext context, AppDbContext database, AccountRecord record, [NotNullWhen(true)] out AuthTokenRecord? authTokenRecord)
    {
        authTokenRecord = record.AuthTokens.FirstOrDefault(x => x.IsBlizzardAuthToken);
        return authTokenRecord != null;
    }

    protected override async Task<RequestResult<AccountProfileSummary>> TryExecuteRequest(AccountRecord record, AuthTokenRecord? authTokenRecord, Instant blizzardLastModified)
    {
        if (authTokenRecord == null || string.IsNullOrWhiteSpace(authTokenRecord.Token) || SystemClock.Instance.GetCurrentInstant() >= authTokenRecord.TokenExpiresAt)
        {
            return new RequestResult<AccountProfileSummary>(HttpStatusCode.Forbidden, null, blizzardLastModified, null);
        }

        using var client = CommonServices.HttpClientProvider.GetWarcraftClient(_blizzardRegion);
        return await client.GetAccountProfile(authTokenRecord.Token, blizzardLastModified).ConfigureAwait(false);
    }

    protected override async Task InternalExecuteWithResult(CommandContext context, AppDbContext database, AccountRecord record, AccountProfileSummary requestResult)
    {
        var characters = await database.Characters.Where(x => x.AccountId == record.Id).ToDictionaryAsync(x => x.MoaRef, x => x).ConfigureAwait(false);
        var deletedCharactersSets = new Dictionary<string, CharacterRecord>(characters);

        foreach (var account in requestResult.WowAccounts.SafeEnumerable())
        {
            foreach (var accountCharacter in account.Characters.SafeEnumerable())
            {
                var characterRef = MoaRef.GetCharacterRef(_blizzardRegion, accountCharacter.Realm?.Slug, accountCharacter.Name, accountCharacter.Id);
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

                characterRecord.AccountId = record.Id;
                characterRecord.MoaRef = characterRef.Full;
                characterRecord.BlizzardId = accountCharacter.Id;
                characterRecord.BlizzardAccountId = account.Id;
                characterRecord.BlizzardRegionId = characterRef.Region;
                characterRecord.CharacterStatus = CharacterStatus2.None;
                characterRecord.RealmId = accountCharacter.Realm?.Id ?? 0;
                characterRecord.Name = accountCharacter.Name ?? $"Character-{characterRecord.Id}";
                characterRecord.NameSearchable = DatabaseHelpers.GetSearchableName(characterRecord.Name);
                characterRecord.Race = (byte)(accountCharacter.PlayableRace?.Id ?? 0);
                characterRecord.Class = (byte)(accountCharacter.PlayableClass?.Id ?? 0);
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

        context.Operation.Items.Set(new Updates_UpdateAccountInvalidate(record.Id, record.FusionId, record.Username, characters.Values.Select(x => x.Id).ToHashSet()));
    }
}