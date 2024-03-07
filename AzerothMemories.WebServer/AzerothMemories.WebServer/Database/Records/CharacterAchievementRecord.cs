using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class CharacterAchievementRecord : IDatabaseRecordWithVersion
{
    public const string TableName = "Characters_Achievements";

    [Key] public int Id { get; init; }

    [Column] public int? AccountId { get; set; }

    [Column] public int CharacterId { get; init; }

    [Column] public int AchievementId { get; init; }

    [Column] public Instant AchievementTimeStamp { get; set; }

    public uint RowVersion { get; set; }
}