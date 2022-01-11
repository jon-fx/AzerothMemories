namespace AzerothMemories.WebServer.Services;

[RegisterComputeService]
[RegisterAlias(typeof(IGuildServices))]
public class GuildServices : IGuildServices
{
    private readonly CommonServices _commonServices;

    public GuildServices(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    [ComputeMethod]
    public virtual async Task<GuildRecord> TryGetGuildRecord(long id)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var record = await database.Guilds.Where(a => a.Id == id).FirstOrDefaultAsync();

        if (record != null)
        {
            var moaRef = new MoaRef(record.MoaRef);

            Exceptions.ThrowIf(!moaRef.IsValidGuild);
            Exceptions.ThrowIf(moaRef.Id != record.BlizzardId);
        }

        return record;
    }

    public virtual async Task<GuildRecord> GetOrCreate(string refFull)
    {
        var moaRef = new MoaRef(refFull);
        Exceptions.ThrowIf(moaRef.IsValidAccount);
        Exceptions.ThrowIf(moaRef.IsValidCharacter);
        Exceptions.ThrowIf(moaRef.IsWildCard);

        Exceptions.ThrowIf(!moaRef.IsValidGuild);

        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var guildId = await (from r in database.Guilds
                             where r.MoaRef == moaRef.Full
                             select r.Id).FirstOrDefaultAsync();

        if (guildId == 0)
        {
            guildId = await (from r in database.Guilds
                             where Sql.Like(r.MoaRef, moaRef.GetLikeQuery())
                             select r.Id).FirstOrDefaultAsync();

            if (guildId > 0)
            {
            }
            else
            {
                guildId = await database.InsertWithInt64IdentityAsync(new GuildRecord
                {
                    MoaRef = moaRef.Full,
                    BlizzardId = moaRef.Id,
                    BlizzardRegionId = moaRef.Region,
                    CreatedDateTime = SystemClock.Instance.GetCurrentInstant()
                });
            }
        }

        var guildRecord = await TryGetGuildRecord(guildId);

        await _commonServices.BlizzardUpdateHandler.TryUpdate(database, guildRecord, BlizzardUpdatePriority.Guild);

        return guildRecord;
    }

    public void OnGuildUpdate(GuildRecord guildRecord)
    {
        OnGuildUpdate(guildRecord.Id);
    }

    private void OnGuildUpdate(long guildId)
    {
        using var computed = Computed.Invalidate();
        _ = TryGetGuildRecord(guildId);
        _ = _commonServices.TagServices.TryGetUserTagInfo(PostTagType.Guild, guildId);
    }
}