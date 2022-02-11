﻿namespace AzerothMemories.WebBlazor.ViewModels;

public sealed record DailyActivityResultsUser
{
    [JsonInclude] public long AccountId { get; init; }
    [JsonInclude] public int Year { get; init; }
    [JsonInclude] public long StartTimeMs { get; init; }
    [JsonInclude] public long EndTimeMs { get; init; }
    [JsonInclude] public List<PostTagInfo> Achievements { get; init; } = new();
    [JsonInclude] public List<PostTagInfo> FirstAchievements { get; init; } = new();
    [JsonInclude] public List<DailyActivityResultsUserPostInfo> MyMemories { get; init; } = new();
}