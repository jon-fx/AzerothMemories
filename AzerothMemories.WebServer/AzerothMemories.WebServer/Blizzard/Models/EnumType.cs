namespace AzerothMemories.WebServer.Blizzard.Models;

/// <summary>
/// An enumerated type.
/// </summary>
public record EnumType
{
    /// <summary>
    /// Gets the type code for this enumerated value.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; init; }

    /// <summary>
    /// Gets the name of the enumerated value.
    /// </summary>
    [JsonPropertyName("name")]
    public Name Name { get; init; }

    public CharacterFaction AsFaction()
    {
        if (Type[0] == 'A')
        {
            return CharacterFaction.Alliance;
        }

        if (Type[0] == 'H')
        {
            return CharacterFaction.Horde;
        }

        return CharacterFaction.None;
    }

    public byte AsGender()
    {
        return Type.StartsWith("M") ? (byte)0 : (byte)1;
    }
}