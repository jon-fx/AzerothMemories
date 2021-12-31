namespace AzerothMemories.WebServer.Database.Records;

[Table("Accounts")]
public class AccountRecord : IBlizzardGrainUpdateRecord
{
    [Column(IsPrimaryKey = true, IsIdentity = true)] public long Id { get; set; }

    [Column, NotNull] public string FusionId { get; set; }

    [Column, NotNull] public DateTimeOffset CreatedDateTime { get; set; }

    [Column, NotNull] public long BlizzardId;

    [Column, NotNull] public BlizzardRegion BlizzardRegionId;

    [Column, Nullable] public string BattleTag;

    [Column, Nullable] public bool BattleTagIsPublic;

    [Column, Nullable] public string BattleNetToken;

    [Column, Nullable] public DateTimeOffset? BattleNetTokenExpiresAt;

    [Column, Nullable] public string Username;

    [Column, Nullable] public string UsernameSearchable;

    [Column, Nullable] public bool IsPrivate;

    [Column, Nullable] public string Avatar;

    [Column, Nullable] public string UpdateJob { get; set; }

    [Column, Nullable] public DateTimeOffset? UpdateJobQueueTime { get; set; }

    [Column, Nullable] public DateTimeOffset? UpdateJobStartTime { get; set; }

    [Column, Nullable] public DateTimeOffset? UpdateJobEndTime { get; set; }

    [Column, NotNull] public HttpStatusCode UpdateJobLastResult { get; set; }
}