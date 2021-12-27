namespace AzerothMemories.WebServer.Blizzard.Models;

public record Name
{
    [JsonPropertyName("it_IT")]
    public string ItIT { get; init; }

    [JsonPropertyName("ru_RU")]
    public string RuRU { get; init; }

    [JsonPropertyName("en_GB")]
    public string EnGB { get; init; }

    [JsonPropertyName("zh_TW")]
    public string ZhTW { get; init; }

    [JsonPropertyName("ko_KR")]
    public string KoKR { get; init; }

    [JsonPropertyName("en_US")]
    public string EnUS { get; init; }

    [JsonPropertyName("es_MX")]
    public string EsMX { get; init; }

    [JsonPropertyName("pt_BR")]
    public string PtBR { get; init; }

    [JsonPropertyName("es_ES")]
    public string EsES { get; init; }

    [JsonPropertyName("zh_CN")]
    public string ZhCN { get; init; }

    [JsonPropertyName("fr_FR")]
    public string FrFR { get; init; }

    [JsonPropertyName("de_DE")]
    public string DeDE { get; init; }

    public bool IsNull()
    {
        return string.IsNullOrEmpty(ItIT) &&
               string.IsNullOrEmpty(RuRU) &&
               string.IsNullOrEmpty(EnGB) &&
               string.IsNullOrEmpty(ZhTW) &&
               string.IsNullOrEmpty(KoKR) &&
               string.IsNullOrEmpty(EnUS) &&
               string.IsNullOrEmpty(EsMX) &&
               string.IsNullOrEmpty(PtBR) &&
               string.IsNullOrEmpty(EsES) &&
               string.IsNullOrEmpty(ZhCN) &&
               string.IsNullOrEmpty(FrFR) &&
               string.IsNullOrEmpty(DeDE);
    }
}