﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class CharacterRecord : IBlizzardUpdateRecord, IDatabaseRecordWithVersion
{
    public const string TableName = "Characters";

    [Key] public int Id { get; set; }

    [Column] public string MoaRef { get; set; }

    [Column] public long BlizzardId { get; set; }

    [Column] public long BlizzardAccountId { get; set; }

    [Column] public BlizzardRegion BlizzardRegionId { get; set; }

    [Column] public string Name { get; set; }

    [Column] public string NameSearchable { get; set; }

    [Column] public Instant CreatedDateTime { get; set; }

    [Column] public CharacterStatus2 CharacterStatus { get; set; }

    [Column] public int? AccountId { get; set; }

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

    [Column] public int? GuildId { get; set; }

    [Column] public string GuildRef { get; set; }

    [Column] public byte BlizzardGuildRank { get; set; }

    [Column] public string BlizzardGuildName { get; set; }

    public uint RowVersion { get; set; }

    public BlizzardUpdateRecord UpdateRecord { get; set; }

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

            UpdateJobLastResults = UpdateRecord?.GetUpdateJobResults(),
        };
    }
}