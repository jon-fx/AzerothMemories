﻿namespace AzerothMemories.WebBlazor.ViewModels;

public class AccountViewModel
{
    [JsonInclude] public long Id;

    [JsonInclude] public string Username;

    [JsonInclude] public AccountType AccountType;

    [JsonInclude] public string BattleTag;

    [JsonInclude] public bool BattleTagIsPublic;

    [JsonInclude] public BlizzardRegion RegionId;

    [JsonInclude] public bool IsPrivate;

    [JsonInclude] public string Avatar;

    [JsonInclude] public long CreatedDateTime;

    [JsonInclude] public string[] SocialLinks;

    [JsonInclude] public CharacterViewModel[] CharactersArray = Array.Empty<CharacterViewModel>();

    [JsonInclude] public Dictionary<long, AccountFollowingViewModel> FollowingViewModels = new();

    [JsonInclude] public Dictionary<long, AccountFollowingViewModel> FollowersViewModels = new();

    public string GetDisplayName()
    {
        if (!string.IsNullOrWhiteSpace(Username))
        {
            return Username;
        }

        if (!string.IsNullOrWhiteSpace(BattleTag))
        {
            return BattleTag;
        }

        return "Unknown";
    }

    public string GetAvatarText()
    {
        if (!string.IsNullOrWhiteSpace(Username))
        {
            return Username[0].ToString();
        }

        if (!string.IsNullOrWhiteSpace(BattleTag))
        {
            return BattleTag[0].ToString();
        }

        return "?";
    }

    public CharacterViewModel[] GetCharactersSafe()
    {
        if (CharactersArray == null || CharactersArray.Length == 0)
        {
            return Array.Empty<CharacterViewModel>();
        }

        return CharactersArray.OrderByDescending(x => x.Level).ThenBy(x => x.Name).ToArray();
    }
}