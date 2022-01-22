namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class AccountViewModel
{
    [JsonInclude] public long Id;

    [JsonInclude] public string Username;

    [JsonInclude] public AccountType AccountType;

    [JsonInclude] public string BattleTag;

    [JsonInclude] public bool BattleTagIsPublic;

    [JsonInclude] public BlizzardRegion RegionId;

    [JsonInclude] public bool IsPrivate;

    [JsonInclude] public string Avatar;

    [JsonInclude] public string AvatarTag;

    [JsonInclude] public long CreatedDateTime;

    [JsonInclude] public string[] SocialLinks;

    [JsonInclude] public int TotalPostCount;

    [JsonInclude] public int TotalMemoriesCount;

    [JsonInclude] public int TotalCommentCount;

    [JsonInclude] public int TotalReactionsCount;

    [JsonInclude] public long LastUpdateJobEndTime;

    [JsonInclude] public string BanReason;

    [JsonInclude] public long BanExpireTime;

    [JsonInclude] public CharacterViewModel[] CharactersArray = Array.Empty<CharacterViewModel>();

    [JsonInclude] public Dictionary<long, AccountFollowingViewModel> FollowingViewModels = new();

    [JsonInclude] public Dictionary<long, AccountFollowingViewModel> FollowersViewModels = new();

    [JsonIgnore] public bool CanInteract => SystemClock.Instance.GetCurrentInstant() > Instant.FromUnixTimeMilliseconds(BanExpireTime);

    [JsonIgnore] public bool CanChangeUsername => true;

    public AccountViewModel()
    {
    }

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

    public Dictionary<long, string> GetUserTagList()
    {
        if (FollowersViewModels == null)
        {
            return new Dictionary<long, string>();
        }

        var tagSet = new Dictionary<long, string>();
        foreach (var kvp in FollowersViewModels)
        {
            tagSet.TryAdd(kvp.Value.FollowerId, kvp.Value.FollowerUsername);
        }

        return tagSet;
    }
}