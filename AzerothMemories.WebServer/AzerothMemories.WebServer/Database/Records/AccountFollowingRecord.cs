using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table("Accounts_Following")]
public sealed class AccountFollowingRecord : IDatabaseRecord
{
    [Key] public long Id { get; set; }

    [Column] public long AccountId { get; set; }

    [Column] public long FollowerId { get; set; }

    [Column] public AccountFollowingStatus Status { get; set; }

    [Column] public Instant LastUpdateTime { get; set; }

    [Column] public Instant CreatedTime { get; set; }
}