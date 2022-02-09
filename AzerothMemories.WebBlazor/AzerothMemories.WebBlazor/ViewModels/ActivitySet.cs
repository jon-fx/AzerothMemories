namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class ActivitySet
{
    public int TotalAchievements { get; set; }
    public HashSet<string> FirstTags { get; set; } = new();
    public HashSet<int> FirstAchievements { get; set; } = new();
    public Dictionary<int, int> AchievementCounts { get; set; } = new();
    public Dictionary<string, int> PostTags { get; set; } = new();
}