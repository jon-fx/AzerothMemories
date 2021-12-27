namespace AzerothMemories.WebServer.Blizzard.Models.GameDataApi;

/// <summary>
/// A reputation faction.
/// </summary>
public record ReputationFaction
{
    /// <summary>
    /// Gets links for the reputation faction.
    /// </summary>
    [JsonPropertyName("_links")]
    public Links Links { get; init; }

    /// <summary>
    /// Gets the ID of the reputation faction.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }

    /// <summary>
    /// Gets the name of the reputation faction.
    /// </summary>
    [JsonPropertyName("name")]
    public Name Name { get; init; }

    /// <summary>
    /// Gets the description of the reputation faction.
    /// </summary>
    [JsonPropertyName("description")]
    public Name Description { get; init; }

    /// <summary>
    /// Gets the reputation tiers for the reputation faction.
    /// </summary>
    [JsonPropertyName("reputation_tiers")]
    public ReputationTierReferenceWithoutName ReputationTiers { get; init; }
}