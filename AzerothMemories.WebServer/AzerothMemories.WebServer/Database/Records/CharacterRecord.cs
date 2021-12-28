﻿namespace AzerothMemories.WebServer.Database.Records;

[Table("Characters")]
public class CharacterRecord : IBlizzardGrainUpdateRecord
{
    [Column(IsPrimaryKey = true, IsIdentity = true)] public long Id { get; set; }

    [Column, NotNull] public string MoaRef;

    [Column, NotNull] public long BlizzardId;

    [Column, NotNull] public BlizzardRegion BlizzardRegionId;

    [Column, Nullable] public string Name;

    [Column, Nullable] public string NameSearchable;

    [Column, NotNull] public DateTimeOffset CreatedDateTime;

    [Column, NotNull] public long AccountId;

    [Column, NotNull] public bool AccountSync;

    [Column, NotNull] public int RealmId;

    [Column, NotNull] public byte Class;

    [Column, NotNull] public byte Race;

    [Column, NotNull] public byte Gender;

    [Column, NotNull] public byte Level;

    [Column, NotNull] public CharacterFaction Faction;

    [Column, Nullable] public string AvatarLink;

    [Column, Nullable] public string UpdateJob { get; set; }

    [Column, Nullable] public DateTimeOffset? UpdateJobQueueTime { get; set; }

    [Column, Nullable] public DateTimeOffset? UpdateJobStartTime { get; set; }

    [Column, Nullable] public DateTimeOffset? UpdateJobEndTime { get; set; }

    [Column, Nullable] public HttpStatusCode UpdateJobLastResult { get; set; }

    [Column, NotNull] public long BlizzardProfileLastModified { get; set; }

    [Column, NotNull] public long BlizzardRendersLastModified { get; set; }

    [Column, NotNull] public long BlizzardAchievementsLastModified { get; set; }
}