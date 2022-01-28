using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table("Accounts_History")]
public sealed class AccountHistoryRecord
{
    [Key] public long Id { get; set; }

    [Column] public long AccountId { get; set; }

    [Column] public AccountHistoryType Type { get; set; }

    [Column] public long? OtherAccountId { get; set; }

    [Column] public long TargetId { get; set; }

    [Column] public long? TargetPostId { get; set; }

    [Column] public long? TargetCommentId { get; set; }

    [Column] public Instant CreatedTime { get; set; }
}