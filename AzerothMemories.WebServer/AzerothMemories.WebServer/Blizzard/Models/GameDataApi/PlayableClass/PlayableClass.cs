﻿namespace AzerothMemories.WebServer.Blizzard.Models.GameDataApi;

/// <summary>
/// A playable class.
/// </summary>
public record PlayableClass
{
    /// <summary>
    /// Gets links for the playable class.
    /// </summary>
    [JsonPropertyName("_links")]
    public Links Links { get; init; }

    /// <summary>
    /// Gets the ID of the playable class.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }

    /// <summary>
    /// Gets the name of the playable class.
    /// </summary>
    [JsonPropertyName("name")]
    public Name Name { get; init; }

    /// <summary>
    /// Gets the gender-specific names for the playable class.
    /// </summary>
    [JsonPropertyName("gender_name")]
    public GenderName GenderName { get; init; }

    /// <summary>
    /// Gets a reference to the power type used by this playable class.
    /// </summary>
    [JsonPropertyName("power_type")]
    public PowerTypeReference PowerType { get; init; }

    /// <summary>
    /// Gets references to the specializations for this playable class.
    /// </summary>
    [JsonPropertyName("specializations")]
    public PlayableSpecializationReference[] Specializations { get; init; }

    /// <summary>
    /// Gets the media for this playable class.
    /// </summary>
    [JsonPropertyName("media")]
    public Media Media { get; init; }

    /// <summary>
    /// Gets a link to the PVP talent slots for this playable class.
    /// </summary>
    [JsonPropertyName("pvp_talent_slots")]
    public Self PvpTalentSlots { get; init; }
}