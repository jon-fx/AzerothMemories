namespace AzerothMemories.WebServer.Blizzard.Models.GameDataApi;

/// <summary>
/// A reference to a zone.
/// </summary>
public record ZoneReference
{
    /// <summary>
    /// Gets the key for the zone.
    /// </summary>
    [JsonPropertyName("key")]
    public Self Key { get; init; }

    /// <summary>
    /// Gets the name of the zone.
    /// </summary>
    [JsonPropertyName("name")]
    public Name Name { get; init; }
}