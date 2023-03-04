namespace AzerothMemories.WebServer.Blizzard.Models.ProfileApi;

/// <summary>
/// A background.
/// </summary>
public record Background
{
    /// <summary>
    /// Gets the background color.
    /// </summary>
    [JsonPropertyName("color")]
    public ProfileColor Color { get; init; }
}