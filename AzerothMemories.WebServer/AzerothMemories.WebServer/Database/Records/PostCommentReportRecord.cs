using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class PostCommentReportRecord : IDatabaseRecord
{
    public const string TableName = "Posts_Comments_Reports";

    [Key] public long Id { get; set; }

    [Column] public long AccountId { get; set; }

    [Column] public long CommentId { get; set; }

    [Column] public PostReportedReason Reason { get; set; }

    [Column] public string ReasonText { get; set; }

    [Column] public Instant CreatedTime { get; set; }

    [Column] public Instant ModifiedTime { get; set; }
}