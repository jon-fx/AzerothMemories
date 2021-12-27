namespace AzerothMemories.WebServer.Blizzard.Models.GameDataApi;

/// <summary>
/// A reference to a realm.
/// </summary>
public record RealmReferenceWithoutKey
{
    /// <summary>
    /// Gets the name of the realm.
    /// </summary>
    [JsonPropertyName("name")]
    public Name Name { get; init; }

    /// <summary>
    /// Gets the ID of the realm.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }

    /// <summary>
    /// Gets a slug for the realm.
    /// </summary>
    [JsonPropertyName("slug")]
    public string Slug { get; init; }
}