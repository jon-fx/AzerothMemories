namespace AzerothMemories.WebServer.Database.Records;

[Table("Accounts_Following")]
public sealed class AccountFollowingRecord
{
    [Column(IsPrimaryKey = true, IsIdentity = true)] public long Id { get; set; }

    [Column, NotNull] public long AccountId;

    [Column, NotNull] public long FollowerId;

    [Column, NotNull] public AccountFollowingStatus Status;

    [Column, NotNull] public Instant LastUpdateTime;

    [Column, NotNull] public Instant CreatedTime;
}