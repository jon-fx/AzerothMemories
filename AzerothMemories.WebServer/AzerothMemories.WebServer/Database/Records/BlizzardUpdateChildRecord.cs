using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class BlizzardUpdateChildRecord : IDatabaseRecord
{
    public const string TableName = "Blizzard_Updates_Children";

    [Key] public int Id { get; init; }

    [Column] public int ParentId { get; init; }

    [Column] public BlizzardUpdateRecord Parent { get; init; } = null!;

    [Column] public BlizzardUpdateType UpdateType { get; init; }

    [Column] public string UpdateTypeString { get; init; } = null!;

    [Column] public byte UpdateFailCounter { get; set; }

    [Column] public Instant BlizzardLastModified { get; set; }

    [Column] public HttpStatusCode UpdateJobLastResult { get; set; }
}