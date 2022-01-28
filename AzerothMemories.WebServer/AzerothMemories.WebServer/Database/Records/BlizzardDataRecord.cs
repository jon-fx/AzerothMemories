using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table("Blizzard_Data")]
public sealed class BlizzardDataRecord : IDatabaseRecord
{
    [Key] public long Id { get; set; }

    [Column] public long TagId { get; set; }
    [Column] public PostTagType TagType { get; set; }
    [Column] public string Key { get; set; }

    [Column] public string Media { get; set; }

    [Column] public string Name_EnUs { get; set; }
    [Column] public string Name_KoKr { get; set; }
    [Column] public string Name_FrFr { get; set; }
    [Column] public string Name_DeDe { get; set; }
    [Column] public string Name_ZhCn { get; set; }
    [Column] public string Name_EsEs { get; set; }
    [Column] public string Name_ZhTw { get; set; }
    [Column] public string Name_EnGb { get; set; }
    [Column] public string Name_EsMx { get; set; }
    [Column] public string Name_RuRu { get; set; }
    [Column] public string Name_PtBr { get; set; }
    [Column] public string Name_ItIt { get; set; }
    [Column] public string Name_PtPt { get; set; }

    public BlizzardDataRecord()
    {
        //Name = new BlizzardDataRecordLocal();
    }

    public BlizzardDataRecord(PostTagType tagType, long tagId) : this()
    {
        TagId = tagId;
        TagType = tagType;
        Key = $"{TagType}-{TagId}";
    }

    //public bool UpdateMedia(string media)
    //{
    //    var changed = false;

    //    CheckAndChange.Check(ref Media, media, ref changed);

    //    return changed;
    //}
}