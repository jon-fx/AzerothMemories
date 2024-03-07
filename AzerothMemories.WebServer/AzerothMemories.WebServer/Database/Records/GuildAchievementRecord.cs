using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class GuildAchievementRecord : IDatabaseRecordWithVersion
{
    public const string TableName = "Guilds_Achievements";

    [Key] public int Id { get; init; }

    [Column] public int GuildId { get; init; }

    [Column] public int AchievementId { get; init; }

    [Column] public Instant AchievementTimeStamp { get; set; }

    public uint RowVersion { get; set; }
}