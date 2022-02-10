namespace AzerothMemories.WebBlazor.ViewModels;

public sealed record DailyActivityResultsMain
{
    [JsonInclude] public int Year { get; init; }
    [JsonInclude] public long StartTimeMs { get; init; }
    [JsonInclude] public long EndTimeMs { get; init; }
    [JsonInclude] public int TotalTags { get; set; }
    [JsonInclude] public int TotalAchievements { get; set; }
    [JsonInclude] public List<ActivityResultsTuple> TopTags { get; init; } = new();
    [JsonInclude] public List<ActivityResultsTuple> TopAchievements { get; init; } = new();
    [JsonInclude] public List<PostTagInfo> FirstTags { get; init; } = new();
    [JsonInclude] public List<PostTagInfo> FirstAchievements { get; init; } = new();
}