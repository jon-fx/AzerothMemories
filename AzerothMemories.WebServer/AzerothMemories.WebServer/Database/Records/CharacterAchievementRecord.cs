using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table("Characters_Achievements")]
public sealed class CharacterAchievementRecord : IDatabaseRecord
{
    [Key] public long Id { get; set; }

    [Column] public long? AccountId { get; set; }

    [Column] public long CharacterId { get; set; }

    [Column] public int AchievementId { get; set; }

    [Column] public long AchievementTimeStamp { get; set; }

    [Column] public bool CompletedByCharacter { get; set; }
}