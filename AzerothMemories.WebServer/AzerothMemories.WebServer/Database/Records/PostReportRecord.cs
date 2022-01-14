namespace AzerothMemories.WebServer.Database.Records;

[Table("Posts_Reports")]
public sealed class PostReportRecord : IDatabaseRecord
{
    [Column(IsPrimaryKey = true, IsIdentity = true)] public long Id { get; set; }

    [Column, NotNull] public long AccountId;

    [Column, NotNull] public long PostId;

    [Column, NotNull] public PostReportedReason Reason;

    [Column, NotNull] public string ReasonText;

    [Column, NotNull] public Instant CreatedTime;

    [Column, NotNull] public Instant ModifiedTime;
}