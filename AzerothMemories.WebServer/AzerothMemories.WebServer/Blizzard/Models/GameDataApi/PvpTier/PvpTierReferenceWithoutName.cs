﻿namespace AzerothMemories.WebServer.Blizzard.Models.GameDataApi;

/// <summary>
/// A reference to a PvP tier.
/// </summary>
public record PvpTierReferenceWithoutName
{
    /// <summary>
    /// Gets the key for the PvP tier.
    /// </summary>
    [JsonPropertyName("key")]
    public Self Key { get; init; }

    /// <summary>
    /// Gets the ID of the PvP tier.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }
}