using LinqToDB.Mapping;
using System.Net;

namespace AzerothMemories.WebServer.Database.Records
{
    [Table("Accounts")]
    public sealed class AccountGrainRecord : IBlizzardGrainUpdateRecord
    {
        [Column(IsPrimaryKey = true, IsIdentity = true)] public long Id { get; set; }

        [Column, NotNull] public string MoaRef { get; set; }

        [Column, NotNull] public AccountType AccountType;

        [Column, NotNull] public long BlizzardId;

        [Column, NotNull] public BlizzardRegion RegionId;

        [Column, NotNull] public string Username;

        [Column, NotNull] public DateTimeOffset UsernameChangeTime;

        [Column, Nullable] public string BattleTag;

        [Column, Nullable] public string AccessToken;

        [Column, Nullable] public long AccessTokenExpiresAt;

        //[Column, Nullable] public string Avatar;

        //[Column, Nullable] public bool IsPrivate;

        [Column, Nullable] public DateTimeOffset CreatedDateTime;

        [Column, Nullable] public DateTimeOffset LastLoginDateTime;

        [Column, NotNull] public DateTimeOffset LastUpdateEndTime { get; set; }

        [Column, NotNull] public RequestResultCode LastUpdateResult { get; set; }

        [Column, NotNull] public HttpStatusCode LastUpdateHttpResult { get; set; }

        //[Column, Nullable] public string BanReason;

        //[Column, NotNull] public DateTimeOffset BanExpireTime;

        //[Column, NotNull] public long BlizzardAccountLastModified;

        //[Column, NotNull] public bool PublicBattleTag;

        //[Column, Nullable] public string SocialDiscord;

        //[Column, Nullable] public string SocialTwitter;

        //[Column, Nullable] public string SocialTwitch;

        //[Column, Nullable] public string SocialYouTube;
    }
}