namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class UpdateHandler_Characters_Mounts : UpdateHandlerBaseResult<CharacterRecord, CharacterMountsCollectionSummary>, IRequiresExecuteOnFirstLogin
{
    public UpdateHandler_Characters_Mounts(CommonServices commonServices, ILogger<BlizzardUpdateServices> logger) : base(BlizzardUpdateType.Character_Mounts, commonServices, logger)
    {
    }

    public async Task OnFirstLogin(AppDbContext database, AccountRecord accountRecord, CharacterRecord characterRecord)
    {
        var records = await database.CharacterMounts.Where(x => x.CharacterId == characterRecord.Id && x.AccountId == null).ToArrayAsync().ConfigureAwait(false);
        foreach (var record in records)
        {
            record.AccountId = accountRecord.Id;
        }
    }

    protected override async Task<RequestResult<CharacterMountsCollectionSummary>> TryExecuteRequest(CharacterRecord record, AuthTokenRecord? authTokenRecord, Instant blizzardLastModified)
    {
        var characterRef = new MoaRef(record.MoaRef);
        if (!characterRef.IsValidCharacter)
        {
            throw new NotImplementedException();
        }

        using var client = CommonServices.HttpClientProvider.GetWarcraftClient(record.BlizzardRegionId);
        return await client.GetCharacterMountsSummaryAsync(characterRef.Realm, characterRef.Name, blizzardLastModified).ConfigureAwait(false);
    }

    protected override async Task InternalExecuteWithResult(AppDbContext database, CharacterRecord record, CharacterMountsCollectionSummary requestResult)
    {
        var currentMounts = await database.CharacterMounts.Where(x => x.CharacterId == record.Id).ToArrayAsync().ConfigureAwait(false);
        var currentMountsDict = new Dictionary<int, CharacterMountRecord>();
        foreach (var currentMount in currentMounts)
        {
            if (currentMountsDict.TryAdd(currentMount.MountId, currentMount))
            {
            }
            else
            {
                database.CharacterMounts.Remove(currentMount);
            }
        }

        var currentTimeStamp = SystemClock.Instance.GetCurrentInstant();
        if (currentMountsDict.Count == 0)
        {
            currentTimeStamp = Instant.FromUnixTimeMilliseconds(0);
        }

        foreach (var mount in requestResult.Mounts.SafeEnumerable())
        {
            if (mount.Mount == null)
            {
                continue;
            }

            if (currentMountsDict.TryGetValue(mount.Mount.Id, out var mountRecord))
            {
                mountRecord.AccountId = record.AccountId;
            }
            else
            {
                mountRecord = new CharacterMountRecord
                {
                    AccountId = record.AccountId,
                    CharacterId = record.Id,
                    MountId = mount.Mount.Id,
                    MountTimeStamp = currentTimeStamp
                };

                if (currentMountsDict.TryAdd(mountRecord.MountId, mountRecord))
                {
                    database.CharacterMounts.Add(mountRecord);
                }
            }
        }
    }
}