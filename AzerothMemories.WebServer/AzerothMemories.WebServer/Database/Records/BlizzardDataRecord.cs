using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class BlizzardDataRecord : IDatabaseRecord
{
    public const string TableName = "Blizzard_Data";

    [Key] public long Id { get; set; }

    [Column] public long TagId { get; set; }

    [Column] public PostTagType TagType { get; set; }

    [Column] public string Key { get; set; }

    [Column] public string Media { get; set; }

    [Column] public BlizzardDataRecordLocal Name { get; set; }

    public BlizzardDataRecord()
    {
        Name = new BlizzardDataRecordLocal();
    }

    public BlizzardDataRecord(PostTagType tagType, long tagId) : this()
    {
        TagId = tagId;
        TagType = tagType;
        Key = $"{TagType}-{TagId}";
    }
}