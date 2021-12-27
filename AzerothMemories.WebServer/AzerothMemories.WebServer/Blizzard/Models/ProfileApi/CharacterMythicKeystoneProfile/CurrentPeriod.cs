using AzerothMemories.WebServer.Blizzard.Models.GameDataApi;

namespace AzerothMemories.WebServer.Blizzard.Models.ProfileApi;

/// <summary>
/// A current period.
/// </summary>
public record CurrentPeriod
{
    /// <summary>
    /// Gets a reference to the Mythic Keystone period.
    /// </summary>
    [JsonPropertyName("period")]
    public MythicKeystonePeriodReference Period { get; init; }

    /// <summary>
    /// Gets the best runs during the current period.
    /// </summary>
    [JsonPropertyName("best_runs")]
    public MythicKeystoneRun[] BestRuns { get; init; }
}