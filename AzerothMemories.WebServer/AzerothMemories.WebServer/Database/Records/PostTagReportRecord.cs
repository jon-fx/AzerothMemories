using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class PostTagReportRecord : IDatabaseRecord
{
    public const string TableName = "Posts_Reports_Tags";

    [Key] public int Id { get; set; }

    [Column] public int AccountId { get; set; }

    [Column] public int PostId { get; set; }

    [Column] public int TagId { get; set; }

    [Column] public Instant CreatedTime { get; set; }
}