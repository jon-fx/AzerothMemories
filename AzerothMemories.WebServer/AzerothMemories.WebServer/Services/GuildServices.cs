﻿namespace AzerothMemories.WebServer.Services;

public class GuildServices : IGuildServices
{
    private readonly ILogger<GuildServices> _logger;
    private readonly CommonServices _commonServices;

    public GuildServices(ILogger<GuildServices> logger, CommonServices commonServices)
    {
        _logger = logger;
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
        await using var database = _commonServices.DatabaseHub.CreateDbContext();
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
    public virtual Task<GuildMembersViewModel> TryGetGuildMembers(Session session, int guildId, int pageIndex)
    {
        return TryGetGuildMembers(guildId, pageIndex);
    }

    [ComputeMethod]
    public virtual async Task<GuildRecord> GetOrCreate(string refFull)
    {
        var moaRef = new MoaRef(refFull);
        Exceptions.ThrowIf(moaRef.IsValidCharacter);
        Exceptions.ThrowIf(moaRef.IsWildCard);
        Exceptions.ThrowIf(!moaRef.IsValidGuild);

        await using var database = _commonServices.DatabaseHub.CreateDbContext(true);
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

        var characters = await TryGetGuildMembers(guildId, 0).ConfigureAwait(false);
        return guildRecord.CreateViewModel(characters);
    }

    [ComputeMethod]
    protected virtual async Task<GuildMembersViewModel> TryGetGuildMembers(int guildId, int pageIndex)
    {
        var membersPerPage = 50;
        var allCharacters = await TryGetAllMembers(guildId).ConfigureAwait(false);
        var currentSet = allCharacters.Skip(membersPerPage * pageIndex).Take(membersPerPage);
        var characters = new HashSet<CharacterViewModel>();
        foreach (var character in currentSet)
        {
            await _commonServices.CharacterServices.DependsOnCharacterRecord(character.Id).ConfigureAwait(false);

            characters.Add(character.CreateViewModel());
        }

        return new GuildMembersViewModel
        {
            Index = pageIndex,
            TotalCount = allCharacters.Length,
            CharactersArray = characters.ToArray()
        };
    }

    [ComputeMethod]
    protected virtual async Task<CharacterRecord[]> TryGetAllMembers(int guildId)
    {
        var guildRecord = await TryGetGuildRecord(guildId).ConfigureAwait(false);
        if (guildRecord == null)
        {
            return Array.Empty<CharacterRecord>();
        }

        await using var database = _commonServices.DatabaseHub.CreateDbContext();

        var characterQuery = from characterRecord in database.Characters
                             where characterRecord.GuildId == guildId
                             orderby characterRecord.BlizzardGuildRank
                             select characterRecord;

        return await characterQuery.ToArrayAsync().ConfigureAwait(false);
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