using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class AccountUploadLog : IDatabaseRecordWithVersion
{
    public const string TableName = "Accounts_UploadLog";

    [Key] public int Id { get; init; }

    [Column] public int AccountId { get; init; }

    [Column] public string BlobName { get; init; }

    [Column] public string BlobHash { get; init; }

    [Column] public AccountUploadLogStatus UploadStatus { get; set; }

    [Column] public Instant UploadTime { get; init; }

    [Column] public int? PostId { get; init; }

    [Column] public PostRecord Post { get; init; }

    public uint RowVersion { get; set; }
}