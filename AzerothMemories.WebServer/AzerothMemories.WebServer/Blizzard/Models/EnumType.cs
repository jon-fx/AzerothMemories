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
        if (string.IsNullOrWhiteSpace(Type))
        {
            return CharacterFaction.None;
        }
        
        if (Type[0] == 'A')
        {
            return CharacterFaction.Alliance;
        }

        return Type[0] == 'H' ? CharacterFaction.Horde : CharacterFaction.None;
    }

    public byte AsGender()
    {
        if (string.IsNullOrWhiteSpace(Type))
        {
            return 0;
        }
        
        return Type.StartsWith('M') ? (byte)0 : (byte)1;
    }
}