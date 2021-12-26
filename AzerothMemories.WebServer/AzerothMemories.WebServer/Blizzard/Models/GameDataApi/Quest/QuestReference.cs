namespace AzerothMemories.Blizzard.Models.GameDataApi;

/// <summary>
/// A reference to a quest.
/// </summary>
public record QuestReference
{
    /// <summary>
    /// Gets the key for the quest.
    /// </summary>
    [JsonPropertyName("key")]
    public Self Key { get; init; }

    /// <summary>
    /// Gets the name of the quest.
    /// </summary>
    [JsonPropertyName("name")]
    public Name Name { get; init; }

    /// <summary>
    /// Gets the ID of the quest.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }
}