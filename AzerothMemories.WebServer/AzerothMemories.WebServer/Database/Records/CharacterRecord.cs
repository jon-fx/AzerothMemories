namespace AzerothMemories.WebServer.Database.Records;

[Table("Characters")]
public class CharacterRecord : IBlizzardGrainUpdateRecord
{
    [Column(IsPrimaryKey = true, IsIdentity = true)] public long Id { get; set; }

    [Column, NotNull] public string MoaRef;

    [Column, NotNull] public long BlizzardId;

    [Column, NotNull] public BlizzardRegion BlizzardRegionId;

    [Column, Nullable] public string Name;

    [Column, Nullable] public string NameSearchable;

    [Column, NotNull] public Instant CreatedDateTime;

    [Column, NotNull] public long? AccountId;

    [Column, NotNull] public bool AccountSync;

    [Column, NotNull] public int RealmId;

    [Column, NotNull] public byte Class;

    [Column, NotNull] public byte Race;

    [Column, NotNull] public byte Gender;

    [Column, NotNull] public byte Level;

    [Column, NotNull] public CharacterFaction Faction;

    [Column, Nullable] public string AvatarLink;

    [Column, NotNull] public int AchievementTotalQuantity;

    [Column, NotNull] public int AchievementTotalPoints;

    [Column, NotNull] public long GuildId;

    [Column, NotNull] public byte GuildRank;

    [Column, Nullable] public string GuildName;

    [Column, Nullable] public string GuildRef;

    [Column, Nullable] public string UpdateJob { get; set; }

    [Column, Nullable] public Instant? UpdateJobQueueTime { get; set; }

    [Column, Nullable] public Instant? UpdateJobStartTime { get; set; }

    [Column, Nullable] public Instant? UpdateJobEndTime { get; set; }

    [Column, Nullable] public HttpStatusCode UpdateJobLastResult { get; set; }

    [Column, NotNull] public long BlizzardProfileLastModified;

    [Column, NotNull] public long BlizzardRendersLastModified;

    [Column, NotNull] public long BlizzardAchievementsLastModified;
}