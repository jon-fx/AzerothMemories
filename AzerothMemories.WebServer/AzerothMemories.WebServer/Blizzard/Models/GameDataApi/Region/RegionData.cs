﻿namespace AzerothMemories.Blizzard.Models.GameDataApi;

/// <summary>
/// A region.
/// </summary>
public record RegionData
{
    /// <summary>
    /// Gets links for the region.
    /// </summary>
    [JsonPropertyName("_links")]
    public Links Links { get; init; }

    /// <summary>
    /// Gets the ID of the region.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }

    /// <summary>
    /// Gets the name of the region.
    /// </summary>
    [JsonPropertyName("name")]
    public Name Name { get; init; }

    /// <summary>
    /// Gets the tag for the region.
    /// </summary>
    [JsonPropertyName("tag")]
    public string Tag { get; init; }
}