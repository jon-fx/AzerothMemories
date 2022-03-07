namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class AccountViewModel
{
    [JsonInclude] public int Id;

    [JsonInclude] public string Username;

    [JsonInclude] public long NextUsernameChangedTime;

    [JsonInclude] public AccountType AccountType;

    [JsonInclude] public AccountFlags AccountFlags;

    [JsonInclude] public string BattleTag;

    [JsonInclude] public bool BattleTagIsPublic;

    [JsonInclude] public BlizzardRegion RegionId;

    [JsonInclude] public bool IsPrivate;

    [JsonInclude] public string Avatar;

    [JsonInclude] public long CreatedDateTime;

    [JsonInclude] public string[] SocialLinks;

    [JsonInclude] public int TotalPostCount;

    [JsonInclude] public int TotalMemoriesCount;

    [JsonInclude] public int TotalCommentCount;

    [JsonInclude] public int TotalReactionsCount;

    [JsonInclude] public long UpdateJobLastEndTime;

    [JsonInclude] public HttpStatusCode UpdateJobLastResult;

    [JsonInclude] public string BanReason;

    [JsonInclude] public long BanExpireTime;

    [JsonInclude] public CharacterViewModel[] CharactersArray = Array.Empty<CharacterViewModel>();

    [JsonInclude] public Dictionary<int, AccountFollowingViewModel> FollowingViewModels = new();

    [JsonInclude] public Dictionary<int, AccountFollowingViewModel> FollowersViewModels = new();

    [JsonIgnore] public bool CanInteract => SystemClock.Instance.GetCurrentInstant() > Instant.FromUnixTimeMilliseconds(BanExpireTime);

    [JsonIgnore] public bool CanChangeUsername => Username.Contains('-') || SystemClock.Instance.GetCurrentInstant() > Instant.FromUnixTimeMilliseconds(NextUsernameChangedTime);

    public AccountViewModel()
    {
    }

    public string GetDisplayName()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            return "Unknown";
        }

        return Username;
    }

    public string GetPageTitle()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            return "Memories of Azeroth";
        }

        return $"{Username}'s Memories of Azeroth";
    }

    public string GetAvatarText()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            return "?";
        }

        return Username[0].ToString();
    }

    public CharacterViewModel[] GetCharactersSafe()
    {
        if (CharactersArray == null || CharactersArray.Length == 0)
        {
            return Array.Empty<CharacterViewModel>();
        }

        return CharactersArray.Where(x => x.CharacterStatus == CharacterStatus2.None).OrderByDescending(x => x.Level).ThenBy(x => x.Name).ToArray();
    }

    public CharacterViewModel[] GetAllCharactersSafe()
    {
        if (CharactersArray == null || CharactersArray.Length == 0)
        {
            return Array.Empty<CharacterViewModel>();
        }

        return CharactersArray.OrderByDescending(x => x.Level).ThenBy(x => x.Name).ToArray();
    }

    public Dictionary<int, string> GetUserTagList()
    {
        if (FollowersViewModels == null)
        {
            return new Dictionary<int, string>();
        }

        var tagSet = new Dictionary<int, string>();
        foreach (var kvp in FollowersViewModels)
        {
            tagSet.TryAdd(kvp.Value.FollowerId, kvp.Value.FollowerUsername);
        }

        return tagSet;
    }

    public int GetUploadQuality()
    {
        var result = 80;
        if (AccountType >= AccountType.Tier1)
        {
        }

        if (AccountType >= AccountType.Tier2)
        {
            result = 85;
        }

        if (AccountType >= AccountType.Tier3)
        {
            result = 90;
        }

        return result;
    }

    public bool IsCustomAvatar()
    {
        if (string.IsNullOrWhiteSpace(Avatar))
        {
            return false;
        }

        return Avatar.StartsWith(ZExtensions.CustomAvatarPathPrefix);
    }
}