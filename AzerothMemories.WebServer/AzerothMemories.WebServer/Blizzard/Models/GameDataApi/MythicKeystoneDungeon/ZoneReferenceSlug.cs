namespace AzerothMemories.WebServer.Blizzard.Models.GameDataApi;

/// <summary>
/// A reference to a zone.
/// </summary>
public record ZoneReferenceSlug
{
    /// <summary>
    /// Gets the slug for the zone.
    /// </summary>
    [JsonPropertyName("slug")]
    public string Slug { get; init; }
}