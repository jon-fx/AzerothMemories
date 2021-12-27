using AzerothMemories.WebServer.Blizzard.Models.GameDataApi;

namespace AzerothMemories.WebServer.Blizzard.Models.ProfileApi;

/// <summary>
/// A PvP talent slot.
/// </summary>
public record PvpTalentSlot
{
    /// <summary>
    /// Gets the selected PvP talent.
    /// </summary>
    [JsonPropertyName("selected")]
    public PvpTalentElementForAbility Selected { get; init; }

    /// <summary>
    /// Gets the PvP talent slot number.
    /// </summary>
    [JsonPropertyName("slot_number")]
    public int SlotNumber { get; init; }
}