namespace AzerothMemories.Blizzard.Models.GameDataApi;

/// <summary>
/// A reference to a skill tier.
/// </summary>
public record SkillTierReferenceWithoutKey
{
    /// <summary>
    /// Gets the name of the skill tier.
    /// </summary>
    [JsonPropertyName("name")]
    public Name Name { get; init; }

    /// <summary>
    /// Gets the ID of the skill tier.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }
}