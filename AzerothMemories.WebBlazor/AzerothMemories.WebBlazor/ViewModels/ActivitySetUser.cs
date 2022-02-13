namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class ActivitySetUser
{
    public int Year { get; init; }

    public HashSet<int> Achievements { get; init; } = new();

    //public HashSet<string> FirstTags { get; init; } = new();

    public HashSet<int> FirstAchievements { get; init; } = new();

    public HashSet<DailyActivityResultsUserPostInfo> MyMemories { get; init; } = new();
}