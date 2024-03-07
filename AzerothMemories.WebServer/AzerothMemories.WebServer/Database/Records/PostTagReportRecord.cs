using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class PostTagReportRecord : IDatabaseRecordWithVersion
{
    public const string TableName = "Posts_Reports_Tags";

    [Key] public int Id { get; init; }

    [Column] public int AccountId { get; init; }

    [Column] public int PostId { get; init; }

    [Column] public int TagId { get; init; }

    [Column] public Instant CreatedTime { get; init; }

    [Column] public PostTagRecord Tag { get; init; }

    [Column] public int? ResolvedByAccountId { get; set; }

    public uint RowVersion { get; set; }
}