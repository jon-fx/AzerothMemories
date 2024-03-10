using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class CharacterMountRecord : IDatabaseRecordWithVersion
{
    public const string TableName = "Characters_Mounts";

    [Key] public int Id { get; init; }

    [Column] public int? AccountId { get; set; }

    [Column] public int CharacterId { get; init; }

    [Column] public int MountId { get; init; }

    [Column] public Instant MountTimeStamp { get; init; }

    public uint RowVersion { get; set; }
}