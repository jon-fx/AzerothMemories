namespace AzerothMemories.WebServer.Blizzard.Models.GameDataApi;

/// <summary>
/// A list of available guild crest colors.
/// </summary>
public record GuildCrestColors
{
    /// <summary>
    /// Gets the guild crest emblems.
    /// </summary>
    [JsonPropertyName("emblems")]
    public ProfileColor[] Emblems { get; init; }

    /// <summary>
    /// Gets the guild crest borders.
    /// </summary>
    [JsonPropertyName("borders")]
    public ProfileColor[] Borders { get; init; }

    /// <summary>
    /// Gets the guild crest borders.
    /// </summary>
    [JsonPropertyName("backgrounds")]
    public ProfileColor[] Backgrounds { get; init; }
}