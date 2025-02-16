using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class BlizzardRealmRecord : IDatabaseRecord
{
    public const string TableName = "Blizzard_Realms";

    [Key] public int Id { get; init; }

    [Column] public int RealmId { get; init; }

    [Column] public string RealmNameEnUs { get; set; } = null!;

    [Column] public string RealmNameEnGb { get; set; } = null!;

    [Column] public string RealmSlug { get; set; } = null!;

    [Column] public BlizzardRegion RealmRegion { get; set; }

    [Column] public BlizzardRealmVersion RealmVersion { get; set; }
}