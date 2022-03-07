using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table(TableName)]
public sealed class BlizzardUpdateChildRecord : IDatabaseRecord
{
    public const string TableName = "Blizzard_Updates_Children";

    [Key] public int Id { get; set; }

    [Column] public int ParentId { get; set; }

    [Column] public BlizzardUpdateRecord Parent { get; set; }

    [Column] public BlizzardUpdateType UpdateType { get; set; }

    [Column] public byte UpdateFailCounter { get; set; }

    [Column] public Instant BlizzardLastModified { get; set; }

    [Column] public HttpStatusCode UpdateJobLastResult { get; set; }
}