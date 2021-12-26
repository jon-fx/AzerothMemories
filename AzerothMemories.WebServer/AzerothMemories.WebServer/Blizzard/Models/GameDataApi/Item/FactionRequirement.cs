﻿namespace AzerothMemories.Blizzard.Models.GameDataApi;

/// <summary>
/// A faction requirement for an item.
/// </summary>
public record FactionRequirement
{
    /// <summary>
    /// Gets the faction.
    /// </summary>
    [JsonPropertyName("value")]
    public EnumType Value { get; init; }

    /// <summary>
    /// Gets the display string for the faction requirement.
    /// </summary>
    [JsonPropertyName("display_string")]
    public Name DisplayString { get; init; }
}