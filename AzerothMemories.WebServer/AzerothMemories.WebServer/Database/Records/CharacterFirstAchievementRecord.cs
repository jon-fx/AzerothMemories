using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class CharacterFirstAchievementRecord : IDatabaseRecord
{
    public const string TableName = "Characters_Achievements_First";

    [Key] public int Id { get; init; }

    [Column] public int AchievementId { get; init; }

    [Column] public Instant AchievementTimeStamp { get; set; }
}