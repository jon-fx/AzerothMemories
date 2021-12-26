﻿namespace AzerothMemories.Blizzard.Models.GameDataApi;

/// <summary>
/// A spell tooltip.
/// </summary>
public record SpellTooltip
{
    /// <summary>
    /// Gets a reference to the spell.
    /// </summary>
    [JsonPropertyName("spell")]
    public SpellReference Spell { get; init; }

    /// <summary>
    /// Gets the description of the spell.
    /// </summary>
    [JsonPropertyName("description")]
    public Name Description { get; init; }

    /// <summary>
    /// Gets the cast time of the spell.
    /// </summary>
    [JsonPropertyName("cast_time")]
    public string CastTime { get; init; }

    /// <summary>
    /// Gets the range of the spell.
    /// </summary>
    [JsonPropertyName("range")]
    public string Range { get; init; }

    /// <summary>
    /// Gets the cooldown of the spell.
    /// </summary>
    [JsonPropertyName("cooldown")]
    public string Cooldown { get; init; }

    /// <summary>
    /// Gets the power cost of the spell.
    /// </summary>
    [JsonPropertyName("power_cost")]
    public string PowerCost { get; init; }
}