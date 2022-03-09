using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class GuildRecord : IBlizzardUpdateRecord
{
    public const string TableName = "Guilds";

    [Key] public int Id { get; set; }

    [Column] public string MoaRef { get; set; }

    [Column] public long BlizzardId { get; set; }

    [Column] public BlizzardRegion BlizzardRegionId { get; set; }

    [Column] public string Name { get; set; }

    [Column] public string NameSearchable { get; set; }

    [Column] public int RealmId { get; set; }

    [Column] public CharacterFaction Faction { get; set; }

    [Column] public int MemberCount { get; set; }

    [Column] public int AchievementPoints { get; set; }

    [Column] public Instant CreatedDateTime { get; set; }

    [Column] public Instant BlizzardCreatedTimestamp { get; set; }

    [Column] public int AchievementTotalQuantity { get; set; }

    [Column] public int AchievementTotalPoints { get; set; }

    public BlizzardUpdateRecord UpdateRecord { get; set; }

    public GuildViewModel CreateViewModel(GuildMembersViewModel memberViewModels)
    {
        return new GuildViewModel
        {
            Id = Id,
            Name = Name,
            RealmId = RealmId,
            RegionId = BlizzardRegionId,
            MemberCount = MemberCount,
            AchievementPoints = AchievementPoints,
            CreatedDateTime = CreatedDateTime.ToUnixTimeMilliseconds(),
            BlizzardCreatedTimestamp = BlizzardCreatedTimestamp.ToUnixTimeMilliseconds(),

            UpdateJobLastResult = UpdateRecord?.UpdateJobLastResult ?? 0,
            UpdateJobLastEndTime = UpdateRecord?.UpdateJobLastEndTime.ToUnixTimeMilliseconds() ?? 0,

            MembersViewModel = memberViewModels
        };
    }
}