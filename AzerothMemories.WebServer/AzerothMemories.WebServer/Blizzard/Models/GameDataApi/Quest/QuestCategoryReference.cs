namespace AzerothMemories.WebServer.Blizzard.Models.GameDataApi;

/// <summary>
/// A reference to a quest category.
/// </summary>
public record QuestCategoryReference
{
    /// <summary>
    /// Gets the key for the quest category.
    /// </summary>
    [JsonPropertyName("key")]
    public Self Key { get; init; }

    /// <summary>
    /// Gets the name of the quest category.
    /// </summary>
    [JsonPropertyName("name")]
    public Name Name { get; init; }

    /// <summary>
    /// Gets the ID of the quest category.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }
}