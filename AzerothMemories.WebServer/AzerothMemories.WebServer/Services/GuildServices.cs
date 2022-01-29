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
    public virtual Task<long> DependsOnGuildRecord(long guildId)
    {
        return Task.FromResult(guildId);
    }

    [ComputeMethod]
    public virtual async Task<GuildRecord> TryGetGuildRecord(long id)
    {
        await using var database = CreateDbContext();
        var record = await database.Guilds.FirstOrDefaultAsync(r => r.Id == id);

        if (record != null)
        {
            var moaRef = new MoaRef(record.MoaRef);

            Exceptions.ThrowIf(!moaRef.IsValidGuild);
            Exceptions.ThrowIf(moaRef.Id != record.BlizzardId);

            await DependsOnGuildRecord(record.Id);
            await _commonServices.BlizzardUpdateHandler.TryUpdate(record, BlizzardUpdatePriority.Guild);
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
                                 select r).FirstOrDefaultAsync();

        if (guildRecord == null)
        {
            guildRecord = await (from r in database.Guilds
                                 where EF.Functions.Like(r.MoaRef, moaRef.GetLikeQuery())
                                 select r).FirstOrDefaultAsync();

            if (guildRecord == null)
            {
                guildRecord = new GuildRecord
                {
                    MoaRef = moaRef.Full,
                    BlizzardId = moaRef.Id,
                    BlizzardRegionId = moaRef.Region,
                    CreatedDateTime = SystemClock.Instance.GetCurrentInstant()
                };

                await database.Guilds.AddAsync(guildRecord);
                await database.SaveChangesAsync();
            }
            else
            {
            }
        }

        Exceptions.ThrowIf(guildRecord.Id == 0);

        await DependsOnGuildRecord(guildRecord.Id);
        await _commonServices.BlizzardUpdateHandler.TryUpdate(guildRecord, BlizzardUpdatePriority.Guild);

        return guildRecord;
    }

    [ComputeMethod]
    public virtual async Task<GuildViewModel> TryGetGuild(Session session, long guildId)
    {
        var guildRecord = await TryGetGuildRecord(guildId);
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
        var characterIds = await characterQuery.ToArrayAsync();
        foreach (var id in characterIds)
        {
            var character = await _commonServices.CharacterServices.TryGetCharacterRecord(id);
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

        var realmId = await _commonServices.TagServices.TryGetRealmId(realmSlug);
        if (realmId == 0)
        {
            return null;
        }

        if (guildName.Length > 50)
        {
            return null;
        }

        var guildRef = await GetFullGuildRef(region, realmSlug, guildName);
        if (guildRef == null)
        {
            return null;
        }

        var guildRecord = await GetOrCreate(guildRef.Full);

        return await TryGetGuild(session, guildRecord.Id);
    }

    public async Task<bool> TryEnqueueUpdate(Session session, BlizzardRegion region, string realmSlug, string guildName)
    {
        var guildRef = await GetFullGuildRef(region, realmSlug, guildName);
        if (guildRef == null)
        {
            return false;
        }

        await GetOrCreate(guildRef.Full);

        return true;
    }

    [ComputeMethod]
    protected virtual async Task<MoaRef> GetFullGuildRef(BlizzardRegion region, string realmSlug, string guildName)
    {
        await using var database = CreateDbContext();

        var moaRef = MoaRef.GetGuildRef(region, realmSlug, guildName, -1);
        var query = from r in database.Guilds
                    where EF.Functions.Like(r.MoaRef, moaRef.GetLikeQuery())
                    select new { r.Id, r.MoaRef };

        var result = await query.FirstOrDefaultAsync();
        if (result != null)
        {
            return new MoaRef(result.MoaRef);
        }

        return null;
    }
}