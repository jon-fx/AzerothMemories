﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class GuildRecord : IBlizzardUpdateRecord
{
    public const string TableName = "Guilds";

    [Key] public long Id { get; set; }

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

    [Column] public long BlizzardCreatedTimestamp { get; set; }

    [Column] public long BlizzardProfileLastModified { get; set; }

    [Column] public long BlizzardAchievementsLastModified { get; set; }

    [Column] public long BlizzardRosterLastModified { get; set; }

    [Column] public string UpdateJob { get; set; }

    [Column] public Instant UpdateJobEndTime { get; set; }

    [Column] public HttpStatusCode UpdateJobLastResult { get; set; }

    public GuildViewModel CreateViewModel(HashSet<CharacterViewModel> characterViewModels)
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
            BlizzardCreatedTimestamp = BlizzardCreatedTimestamp,
            LastUpdateJobEndTime = UpdateJobEndTime.ToUnixTimeMilliseconds(),

            CharactersArray = characterViewModels.ToArray()
        };
    }
}