using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class AccountRecord : IBlizzardUpdateRecord, IDatabaseRecordWithVersion
{
    public const string TableName = "Accounts";

    [Key] public int Id { get; set; }

    [Column] public string FusionId { get; set; }

    [Column] public AccountType AccountType { get; set; }

    [Column] public AccountFlags AccountFlags { get; set; }

    [Column] public Instant CreatedDateTime { get; set; }

    [Column] public long BlizzardId { get; set; }

    [Column] public string BattleTag { get; set; }

    [Column] public bool BattleTagIsPublic { get; set; }

    [Column] public string Username { get; set; }

    [Column] public string UsernameSearchable { get; set; }

    [Column] public Instant UsernameChangedTime { get; set; }

    [Column] public Instant LastLoginTime { get; set; }

    [Column] public int LoginConsecutiveDaysCount { get; set; }

    [Column] public bool IsPrivate { get; set; }

    [Column] public string Avatar { get; set; }

    [Column] public string SocialDiscord { get; set; }

    [Column] public string SocialTwitter { get; set; }

    [Column] public string SocialTwitch { get; set; }

    [Column] public string SocialYouTube { get; set; }

    [Column] public string BanReason { get; set; }

    [Column] public Instant BanExpireTime { get; set; }

    public BlizzardUpdateRecord UpdateRecord { get; set; }

    public ICollection<AuthTokenRecord> AuthTokens { get; set; } = new List<AuthTokenRecord>();

    public uint RowVersion { get; set; }

    public AccountViewModel CreateViewModel(CommonServices commonServices, bool activeOrAdmin, Dictionary<int, AccountFollowingViewModel> followingViewModels, Dictionary<int, AccountFollowingViewModel> followersViewModels)
    {

        var viewModel = new AccountViewModel
        {
            Id = Id,
            Avatar = Avatar,
            AccountFlags = AccountFlags,
            Username = Username,
            AccountType = AccountType,
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
            LinkedLogins = GetLinkedLogins(activeOrAdmin),
            BanReason = BanReason,
            BanExpireTime = BanExpireTime.ToUnixTimeMilliseconds(),
            FollowingViewModels = RemoveNoneStatus(followingViewModels),
            FollowersViewModels = RemoveNoneStatus(followersViewModels),

            UpdateJobLastResults = UpdateRecord?.GetUpdateJobResults(),
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

    private AccountViewModelLinks[] GetLinkedLogins(bool activeOrAdmin)
    {
        var results = new List<AccountViewModelLinks>();
        foreach (var authToken in AuthTokens)
        {
            if (authToken.IsPatreon && activeOrAdmin)
            {
                results.Add(new AccountViewModelLinks { Id = authToken.Id, Name = authToken.Name, Key = authToken.Key});
            }
            
        }
        return results.ToArray();
    }

    private static Dictionary<int, AccountFollowingViewModel> RemoveNoneStatus(Dictionary<int, AccountFollowingViewModel> viewModels)
    {
        var results = new Dictionary<int, AccountFollowingViewModel>();
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

    public bool ShouldUpdateLoginConsecutiveDays()
    {
        var lastSeen = LastLoginTime.InUtc();
        var timeNow = SystemClock.Instance.GetCurrentInstant().InUtc();
        if (timeNow - lastSeen < Duration.FromMinutes(10))
        {
            return timeNow.DayOfYear != lastSeen.DayOfYear;
        }

        return true;
    }

    public void TryUpdateLoginConsecutiveDaysCount()
    {
        if (!ShouldUpdateLoginConsecutiveDays())
        {
            return;
        }

        var lastSeen = LastLoginTime.InUtc();
        var timeNow = SystemClock.Instance.GetCurrentInstant().InUtc();
        if (timeNow.Year == lastSeen.Year)
        {
            if (timeNow.DayOfYear == lastSeen.DayOfYear)
            {
            }
            else if (timeNow.DayOfYear == lastSeen.DayOfYear + 1)
            {
                LoginConsecutiveDaysCount++;
            }
            else
            {
                LoginConsecutiveDaysCount = 1;
            }
        }
        else if (timeNow.Year == lastSeen.Year + 1 && timeNow.DayOfYear == 1 && lastSeen.Month == 12 && lastSeen.Day == 31)
        {
            LoginConsecutiveDaysCount++;
        }
        else
        {
            LoginConsecutiveDaysCount = 1;
        }

        LastLoginTime = timeNow.ToInstant();
    }
}