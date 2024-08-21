using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class CharacterAchievementStatisticRecord : IDatabaseRecordWithVersion
{
    public const string TableName = "Characters_Achievement_Statistics";

    [Key] public int Id { get; init; }

    [Column] public int? AccountId { get; set; }

    [Column] public int CharacterId { get; init; }

    [Column] public int StatisticId { get; init; }

    [Column] public float StatisticQuantity { get; set; }

    [Column] public Instant StatisticTimeStamp { get; set; }

    [Column, Required] public BlizzardDataRecordLocal StatisticDescription { get; init; } = new();

    public uint RowVersion { get; set; }
}