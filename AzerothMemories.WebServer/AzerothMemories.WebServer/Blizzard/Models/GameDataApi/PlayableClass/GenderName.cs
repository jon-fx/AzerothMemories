namespace AzerothMemories.Blizzard.Models.GameDataApi;

/// <summary>
/// The gender-specific names for a playable class, race, or title.
/// </summary>
public record GenderName
{
    /// <summary>
    /// Gets the name for male characters.
    /// </summary>
    [JsonPropertyName("male")]
    public Name Male { get; init; }

    /// <summary>
    /// Gets the name for female characters.
    /// </summary>
    [JsonPropertyName("female")]
    public Name Female { get; init; }
}