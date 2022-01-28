using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table("Posts_Reports_Tags")]
public sealed class PostTagReportRecord : IDatabaseRecord
{
    [Key] public long Id { get; set; }

    [Column] public long AccountId { get; set; }

    [Column] public long PostId { get; set; }

    [Column] public long TagId { get; set; }

    [Column] public Instant CreatedTime { get; set; }
}