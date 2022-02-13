using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class AccountFollowingRecord : IDatabaseRecord
{
    public const string TableName = "Accounts_Following";

    [Key] public long Id { get; set; }

    [Column] public long AccountId { get; set; }

    [Column] public long FollowerId { get; set; }

    [Column] public AccountFollowingStatus Status { get; set; }

    [Column] public Instant LastUpdateTime { get; set; }

    [Column] public Instant CreatedTime { get; set; }
}