namespace AzerothMemories.WebServer.Database.Records;

[Table("Accounts")]
public sealed class AccountRecord : IBlizzardUpdateRecord
{
    [Column(IsPrimaryKey = true, IsIdentity = true)] public long Id { get; set; }

    [Column, NotNull] public string FusionId { get; set; }

    [Column, NotNull] public AccountType AccountType;

    [Column, NotNull] public Instant CreatedDateTime;

    [Column, NotNull] public long BlizzardId;

    [Column, NotNull] public BlizzardRegion BlizzardRegionId;

    [Column, Nullable] public string BattleTag;

    [Column, Nullable] public bool BattleTagIsPublic;

    [Column, Nullable] public string BattleNetToken;

    [Column, Nullable] public Instant? BattleNetTokenExpiresAt;

    [Column, Nullable] public string Username;

    [Column, Nullable] public string UsernameSearchable;

    [Column, Nullable] public bool IsPrivate;

    [Column, Nullable] public string Avatar;

    [Column, Nullable] public string SocialDiscord;

    [Column, Nullable] public string SocialTwitter;

    [Column, Nullable] public string SocialTwitch;

    [Column, Nullable] public string SocialYouTube;

    [Column, Nullable] public string UpdateJob { get; set; }

    [Column, Nullable] public Instant UpdateJobEndTime { get; set; }

    [Column, NotNull] public HttpStatusCode UpdateJobLastResult { get; set; }

    public AccountViewModel CreateViewModel(bool activeOrAdmin, Dictionary<long, AccountFollowingViewModel> followingViewModels, Dictionary<long, AccountFollowingViewModel> followersViewModels)
    {
        var viewModel = new AccountViewModel
        {
            Id = Id,
            Avatar = Avatar,
            Username = Username,
            AccountType = AccountType,
            RegionId = BlizzardRegionId,
            BattleTag = BattleTag,
            BattleTagIsPublic = BattleTagIsPublic,
            CreatedDateTime = CreatedDateTime.ToUnixTimeMilliseconds(),
            IsPrivate = IsPrivate,
            SocialLinks = new[]
            {
               SocialDiscord,
               SocialTwitter,
               SocialTwitch,
               SocialYouTube,
            },
            FollowingViewModels = RemoveNoneStatus(followingViewModels),
            FollowersViewModels = RemoveNoneStatus(followersViewModels),
        };

        if (viewModel.BattleTagIsPublic || activeOrAdmin)
        {
        }
        else
        {
            viewModel.BattleTag = null;
        }

        return viewModel;
    }

    private static Dictionary<long, AccountFollowingViewModel> RemoveNoneStatus(Dictionary<long, AccountFollowingViewModel> viewModels)
    {
        var results = new Dictionary<long, AccountFollowingViewModel>();
        foreach (var kvp in viewModels)
        {
            if (kvp.Value.Status == AccountFollowingStatus.None)
            {
                continue;
            }

            results.Add(kvp.Key, kvp.Value);
        }

        return results;
    }
}