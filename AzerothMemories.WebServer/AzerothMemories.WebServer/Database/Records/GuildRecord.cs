namespace AzerothMemories.WebServer.Database.Records;

[Table("Guilds")]
public class GuildRecord : IBlizzardGrainUpdateRecord
{
    [Column(IsPrimaryKey = true, IsIdentity = true)] public long Id { get; set; }

    [Column, NotNull] public string MoaRef;

    [Column, NotNull] public long BlizzardId;

    [Column, NotNull] public BlizzardRegion BlizzardRegionId;

    [Column, NotNull] public string Name;

    [Column, NotNull] public string NameSearchable;

    [Column, NotNull] public int RealmId;

    [Column, NotNull] public CharacterFaction Faction;

    [Column, NotNull] public int MemberCount;

    [Column, NotNull] public int AchievementPoints;

    [Column, NotNull] public Instant CreatedDateTime;

    [Column, NotNull] public long BlizzardCreatedTimestamp;

    [Column, NotNull] public long BlizzardProfileLastModified;

    [Column, NotNull] public long BlizzardAchievementsLastModified;

    [Column, NotNull] public long BlizzardRosterLastModified;

    [Column, Nullable] public string UpdateJob { get; set; }

    [Column, Nullable] public Instant? UpdateJobQueueTime { get; set; }

    [Column, Nullable] public Instant? UpdateJobStartTime { get; set; }

    [Column, Nullable] public Instant? UpdateJobEndTime { get; set; }

    [Column, Nullable] public HttpStatusCode UpdateJobLastResult { get; set; }
}