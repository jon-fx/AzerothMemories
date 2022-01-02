namespace AzerothMemories.WebServer.Blizzard.Models;

public record Name
{
    [JsonPropertyName("it_IT")]
    public string It_IT { get; init; }

    [JsonPropertyName("ru_RU")]
    public string Ru_RU { get; init; }

    [JsonPropertyName("en_GB")]
    public string En_GB { get; init; }

    [JsonPropertyName("zh_TW")]
    public string Zh_TW { get; init; }

    [JsonPropertyName("ko_KR")]
    public string Ko_KR { get; init; }

    [JsonPropertyName("en_US")]
    public string En_US { get; init; }

    [JsonPropertyName("es_MX")]
    public string Es_MX { get; init; }

    [JsonPropertyName("pt_BR")]
    public string Pt_BR { get; init; }

    [JsonPropertyName("es_ES")]
    public string Es_ES { get; init; }

    [JsonPropertyName("zh_CN")]
    public string Zh_CN { get; init; }

    [JsonPropertyName("fr_FR")]
    public string Fr_FR { get; init; }

    [JsonPropertyName("de_DE")]
    public string De_DE { get; init; }

    [JsonPropertyName("pt_PT")]
    public string Pt_PT { get; init; }
}