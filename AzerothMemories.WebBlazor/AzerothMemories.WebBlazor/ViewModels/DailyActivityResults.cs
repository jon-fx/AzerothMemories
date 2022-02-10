namespace AzerothMemories.WebBlazor.ViewModels;

public sealed record DailyActivityResults
{
    [JsonInclude] public int Year { get; init; }
    [JsonInclude] public DailyActivityResultsMain Main { get; init; }
    [JsonInclude] public DailyActivityResultsUser User { get; init; }
}