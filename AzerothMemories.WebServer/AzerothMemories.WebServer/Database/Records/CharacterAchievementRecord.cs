using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class CharacterAchievementRecord : IDatabaseRecord
{
    public const string TableName = "Characters_Achievements";

    [Key] public int Id { get; set; }

    [Column] public int? AccountId { get; set; }

    [Column] public int CharacterId { get; set; }

    [Column] public int AchievementId { get; set; }

    [Column] public Instant AchievementTimeStamp { get; set; }

    [Column] public bool CompletedByCharacter { get; set; }
}