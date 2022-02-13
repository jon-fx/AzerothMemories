using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class CharacterRecord : IBlizzardUpdateRecord
{
    public const string TableName = "Characters";

    [Key] public long Id { get; set; }

    [Column] public string MoaRef { get; set; }

    [Column] public long BlizzardId { get; set; }

    [Column] public BlizzardRegion BlizzardRegionId { get; set; }

    [Column] public string Name { get; set; }

    [Column] public string NameSearchable { get; set; }

    [Column] public Instant CreatedDateTime { get; set; }

    [Column] public CharacterStatus2 CharacterStatus { get; set; }

    [Column] public long? AccountId { get; set; }

    //[Column] public AccountRecord Account { get; set; }

    [Column] public bool AccountSync { get; set; }

    [Column] public int RealmId { get; set; }

    [Column] public byte Class { get; set; }

    [Column] public byte Race { get; set; }

    [Column] public byte Gender { get; set; }

    [Column] public byte Level { get; set; }

    [Column] public CharacterFaction Faction { get; set; }

    [Column] public string AvatarLink { get; set; }

    [Column] public int AchievementTotalQuantity { get; set; }

    [Column] public int AchievementTotalPoints { get; set; }

    [Column] public long? GuildId { get; set; }

    [Column] public string GuildRef { get; set; }

    [Column] public long BlizzardGuildId { get; set; }

    [Column] public byte BlizzardGuildRank { get; set; }

    [Column] public string BlizzardGuildName { get; set; }

    [Column] public string UpdateJob { get; set; }

    [Column] public Instant UpdateJobEndTime { get; set; }

    [Column] public HttpStatusCode UpdateJobLastResult { get; set; }

    [Column] public long BlizzardProfileLastModified { get; set; }

    [Column] public long BlizzardRendersLastModified { get; set; }

    [Column] public long BlizzardAchievementsLastModified { get; set; }

    public CharacterViewModel CreateViewModel()
    {
        return new CharacterViewModel
        {
            Id = Id,
            Ref = MoaRef,
            Race = Race,
            Class = Class,
            Level = Level,
            Gender = Gender,
            //Faction = Faction,
            CharacterStatus = CharacterStatus,
            AvatarLink = AvatarLink,
            AccountSync = AccountSync,
            Name = Name,
            RealmId = RealmId,
            RegionId = BlizzardRegionId,
            GuildRef = GuildRef,
            GuildName = BlizzardGuildName,
            //GuildRank = BlizzardGuildRank,
            //AchievementTotalPoints = AchievementTotalPoints,
            //AchievementTotalQuantity = AchievementTotalQuantity,

            UpdateJobEndTime = UpdateJobEndTime.ToUnixTimeMilliseconds(),
            UpdateJobLastResult = UpdateJobLastResult
        };
    }
}