namespace AzerothMemories.Blizzard.Models.GameDataApi;

/// <summary>
/// A value with an associated display string.
/// </summary>
public record DescribedValue
{
    /// <summary>
    /// Gets the numerical value.
    /// </summary>
    [JsonPropertyName("value")]
    public float Value { get; init; }

    /// <summary>
    /// Gets a display string for the value.
    /// </summary>
    [JsonPropertyName("display_string")]
    public Name DisplayString { get; init; }
}