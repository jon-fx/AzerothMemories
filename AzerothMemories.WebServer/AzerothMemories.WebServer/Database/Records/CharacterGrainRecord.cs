using LinqToDB.Mapping;
using System.Net;

namespace AzerothMemories.WebServer.Database.Records
{
    [Table("Characters")]
    public sealed class CharacterGrainRecord : IBlizzardGrainUpdateRecord
    {
        [Column(IsPrimaryKey = true, IsIdentity = true)] public long Id { get; set; }

        [Column, NotNull] public string MoaRef { get; set; }

        [Column, NotNull] public long BlizzardId;

        [Column, NotNull] public string Name;

        [Column, NotNull] public string SearchableName;

        [Column, NotNull] public BlizzardRegion RegionId;

        [Column, NotNull] public long AccountId;

        [Column, NotNull] public bool AccountSync;

        [Column, NotNull] public int RealmId;

        [Column, NotNull] public byte Class;

        [Column, NotNull] public byte Race;

        [Column, NotNull] public byte Gender;

        [Column, NotNull] public byte Level;

        [Column, NotNull] public CharacterFaction Faction;

        [Column, NotNull] public string AvatarLink;

        [Column, NotNull] public int AchievementTotalQuantity;

        [Column, NotNull] public int AchievementTotalPoints;

        [Column, NotNull] public long GuildId;

        [Column, NotNull] public byte GuildRank;

        [Column, Nullable] public string GuildName;

        [Column, Nullable] public string GuildRef;

        [Column, NotNull] public DateTimeOffset LastUpdateEndTime { get; set; }

        [Column, NotNull] public RequestResultCode LastUpdateResult { get; set; }

        [Column, NotNull] public HttpStatusCode LastUpdateHttpResult { get; set; }

        [Column, NotNull] public long BlizzardProfileLastModified;

        [Column, NotNull] public long BlizzardAchievementsLastModified;

        [Column, NotNull] public long BlizzardRendersLastModified;
    }
}