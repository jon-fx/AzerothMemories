using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class AccountRecord : IBlizzardUpdateRecord
{
    public const string TableName = "Accounts";

    [Key] public long Id { get; set; }

    [Column] public string FusionId { get; set; }

    [Column] public AccountType AccountType { get; set; }

    [Column] public AccountFlags AccountFlags { get; set; }

    [Column] public Instant CreatedDateTime { get; set; }

    [Column] public long BlizzardId { get; set; }

    [Column] public BlizzardRegion BlizzardRegionId { get; set; }

    [Column] public string BattleTag { get; set; }

    [Column] public bool BattleTagIsPublic { get; set; }

    [Column] public string BattleNetToken { get; set; }

    [Column] public Instant? BattleNetTokenExpiresAt { get; set; }

    [Column] public string Username { get; set; }

    [Column] public string UsernameSearchable { get; set; }

    [Column] public Instant UsernameChangedTime { get; set; }

    [Column] public bool IsPrivate { get; set; }

    [Column] public string Avatar { get; set; }

    [Column] public string SocialDiscord { get; set; }

    [Column] public string SocialTwitter { get; set; }

    [Column] public string SocialTwitch { get; set; }

    [Column] public string SocialYouTube { get; set; }

    [Column] public string BanReason { get; set; }

    [Column] public Instant BanExpireTime { get; set; }

    [Column] public string UpdateJob { get; set; }

    [Column] public Instant UpdateJobEndTime { get; set; }

    [Column] public HttpStatusCode UpdateJobLastResult { get; set; }

    //public ICollection<CharacterRecord> Characters { get; set; }

    public AccountViewModel CreateViewModel(CommonServices commonServices, bool activeOrAdmin, Dictionary<long, AccountFollowingViewModel> followingViewModels, Dictionary<long, AccountFollowingViewModel> followersViewModels)
    {
        var viewModel = new AccountViewModel
        {
            Id = Id,
            Avatar = Avatar,
            AccountFlags = AccountFlags,
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
            BanReason = BanReason,
            BanExpireTime = BanExpireTime.ToUnixTimeMilliseconds(),
            FollowingViewModels = RemoveNoneStatus(followingViewModels),
            FollowersViewModels = RemoveNoneStatus(followersViewModels),
            LastUpdateJobEndTime = UpdateJobEndTime.ToUnixTimeMilliseconds(),
        };

        if (viewModel.BattleTagIsPublic || activeOrAdmin)
        {
        }
        else
        {
            viewModel.BattleTag = null;
        }

        if (activeOrAdmin)
        {
            viewModel.NextUsernameChangedTime = (UsernameChangedTime + commonServices.Config.UsernameChangeDelay).ToUnixTimeMilliseconds();
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