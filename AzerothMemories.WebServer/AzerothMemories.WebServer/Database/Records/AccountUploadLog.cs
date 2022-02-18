using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class AccountUploadLog : IDatabaseRecord
{
    public const string TableName = "Accounts_UploadLog";

    [Key] public long Id { get; set; }

    [Column] public long AccountId { get; set; }

    [Column] public string BlobName { get; set; }

    [Column] public string BlobHash { get; set; }

    [Column] public Instant UploadTime { get; set; }
}