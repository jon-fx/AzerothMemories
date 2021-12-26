namespace AzerothMemories.Blizzard.Models.ProfileApi;

/// <summary>
/// A Mythic Keystone dungeon run.
/// </summary>
public record MythicKeystoneRun
{
    /// <summary>
    /// Gets the timestamp when the run was completed.
    /// </summary>
    [JsonPropertyName("completed_timestamp")]
    public long CompletedTimestamp { get; init; }

    /// <summary>
    /// Gets the duration.
    /// </summary>
    [JsonPropertyName("duration")]
    public long Duration { get; init; }

    /// <summary>
    /// Gets the keystone level.
    /// </summary>
    [JsonPropertyName("keystone_level")]
    public int KeystoneLevel { get; init; }

    /// <summary>
    /// Gets references to the Mythic Keystone affixes for this run.
    /// </summary>
    [JsonPropertyName("keystone_affixes")]
    public MythicKeystoneAffixReference[] KeystoneAffixes { get; init; }

    /// <summary>
    /// Gets the party members for the Mythic Keystone run.
    /// </summary>
    [JsonPropertyName("members")]
    public MythicKeystonePartyMember[] Members { get; init; }

    /// <summary>
    /// A reference to the Mythic Keystone dungeon.
    /// </summary>
    [JsonPropertyName("dungeon")]
    public MythicKeystoneDungeonReference Dungeon { get; init; }

    /// <summary>
    /// Gets a value indicating whether the run was completed within the time limit.
    /// </summary>
    [JsonPropertyName("is_completed_within_time")]
    public bool IsCompletedWithinTime { get; init; }
}