using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class AccountFollowingRecord : IDatabaseRecordWithVersion
{
    public const string TableName = "Accounts_Following";

    [Key] public int Id { get; init; }

    [Column] public int AccountId { get; init; }

    [Column] public int FollowerId { get; init; }

    [Column] public AccountFollowingStatus Status { get; set; }

    [Column] public Instant LastUpdateTime { get; set; }

    [Column] public Instant CreatedTime { get; init; }

    public uint RowVersion { get; set; }
}