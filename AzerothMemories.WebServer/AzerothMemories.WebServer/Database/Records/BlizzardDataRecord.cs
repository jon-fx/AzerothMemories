using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class BlizzardDataRecord : IDatabaseRecord
{
    public const string TableName = "Blizzard_Data";

    [Key] public int Id { get; set; }

    [Column] public int TagId { get; set; }

    [Column] public PostTagType TagType { get; set; }

    [Column] public string Key { get; set; }

    [Column] public string Media { get; set; }

    [Column, Required] public BlizzardDataRecordLocal Name { get; set; }

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