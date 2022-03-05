namespace AzerothMemories.WebServer.Services;

[RegisterComputeService]
[RegisterAlias(typeof(IGuildServices))]
public class GuildServices : DbServiceBase<AppDbContext>, IGuildServices
{
    private readonly CommonServices _commonServices;

    public GuildServices(IServiceProvider services, CommonServices commonServices) : base(services)
    {
        _commonServices = commonServices;
    }

    [ComputeMethod]
    public virtual Task<int> DependsOnGuildRecord(int guildId)
    {
        return Task.FromResult(guildId);
    }

    [ComputeMethod]
    public virtual async Task<GuildRecord> TryGetGuildRecord(int id)
    {
        await using var database = CreateDbContext();
        var record = await database.Guilds.FirstOrDefaultAsync(r => r.Id == id).ConfigureAwait(false);

        if (record != null)
        {
            var moaRef = new MoaRef(record.MoaRef);

            Exceptions.ThrowIf(!moaRef.IsValidGuild);
            Exceptions.ThrowIf(moaRef.Id != 0);

            await DependsOnGuildRecord(record.Id).ConfigureAwait(false);
            await _commonServices.BlizzardUpdateHandler.TryUpdate(record, BlizzardUpdatePriority.Guild).ConfigureAwait(false);
        }

        return record;
    }

    [ComputeMethod]
    public virtual async Task<GuildRecord> GetOrCreate(string refFull)
    {
        var moaRef = new MoaRef(refFull);
        Exceptions.ThrowIf(moaRef.IsValidCharacter);
        Exceptions.ThrowIf(moaRef.IsWildCard);
        Exceptions.ThrowIf(!moaRef.IsValidGuild);

        await using var database = CreateDbContext(true);
        var guildRecord = await (from r in database.Guilds
                                 where r.MoaRef == moaRef.Full
                                 select r).FirstOrDefaultAsync().ConfigureAwait(false);

        if (guildRecord == null)
        {
            guildRecord = new GuildRecord
            {
                MoaRef = moaRef.Full,
                BlizzardId = moaRef.Id,
                BlizzardRegionId = moaRef.Region,
                CreatedDateTime = SystemClock.Instance.GetCurrentInstant()
            };

            await database.Guilds.AddAsync(guildRecord).ConfigureAwait(false);
            await database.SaveChangesAsync().ConfigureAwait(false);
        }

        Exceptions.ThrowIf(guildRecord.Id == 0);

        await DependsOnGuildRecord(guildRecord.Id).ConfigureAwait(false);
        await _commonServices.BlizzardUpdateHandler.TryUpdate(guildRecord, BlizzardUpdatePriority.Guild).ConfigureAwait(false);

        return guildRecord;
    }

    [ComputeMethod]
    public virtual async Task<GuildViewModel> TryGetGuild(Session session, int guildId)
    {
        var guildRecord = await TryGetGuildRecord(guildId).ConfigureAwait(false);
        if (guildRecord == null)
        {
            return null;
        }

        await using var database = CreateDbContext();

        var characterQuery = from characterRecord in database.Characters
                             where characterRecord.GuildId == guildId
                             orderby characterRecord.BlizzardGuildRank
                             select characterRecord.Id;

        var characters = new HashSet<CharacterViewModel>();
        var characterIds = await characterQuery.ToArrayAsync().ConfigureAwait(false);
        foreach (var id in characterIds)
        {
            var character = await _commonServices.CharacterServices.TryGetCharacterRecord(id).ConfigureAwait(false);
            if (character != null)
            {
                characters.Add(character.CreateViewModel());
            }
        }

        return guildRecord.CreateViewModel(characters);
    }

    [ComputeMethod]
    public virtual async Task<GuildViewModel> TryGetGuild(Session session, BlizzardRegion region, string realmSlug, string guildName)
    {
        if (region is <= 0 or >= BlizzardRegion.Count || string.IsNullOrWhiteSpace(realmSlug) || string.IsNullOrWhiteSpace(guildName))
        {
            return null;
        }

        var validRealmSlug = await _commonServices.TagServices.IsValidRealmSlug(realmSlug).ConfigureAwait(false);
        if (!validRealmSlug)
        {
            return null;
        }

        if (guildName.Length > 50)
        {
            return null;
        }

        var guildRef = MoaRef.GetGuildRef(region, realmSlug, guildName);
        var guildRecord = await GetOrCreate(guildRef.Full).ConfigureAwait(false);

        return await TryGetGuild(session, guildRecord.Id).ConfigureAwait(false);
    }

    public async Task<bool> TryEnqueueUpdate(Session session, BlizzardRegion region, string realmSlug, string guildName)
    {
        var guildRef = MoaRef.GetGuildRef(region, realmSlug, guildName);
        var guildRecord = await GetOrCreate(guildRef.Full).ConfigureAwait(false);

        return true;
    }
}