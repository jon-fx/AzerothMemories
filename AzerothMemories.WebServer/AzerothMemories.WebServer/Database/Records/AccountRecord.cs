using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

public class AccountRecord
{
    [Key] public long Id { get; set; }

    [Column] public string FusionId { get; set; }

    [Column] public DateTimeOffset CreatedDateTime { get; set; }

    [Column] public long BlizzardId { get; set; }

    [Column] public BlizzardRegion BlizzardRegion { get; set; }

    [Column] public string? BattleTag { get; set; }

    [Column] public string? BattleNetToken { get; set; }

    [Column] public DateTimeOffset BattleNetTokenExpiresAt { get; set; }

    [Column] public string? Username { get; set; }

    [Column] public string? UpdateJob { get; set; }

    [Column] public DateTimeOffset UpdateJobStartTime { get; set; }
}