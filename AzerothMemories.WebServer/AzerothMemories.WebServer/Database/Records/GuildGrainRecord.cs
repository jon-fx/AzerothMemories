using LinqToDB.Mapping;
using System.Net;

namespace AzerothMemories.WebServer.Database.Records
{
    [Table("Guilds")]
    public sealed class GuildGrainRecord : IBlizzardGrainUpdateRecord
    {
        [Column(IsPrimaryKey = true, IsIdentity = true)] public long Id { get; set; }

        [Column, NotNull] public string MoaRef { get; set; }

        [Column, NotNull] public long BlizzardId;

        [Column, NotNull] public string Name;

        [Column, NotNull] public string SearchableName;

        [Column, NotNull] public BlizzardRegion RegionId;

        [Column, NotNull] public int RealmId;

        [Column, NotNull] public CharacterFaction Faction;

        [Column, NotNull] public long BlizzardCreatedTimestamp;

        [Column, NotNull] public int MemberCount;

        [Column, NotNull] public int AchievementPoints;

        [Column, NotNull] public DateTimeOffset LastUpdateEndTime { get; set; }

        [Column, NotNull] public RequestResultCode LastUpdateResult { get; set; }

        [Column, NotNull] public HttpStatusCode LastUpdateHttpResult { get; set; }

        [Column, NotNull] public long BlizzardProfileLastModified;

        [Column, NotNull] public long BlizzardAchievementsLastModified;

        [Column, NotNull] public long BlizzardRosterLastModified;
    }
}