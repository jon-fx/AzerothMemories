using System.Diagnostics.CodeAnalysis;

namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class AccountViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public int Id { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public string Username { get; set; } = null!;

    [JsonInclude, DataMember, MemoryPackInclude] public long NextUsernameChangedTime { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public AccountType AccountType { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public AccountFlags AccountFlags { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public string? BattleTag { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public bool BattleTagIsPublic { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public bool IsPrivate { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public string? Avatar { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public long CreatedDateTime { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public string?[] SocialLinks { get; init; } = [];

    [JsonInclude, DataMember, MemoryPackInclude] public int TotalPostCount { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public int TotalMemoriesCount { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public int TotalCommentCount { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public int TotalReactionsCount { get; set; }

    [JsonInclude, DataMember, MemoryPackInclude] public BlizzardUpdateViewModel? UpdateJobLastResults { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public string? BanReason { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public long BanExpireTime { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public CharacterViewModel[]? CharactersArray { get; set; } = [];

    [JsonInclude, DataMember, MemoryPackInclude] public Dictionary<int, AccountFollowingViewModel> FollowingViewModels { get; init; } = new();

    [JsonInclude, DataMember, MemoryPackInclude] public Dictionary<int, AccountFollowingViewModel> FollowersViewModels { get; init; } = new();

    [JsonInclude, DataMember, MemoryPackInclude] public AccountViewModelLinks[] LinkedLogins { get; set; } = [];

    [JsonIgnore, IgnoreDataMember, MemoryPackIgnore] public bool CanInteract => SystemClock.Instance.GetCurrentInstant() > Instant.FromUnixTimeMilliseconds(BanExpireTime);

    [JsonIgnore, IgnoreDataMember, MemoryPackIgnore] public bool CanChangeUsername => Username.Contains('-') || SystemClock.Instance.GetCurrentInstant() > Instant.FromUnixTimeMilliseconds(NextUsernameChangedTime);

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

    [MemberNotNullWhen(true, nameof(Avatar))]
    public bool IsCustomAvatar()
    {
        if (string.IsNullOrWhiteSpace(Avatar))
        {
            return false;
        }

        return Avatar.StartsWith(ZExtensions.CustomUserAvatarPathPrefix);
    }
}