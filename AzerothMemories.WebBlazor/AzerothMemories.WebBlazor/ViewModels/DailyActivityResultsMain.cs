namespace AzerothMemories.WebBlazor.ViewModels;

public sealed record DailyActivityResultsMain
{
    [JsonInclude] public int Year { get; init; }
    [JsonInclude] public string ZoneId { get; init; }
    [JsonInclude] public long StartTimeMs { get; init; }
    [JsonInclude] public long EndTimeMs { get; init; }
    [JsonInclude] public int TotalTags { get; set; }
    [JsonInclude] public int TotalAchievements { get; set; }
    [JsonInclude] public List<PostTagInfo> TopTags { get; init; } = new();
    [JsonInclude] public List<PostTagInfo> TopAchievements { get; init; } = new();
    [JsonInclude] public List<PostTagInfo> FirstTags { get; init; } = new();
    [JsonInclude] public List<PostTagInfo> FirstAchievements { get; init; } = new();
}