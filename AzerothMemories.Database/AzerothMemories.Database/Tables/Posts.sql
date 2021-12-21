CREATE TABLE [dbo].[Posts]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY,
    [AccountId] BIGINT NOT NULL DEFAULT 0 ,
    [PostAvatar] NVARCHAR(200) NULL,
    [PostFlags] INT NOT NULL DEFAULT 0 ,
    [PostVisibility] TINYINT NOT NULL DEFAULT 0,
    [PostComment] NVARCHAR(2048) NULL,
    [PostTime] DATETIMEOFFSET NOT NULL DEFAULT '1900-01-01 00:00:00 +00:00',
    [PostEditedTime] DATETIMEOFFSET NOT NULL DEFAULT '1900-01-01 00:00:00 +00:00',
    [PostCreatedTime] DATETIMEOFFSET NOT NULL DEFAULT '1900-01-01 00:00:00 +00:00',
    [BlobNames] NVARCHAR(256) NULL,
    [SystemTags] NVARCHAR(256) NULL,
    [ReactionCount1] INT NOT NULL DEFAULT 0 ,
    [ReactionCount2] INT NOT NULL DEFAULT 0 ,
    [ReactionCount3] INT NOT NULL DEFAULT 0 ,
    [ReactionCount4] INT NOT NULL DEFAULT 0 ,
    [ReactionCount5] INT NOT NULL DEFAULT 0 ,
    [ReactionCount6] INT NOT NULL DEFAULT 0 ,
    [ReactionCount7] INT NOT NULL DEFAULT 0 ,
    [ReactionCount8] INT NOT NULL DEFAULT 0 ,
    [ReactionCount9] INT NOT NULL DEFAULT 0 ,
    [TotalReactionCount] INT NOT NULL DEFAULT 0 ,
    [TotalCommentCount] INT NOT NULL DEFAULT 0 ,
    [TotalReportCount] INT NOT NULL DEFAULT 0 ,
    [DeletedTimeStamp] BIGINT NOT NULL DEFAULT 0,
)

GO CREATE INDEX [IX_Posts_AccountId] ON [dbo].[Posts] ([AccountId])