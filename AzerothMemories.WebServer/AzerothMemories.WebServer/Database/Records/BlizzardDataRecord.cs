using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class BlizzardDataRecord : IDatabaseRecord
{
    public const string TableName = "Blizzard_Data";

    [Key] public int Id { get; init; }

    [Column] public int TagId { get; init; }

    [Column] public PostTagType TagType { get; init; }

    [Column] public string Key { get; init; } = null!;

    [Column] public string? Media { get; set; }

    [Column, Required] public BlizzardDataRecordLocal Name { get; init; }

    [Column] public Instant MinTagTime { get; set; }

    public BlizzardDataRecord()
    {
        Name = new BlizzardDataRecordLocal();
    }

    public BlizzardDataRecord(PostTagType tagType, int tagId) : this()
    {
        TagId = tagId;
        TagType = tagType;
        Key = $"{TagType}-{TagId}";
    }
}