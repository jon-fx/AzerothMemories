namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class ActivitySetUser
{
    public int Year { get; init; }

    public Instant StartTime { get; init; }

    public Instant EndTime { get; init; }

    public HashSet<int> Achievements { get; } = new();

    public HashSet<int> FirstAchievements { get; } = new();

    public HashSet<DailyActivityResultsUserPostInfo> MyMemories { get; } = new();
}