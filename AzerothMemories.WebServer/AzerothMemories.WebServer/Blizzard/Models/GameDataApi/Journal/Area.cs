namespace AzerothMemories.Blizzard.Models.GameDataApi;

/// <summary>
/// An area.
/// </summary>
public record Area
{
    /// <summary>
    /// Gets the name of the area.
    /// </summary>
    [JsonPropertyName("name")]
    public Name Name { get; init; }

    /// <summary>
    /// Gets the ID of the area.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }
}