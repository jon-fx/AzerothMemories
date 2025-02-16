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

    public static string GetDescription(this AccountViewModel? accountViewModel, IStringLocalizer<BlizzardResources> stringLocalizer)
    {
        var name = accountViewModel.GetDisplayName();
        var totalPostCount = accountViewModel?.TotalPostCount ?? 0;
        var totalMemoriesCount = totalPostCount + accountViewModel?.TotalMemoriesCount ?? 0;
        var characterDesc = string.Empty;
        var characters = accountViewModel.GetCharactersSafeAllVersions();

        if (characters.Length > 0)
        {
            var allCharacterDesc = new List<string>();
            foreach (var character in characters)
            {
                allCharacterDesc.Add(GetDescription(character, stringLocalizer));
            }

            characterDesc = $" Their characters include {string.Join(", ", allCharacterDesc)}";
        }

        return $"{name} has {totalPostCount.ToMetric()} posts and {totalMemoriesCount.ToMetric()} memories.{characterDesc}";
    }

    public static string GetDescription(this CharacterViewModel? characterViewModel, IStringLocalizer<BlizzardResources> stringLocalizer)
    {
        var characterRace = stringLocalizer.GetString($"CharacterRace-{characterViewModel?.Race ?? 0}");
        var characterClass = stringLocalizer.GetString($"CharacterClass-{characterViewModel?.Class ?? 0}");
        var characterRealm = stringLocalizer.GetString($"Realm-{characterViewModel?.RealmId ?? 0}");

        return $"{characterViewModel.GetDisplayName()} on {characterRealm} a level {characterViewModel?.Level ?? 0} {characterRace} {characterClass}";
    }

    public static string GetAvatarText(this AccountViewModel? accountViewModel)
    {
        if (accountViewModel == null || string.IsNullOrWhiteSpace(accountViewModel.Username))
        {
            return "?";
        }

        return accountViewModel.Username[0].ToString();
    }

    public static CharacterViewModel[] GetCharactersSafeAllVersions(this AccountViewModel? accountViewModel)
    {
        if (accountViewModel == null)
        {
            return [];
        }

        var results = new List<CharacterViewModel>();

        if (accountViewModel.CharactersArray != null)
        {
            results.AddRange(accountViewModel.CharactersArray);
        }

        if (accountViewModel.CharactersArrayClassicEra != null)
        {
            results.AddRange(accountViewModel.CharactersArrayClassicEra);
        }

        if (accountViewModel.CharactersArrayClassicProgression != null)
        {
            results.AddRange(accountViewModel.CharactersArrayClassicProgression);
        }

        return results.Where(x => x.CharacterStatus == CharacterStatus2.None).OrderBy(x => x.RealmVersion).ThenByDescending(x => x.Level).ThenBy(x => x.Name).ToArray();
    }

    public static CharacterViewModel[] GetAllCharactersSafeAllVersions(this AccountViewModel? accountViewModel)
    {
        if (accountViewModel == null)
        {
            return [];
        }

        var results = new List<CharacterViewModel>();

        if (accountViewModel.CharactersArray != null)
        {
            results.AddRange(accountViewModel.CharactersArray);
        }

        if (accountViewModel.CharactersArrayClassicEra != null)
        {
            results.AddRange(accountViewModel.CharactersArrayClassicEra);
        }

        if (accountViewModel.CharactersArrayClassicProgression != null)
        {
            results.AddRange(accountViewModel.CharactersArrayClassicProgression);
        }

        return results.OrderBy(x => x.RealmVersion).ThenByDescending(x => x.Level).ThenBy(x => x.Name).ToArray();
    }

    public static CharacterViewModel[] GetCharactersForTagSafe(this AccountViewModel? accountViewModel, PostTagInfo? postTagInfo)
    {
        if (accountViewModel == null || postTagInfo == null || postTagInfo.Type != PostTagType.Type)
        {
            return [];
        }

        return accountViewModel.GetCharactersForTagSafe(postTagInfo.Id);
    }

    public static CharacterViewModel[] GetCharactersForTagSafe(this AccountViewModel? accountViewModel, int tagId)
    {
        if (accountViewModel == null)
        {
            return [];
        }

        CharacterViewModel[]? results;
        if (tagId == 0)        //Retail
        {
            results = accountViewModel.CharactersArray;
        }
        else if (tagId == 1)   //Classic
        {
            results = accountViewModel.CharactersArrayClassicProgression;
        }
        else if (tagId == 2)   //Season of Mastery
        {
            results = accountViewModel.CharactersArrayClassicEra;
        }
        else if (tagId == 3)   //Hardcore
        {
            results = accountViewModel.CharactersArrayClassicEra;
        }
        else if (tagId == 4)   //Season of Discovery
        {
            results = accountViewModel.CharactersArrayClassicEra;
        }
        else if (tagId == 5)   //Anniversary
        {
            results = accountViewModel.CharactersArrayClassicEra;
        }
        else if (tagId == 6)   //Anniversary Hardcore
        {
            results = accountViewModel.CharactersArrayClassicEra;
        }
        else
        {
            throw new NotImplementedException();
        }

        if (results == null)
        {
            return [];
        }

        return results.OrderByDescending(x => x.Level).ThenBy(x => x.Name).ToArray();
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