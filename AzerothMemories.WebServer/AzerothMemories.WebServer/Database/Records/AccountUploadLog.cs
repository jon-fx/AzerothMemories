using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class AccountUploadLog : IDatabaseRecord
{
    public const string TableName = "Accounts_UploadLog";

    [Key] public int Id { get; set; }

    [Column] public int AccountId { get; set; }

    [Column] public string BlobName { get; set; }

    [Column] public string BlobHash { get; set; }

    [Column] public AccountUploadLogStatus UploadStatus { get; set; }

    [Column] public Instant UploadTime { get; set; }
    
    [Column] public int? PostId { get; set; }

    [Column] public PostRecord Post { get; set; }
}