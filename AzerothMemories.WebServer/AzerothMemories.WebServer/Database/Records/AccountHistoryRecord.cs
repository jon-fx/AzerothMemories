using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class AccountHistoryRecord : IDatabaseRecordWithVersion
{
    public const string TableName = "Accounts_History";

    [Key] public int Id { get; set; }

    [Column] public int AccountId { get; set; }

    [Column] public AccountHistoryType Type { get; set; }

    [Column] public int? OtherAccountId { get; set; }

    [Column] public int TargetId { get; set; }

    [Column] public int? TargetPostId { get; set; }

    [Column] public int? TargetCommentId { get; set; }

    [Column] public Instant CreatedTime { get; set; }

    public uint RowVersion { get; set; }
}