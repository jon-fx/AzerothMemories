namespace AzerothMemories.Blizzard.Models.GameDataApi;

/// <summary>
/// Currency display strings for a price.
/// </summary>
public record CurrencyDisplay
{
    /// <summary>
    /// Gets the header.
    /// </summary>
    [JsonPropertyName("header")]
    public Name Header { get; init; }

    /// <summary>
    /// Gets the gold portion of the price.
    /// </summary>
    [JsonPropertyName("gold")]
    public Name Gold { get; init; }

    /// <summary>
    /// Gets the silver portion of the price.
    /// </summary>
    [JsonPropertyName("silver")]
    public Name Silver { get; init; }

    /// <summary>
    /// Gets the copper portion of the price.
    /// </summary>
    [JsonPropertyName("copper")]
    public Name Copper { get; init; }
}