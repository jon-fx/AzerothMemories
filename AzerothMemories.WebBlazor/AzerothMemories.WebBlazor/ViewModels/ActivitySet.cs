namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class ActivitySet
{
    public int TotalAchievements { get; set; }
    public List<string> FirstTags { get; set; } = new();
    public List<int> FirstAchievements { get; set; } = new();
    public Dictionary<int, int> AchievementCounts { get; set; } = new();
    public Dictionary<string, int> PostTags { get; set; } = new();
}