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

    //[Column, Nullable] public Instant? UpdateJobQueueTime { get; set; }

    //[Column, Nullable] public Instant? UpdateJobStartTime { get; set; }

    [Column, Nullable] public Instant UpdateJobEndTime { get; set; }

    [Column, NotNull] public HttpStatusCode UpdateJobLastResult { get; set; }
}