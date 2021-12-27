namespace AzerothMemories.WebServer.Blizzard.Models.GameDataApi;

/// <summary>
/// A reference to an azerite essence.
/// </summary>
public record AzeriteEssenceReference
{
    /// <summary>
    /// Gets the key for the azerite essence.
    /// </summary>
    [JsonPropertyName("key")]
    public Self Key { get; init; }

    /// <summary>
    /// Gets the name of the azerite essence.
    /// </summary>
    [JsonPropertyName("name")]
    public Name Name { get; init; }

    /// <summary>
    /// Gets the ID of the azerite essence.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }
}