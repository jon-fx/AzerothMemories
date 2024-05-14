using AzerothMemories.WebServer.Blizzard.Models;

namespace AzerothMemories.WebServer.Blizzard;

public static class Extensions
{
    public static CharacterFaction AsFaction(this EnumType? enumType)
    {
        if (enumType == null)
        {
            return CharacterFaction.None;
        }

        if (string.IsNullOrWhiteSpace(enumType.Type))
        {
            return CharacterFaction.None;
        }

        if (enumType.Type[0] == 'A')
        {
            return CharacterFaction.Alliance;
        }

        return enumType.Type[0] == 'H' ? CharacterFaction.Horde : CharacterFaction.None;
    }

    public static byte AsGender(this EnumType? enumType)
    {
        if (enumType == null)
        {
            return 0;
        }

        if (string.IsNullOrWhiteSpace(enumType.Type))
        {
            return 0;
        }

        return enumType.Type.StartsWith('M') ? (byte)0 : (byte)1;
    }
}