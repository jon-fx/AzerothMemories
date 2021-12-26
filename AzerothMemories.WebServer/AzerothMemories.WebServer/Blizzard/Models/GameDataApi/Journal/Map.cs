namespace AzerothMemories.Blizzard.Models.GameDataApi;

/// <summary>
/// A map.
/// </summary>
public record Map
{
    /// <summary>
    /// Gets the name of the map.
    /// </summary>
    [JsonPropertyName("name")]
    public Name Name { get; init; }

    /// <summary>
    /// Gets the ID of the map.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }
}