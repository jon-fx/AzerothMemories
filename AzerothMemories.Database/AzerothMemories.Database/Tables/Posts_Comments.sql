﻿CREATE TABLE [dbo].[Posts_Comments]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [AccountId] BIGINT NOT NULL DEFAULT 0 ,
    [PostId] BIGINT NOT NULL DEFAULT 0 ,
    [ParentId] BIGINT NOT NULL DEFAULT 0 ,
    [PostComment] NVARCHAR(2048) NULL,
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
    [TotalReportCount] INT NOT NULL DEFAULT 0 ,
    [CreatedTime]  DATETIMEOFFSET NOT NULL DEFAULT '1900-01-01 00:00:00 +00:00',
    [DeletedTimeStamp] BIGINT NOT NULL DEFAULT 0,
)