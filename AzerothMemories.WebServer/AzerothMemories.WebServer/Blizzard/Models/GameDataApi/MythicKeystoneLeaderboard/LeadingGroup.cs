namespace AzerothMemories.Blizzard.Models.GameDataApi;

/// <summary>
/// A leading group.
/// </summary>
public record LeadingGroup
{
    /// <summary>
    /// Gets the ranking of the group.
    /// </summary>
    [JsonPropertyName("ranking")]
    public int Ranking { get; init; }

    /// <summary>
    /// Gets the duration of the run.
    /// </summary>
    [JsonPropertyName("duration")]
    public long Duration { get; init; }

    /// <summary>
    /// Gets the timestamp of the completion.
    /// </summary>
    [JsonPropertyName("completed_timestamp")]
    public long CompletedTimestamp { get; init; }

    /// <summary>
    /// Gets the keystone level.
    /// </summary>
    [JsonPropertyName("keystone_level")]
    public int KeystoneLevel { get; init; }

    /// <summary>
    /// Gets the members of the group.
    /// </summary>
    [JsonPropertyName("members")]
    public Member[] Members { get; init; }
}