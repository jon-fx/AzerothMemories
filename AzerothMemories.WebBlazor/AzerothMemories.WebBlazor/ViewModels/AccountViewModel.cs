namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class AccountViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public int Id;

    [JsonInclude, DataMember, MemoryPackInclude] public string Username;

    [JsonInclude, DataMember, MemoryPackInclude] public long NextUsernameChangedTime;

    [JsonInclude, DataMember, MemoryPackInclude] public AccountType AccountType;

    [JsonInclude, DataMember, MemoryPackInclude] public AccountFlags AccountFlags;

    [JsonInclude, DataMember, MemoryPackInclude] public string BattleTag;

    [JsonInclude, DataMember, MemoryPackInclude] public bool BattleTagIsPublic;

    [JsonInclude, DataMember, MemoryPackInclude] public bool IsPrivate;

    [JsonInclude, DataMember, MemoryPackInclude] public string Avatar;

    [JsonInclude, DataMember, MemoryPackInclude] public long CreatedDateTime;

    [JsonInclude, DataMember, MemoryPackInclude] public string[] SocialLinks;

    [JsonInclude, DataMember, MemoryPackInclude] public int TotalPostCount;

    [JsonInclude, DataMember, MemoryPackInclude] public int TotalMemoriesCount;

    [JsonInclude, DataMember, MemoryPackInclude] public int TotalCommentCount;

    [JsonInclude, DataMember, MemoryPackInclude] public int TotalReactionsCount;

    [JsonInclude, DataMember, MemoryPackInclude] public BlizzardUpdateViewModel UpdateJobLastResults;

    [JsonInclude, DataMember, MemoryPackInclude] public string BanReason;

    [JsonInclude, DataMember, MemoryPackInclude] public long BanExpireTime;

    [JsonInclude, DataMember, MemoryPackInclude] public CharacterViewModel[] CharactersArray = Array.Empty<CharacterViewModel>();

    [JsonInclude, DataMember, MemoryPackInclude] public Dictionary<int, AccountFollowingViewModel> FollowingViewModels = new();

    [JsonInclude, DataMember, MemoryPackInclude] public Dictionary<int, AccountFollowingViewModel> FollowersViewModels = new();

    [JsonInclude, DataMember, MemoryPackInclude] public AccountViewModelLinks[] LinkedLogins { get; set; }

    [JsonIgnore, IgnoreDataMember, MemoryPackIgnore] public bool CanInteract => SystemClock.Instance.GetCurrentInstant() > Instant.FromUnixTimeMilliseconds(BanExpireTime);

    [JsonIgnore, IgnoreDataMember, MemoryPackIgnore] public bool CanChangeUsername => Username.Contains('-') || SystemClock.Instance.GetCurrentInstant() > Instant.FromUnixTimeMilliseconds(NextUsernameChangedTime);

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

        return Avatar.StartsWith(ZExtensions.CustomUserAvatarPathPrefix);
    }
}