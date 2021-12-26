﻿namespace AzerothMemories.Blizzard.Models.ProfileApi;

/// <summary>
/// A character achievement statistic.
/// </summary>
public record Statistic
{
    /// <summary>
    /// Gets the ID of the statistic.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }

    /// <summary>
    /// Gets the name of the statistic.
    /// </summary>
    [JsonPropertyName("name")]
    public Name Name { get; init; }

    /// <summary>
    /// Gets the timestamp when the statistic was last updated.
    /// </summary>
    [JsonPropertyName("last_updated_timestamp")]
    public long LastUpdatedTimestamp { get; init; }

    /// <summary>
    /// Gets a quantity associated with the statistic.
    /// </summary>
    [JsonPropertyName("quantity")]
    public float Quantity { get; init; }

    /// <summary>
    /// Gets an optional description of the statistic.
    /// </summary>
    [JsonPropertyName("description")]
    public Name Description { get; init; }
}