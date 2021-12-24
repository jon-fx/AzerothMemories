using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

public class AccountRecord
{
    [Key] public long Id { get; set; }

    [Column] public string FusionId { get; set; }

    [Column] public DateTimeOffset CreatedDateTime { get; set; }
}