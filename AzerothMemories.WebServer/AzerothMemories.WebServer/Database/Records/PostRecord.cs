﻿namespace AzerothMemories.WebServer.Database.Records;

[Table("Posts")]
public sealed class PostRecord : IDatabaseRecord
{
    [Column(IsPrimaryKey = true, IsIdentity = true)] public long Id { get; set; }

    [Column, NotNull] public long AccountId;

    [Column, NotNull] public string PostComment;

    [Column, Nullable] public string PostAvatar;

    [Column, Nullable] public byte PostVisibility;

    [Column, NotNull] public PostFlags PostFlags;

    [Column, NotNull] public Instant PostTime;

    [Column, NotNull] public Instant PostEditedTime;

    [Column, NotNull] public Instant PostCreatedTime;

    [Column, NotNull] public string BlobNames;

    [Column, NotNull] public int ReactionCount1;

    [Column, NotNull] public int ReactionCount2;

    [Column, NotNull] public int ReactionCount3;

    [Column, NotNull] public int ReactionCount4;

    [Column, NotNull] public int ReactionCount5;

    [Column, NotNull] public int ReactionCount6;

    [Column, NotNull] public int ReactionCount7;

    [Column, NotNull] public int ReactionCount8;

    [Column, NotNull] public int ReactionCount9;

    [Column, NotNull] public int TotalReactionCount;

    [Column, NotNull] public int TotalCommentCount;

    [Column, NotNull] public int TotalReportCount;

    [Column, NotNull] public long DeletedTimeStamp;
}