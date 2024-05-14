namespace AzerothMemories.WebBlazor.Common;

public static class XExtensions
{
    public static IEnumerable<TValue> SafeEnumerable<TValue>(this IEnumerable<TValue?>? values)
    {
        if (values == null)
        {
            return [];
        }

        return values.Where(x => x != null).Cast<TValue>().ToArray();
    }

    public static int GetIdSafe(this AccountViewModel? accountViewModel)
    {
        return accountViewModel?.Id ?? -1;
    }

    public static string GetDisplayName(this AccountViewModel? accountViewModel)
    {
        if (accountViewModel == null || string.IsNullOrWhiteSpace(accountViewModel.Username))
        {
            return "Unknown";
        }

        return accountViewModel.Username;
    }

    public static string GetAvatarText(this AccountViewModel? accountViewModel)
    {
        if (accountViewModel == null || string.IsNullOrWhiteSpace(accountViewModel.Username))
        {
            return "?";
        }

        return accountViewModel.Username[0].ToString();
    }

    public static CharacterViewModel[] GetCharactersSafe(this AccountViewModel? accountViewModel)
    {
        if (accountViewModel == null || accountViewModel.CharactersArray == null || accountViewModel.CharactersArray.Length == 0)
        {
            return [];
        }

        return accountViewModel.CharactersArray.Where(x => x.CharacterStatus == CharacterStatus2.None).OrderByDescending(x => x.Level).ThenBy(x => x.Name).ToArray();
    }

    public static CharacterViewModel[] GetAllCharactersSafe(this AccountViewModel? accountViewModel)
    {
        if (accountViewModel == null || accountViewModel.CharactersArray == null || accountViewModel.CharactersArray.Length == 0)
        {
            return [];
        }

        return accountViewModel.CharactersArray.OrderByDescending(x => x.Level).ThenBy(x => x.Name).ToArray();
    }

    public static string GetDisplayName(this CharacterViewModel? characterViewModel)
    {
        if (string.IsNullOrWhiteSpace(characterViewModel?.Name))
        {
            return "Unknown";
        }

        return characterViewModel.Name;
    }

    public static string? GetAvatarLinkWithFallBack(this CharacterViewModel? characterViewModel)
    {
        if (characterViewModel == null)
        {
            return null;
        }

        return GetAvatarStringWithFallBack(characterViewModel.AvatarLink, characterViewModel.Race, characterViewModel.Gender);
    }

    public static string GetAvatarText(this CharacterViewModel? characterViewModel)
    {
        if (characterViewModel == null || string.IsNullOrWhiteSpace(characterViewModel.Name))
        {
            return "?";
        }

        return characterViewModel.Name[0].ToString();
    }

    public static string GetAvatarStringWithFallBack(string? avatarLink, byte race, byte gender)
    {
        if (string.IsNullOrEmpty(avatarLink))
        {
            avatarLink = "https://render-us.worldofwarcraft.com/character/tichondrius/00/000000000-avatar.jpg";
        }

        return $"{avatarLink}?alt=/shadow/avatar/{race}-{gender}.jpg";
    }

    public static string GetDisplayName(this GuildViewModel? guildViewModel)
    {
        if (guildViewModel == null || string.IsNullOrWhiteSpace(guildViewModel.Name))
        {
            return "Unknown";
        }

        return guildViewModel.Name;
    }

    public static string GetAvatarText(this GuildViewModel? guildViewModel)
    {
        if (guildViewModel == null || string.IsNullOrWhiteSpace(guildViewModel.Name))
        {
            return "?";
        }

        return guildViewModel.Name[0].ToString();
    }
}