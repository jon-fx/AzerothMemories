namespace AzerothMemories.WebServer.Database.Records;

[Table("Blizzard_Data")]
public sealed  class BlizzardDataRecord: IDatabaseRecord
{
    [Column(IsPrimaryKey = true, IsIdentity = true)] public long Id { get; set; }

    [Column, NotNull] public long TagId;
    [Column, NotNull] public PostTagType TagType;
    [Column, NotNull] public string Key;

    [Column, NotNull] public string Media;

    [Column("Name_EnUs", MemberName = ".En_Us")]
    [Column("Name_KoKr", MemberName = ".Ko_Kr")]
    [Column("Name_FrFr", MemberName = ".Fr_Fr")]
    [Column("Name_DeDe", MemberName = ".De_De")]
    [Column("Name_ZhCn", MemberName = ".Zh_Cn")]
    [Column("Name_EsEs", MemberName = ".Es_Es")]
    [Column("Name_ZhTw", MemberName = ".Zh_Tw")]
    [Column("Name_EnGb", MemberName = ".En_Gb")]
    [Column("Name_EsMx", MemberName = ".Es_Mx")]
    [Column("Name_RuRu", MemberName = ".Ru_Ru")]
    [Column("Name_PtBr", MemberName = ".Pt_Br")]
    [Column("Name_ItIt", MemberName = ".It_It")]
    [Column("Name_PtPt", MemberName = ".Pt_Pt")]
    public BlizzardDataRecordLocal Name;

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

    public bool UpdateMedia(string media)
    {
        var changed = false;

        CheckAndChange.Check(ref Media, media, ref changed);

        return changed;
    }
}