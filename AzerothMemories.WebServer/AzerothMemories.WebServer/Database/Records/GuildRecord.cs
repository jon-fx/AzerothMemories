using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class GuildRecord : IBlizzardUpdateRecord, IDatabaseRecordWithVersion
{
    public const string TableName = "Guilds";

    [Key] public int Id { get; init; }

    [Column] public string MoaRef { get; init; } = null!;

    [Column] public long BlizzardId { get; set; }

    [Column] public BlizzardRegion BlizzardRegionId { get; init; }

    [Column] public BlizzardRealmVersion BlizzardRealmVersionId { get; set; }

    [Column] public string Name { get; set; } = null!;

    [Column] public string NameSearchable { get; set; } = null!;

    [Column] public int RealmId { get; set; }

    [Column] public CharacterFaction Faction { get; set; }

    [Column] public int MemberCount { get; set; }

    [Column] public int AchievementPoints { get; set; }

    [Column] public Instant CreatedDateTime { get; init; }

    [Column] public Instant BlizzardCreatedTimestamp { get; set; }

    [Column] public int AchievementTotalQuantity { get; set; }

    [Column] public int AchievementTotalPoints { get; set; }

    public BlizzardUpdateRecord? UpdateRecord { get; set; }

    public uint RowVersion { get; set; }

    public GuildViewModel CreateViewModel(GuildMembersViewModel memberViewModels)
    {
        return new GuildViewModel
        {
            Id = Id,
            Name = Name,
            MoaRef = MoaRef,
            RealmId = RealmId,
            RegionId = BlizzardRegionId,
            MemberCount = MemberCount,
            AchievementPoints = AchievementPoints,
            CreatedDateTime = CreatedDateTime.ToUnixTimeMilliseconds(),
            BlizzardCreatedTimestamp = BlizzardCreatedTimestamp.ToUnixTimeMilliseconds(),
            RealmVersion = BlizzardRealmVersionId,

            UpdateJobLastResults = UpdateRecord?.GetUpdateJobResults(),

            MembersViewModel = memberViewModels
        };
    }
}