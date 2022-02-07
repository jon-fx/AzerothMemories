﻿namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class ActivityResultsChild
{
    [JsonInclude] public int Year { get; set; }
    [JsonInclude] public long StartTimeMs { get; set; }
    [JsonInclude] public long EndTimeMs { get; set; }
    [JsonInclude] public int TotalTags { get; set; }
    [JsonInclude] public int TotalAchievements { get; set; }
    [JsonInclude] public List<ActivityResultsTuple> TopTags { get; set; } = new();
    [JsonInclude] public List<ActivityResultsTuple> TopAchievements { get; set; } = new();
    [JsonInclude] public List<PostTagInfo> FirstTags { get; set; } = new();
    [JsonInclude] public List<PostTagInfo> FirstAchievements { get; set; } = new();
}