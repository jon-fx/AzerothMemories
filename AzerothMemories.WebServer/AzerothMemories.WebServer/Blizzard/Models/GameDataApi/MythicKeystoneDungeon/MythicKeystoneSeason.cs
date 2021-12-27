namespace AzerothMemories.WebServer.Blizzard.Models.GameDataApi;

/// <summary>
/// A mythic keystone season.
/// </summary>
public record MythicKeystoneSeason
{
    /// <summary>
    /// Gets links for the mythic keystone season.
    /// </summary>
    [JsonPropertyName("_links")]
    public Links Links { get; init; }

    /// <summary>
    /// Gets the ID of the mythic keystone season.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }

    /// <summary>
    /// Gets the start timestamp of the mythic keystone season.
    /// </summary>
    [JsonPropertyName("start_timestamp")]
    public long StartTimestamp { get; init; }

    /// <summary>
    /// Gets the end timestamp of the mythic keystone season.
    /// </summary>
    [JsonPropertyName("end_timestamp")]
    public long EndTimestamp { get; init; }

    /// <summary>
    /// Gets refernces to the periods in the mythic keystone season.
    /// </summary>
    [JsonPropertyName("periods")]
    public MythicKeystonePeriodReference[] Periods { get; init; }
}