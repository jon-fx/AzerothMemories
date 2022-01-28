using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table("Posts_Reports")]
public sealed class PostReportRecord : IDatabaseRecord
{
    [Key] public long Id { get; set; }

    [Column] public long AccountId { get; set; }

    [Column] public long PostId { get; set; }

    [Column] public PostReportedReason Reason { get; set; }

    [Column] public string ReasonText { get; set; }

    [Column] public Instant CreatedTime { get; set; }

    [Column] public Instant ModifiedTime { get; set; }
}