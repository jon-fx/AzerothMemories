using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class PostReportRecord : IDatabaseRecordWithVersion
{
    public const string TableName = "Posts_Reports";

    [Key] public int Id { get; init; }

    [Column] public int AccountId { get; init; }

    [Column] public int PostId { get; init; }

    [Column] public PostReportedReason Reason { get; set; }

    [Column] public string ReasonText { get; set; }

    [Column] public Instant CreatedTime { get; init; }

    [Column] public Instant ModifiedTime { get; set; }

    [Column] public int? ResolvedByAccountId { get; set; }

    public uint RowVersion { get; set; }
}