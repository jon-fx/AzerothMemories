namespace AzerothMemories.WebServer.Blizzard.Models.GameDataApi;

/// <summary>
/// A reference to an item subclass.
/// </summary>
public record ItemSubclassReference
{
    /// <summary>
    /// Gets the key for the item subclass.
    /// </summary>
    [JsonPropertyName("key")]
    public Self Key { get; init; }

    /// <summary>
    /// Gets the name of the item subclass.
    /// </summary>
    [JsonPropertyName("name")]
    public Name Name { get; init; }

    /// <summary>
    /// Gets the ID of the item subclass.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }
}