namespace AzerothMemories.WebServer.Database.Records;

[Table("Accounts_History")]
public sealed class AccountHistoryRecord
{
    [Column(IsPrimaryKey = true, IsIdentity = true)] public long Id { get; set; }

    [Column, NotNull] public long AccountId;

    [Column, NotNull] public AccountHistoryType Type;

    [Column, NotNull] public long? OtherAccountId;

    [Column, NotNull] public long TargetId;

    [Column, NotNull] public long? TargetPostId;

    [Column, NotNull] public long? TargetCommentId;

    [Column, NotNull] public Instant CreatedTime;
}