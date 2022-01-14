namespace AzerothMemories.WebServer.Database.Records;

[Table("Posts_Comments_Reports")]
public sealed class PostCommentReportRecord : IDatabaseRecord
{
    [Column(IsPrimaryKey = true, IsIdentity = true)] public long Id { get; set; }

    [Column, NotNull] public long AccountId;

    [Column, NotNull] public long CommentId;

    [Column, NotNull] public PostReportedReason Reason;

    [Column, NotNull] public string ReasonText;

    [Column, NotNull] public Instant CreatedTime;

    [Column, NotNull] public Instant ModifiedTime;
}