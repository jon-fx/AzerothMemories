using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class AccountFollowingRecord : IDatabaseRecordWithVersion
{
    public const string TableName = "Accounts_Following";

    [Key] public int Id { get; set; }

    [Column] public int AccountId { get; set; }

    [Column] public int FollowerId { get; set; }

    [Column] public AccountFollowingStatus Status { get; set; }

    [Column] public Instant LastUpdateTime { get; set; }

    [Column] public Instant CreatedTime { get; set; }

    public uint RowVersion { get; set; }
}