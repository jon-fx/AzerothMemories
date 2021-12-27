namespace AzerothMemories.WebServer.Blizzard.Models.GameDataApi;

/// <summary>
/// A location.
/// </summary>
public record Location
{
    /// <summary>
    /// Gets the name of the location.
    /// </summary>
    [JsonPropertyName("name")]
    public Name Name { get; init; }

    /// <summary>
    /// Gets the ID of the location.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }
}