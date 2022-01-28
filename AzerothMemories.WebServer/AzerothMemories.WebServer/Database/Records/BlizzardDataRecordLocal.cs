using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Owned]
public sealed class BlizzardDataRecordLocal
{
    [Column] public string EnUs { get; set; }
    [Column] public string KoKr { get; set; }
    [Column] public string FrFr { get; set; }
    [Column] public string DeDe { get; set; }
    [Column] public string ZhCn { get; set; }
    [Column] public string EsEs { get; set; }
    [Column] public string ZhTw { get; set; }
    [Column] public string EnGb { get; set; }
    [Column] public string EsMx { get; set; }
    [Column] public string RuRu { get; set; }
    [Column] public string PtBr { get; set; }
    [Column] public string ItIt { get; set; }
    [Column] public string PtPt { get; set; }
}