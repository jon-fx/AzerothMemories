﻿namespace AzerothMemories.Blizzard.Models.GameDataApi;

/// <summary>
/// A reference to a mount.
/// </summary>
public record MountReference
{
    /// <summary>
    /// Gets the key for the mount.
    /// </summary>
    [JsonPropertyName("key")]
    public Self Key { get; init; }

    /// <summary>
    /// Gets the name of the mount.
    /// </summary>
    [JsonPropertyName("name")]
    public Name Name { get; init; }

    /// <summary>
    /// Gets the ID of the mount.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }
}