CREATE TABLE [dbo].[Characters_Achievements]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY,
    [AccountId] BIGINT NOT NULL DEFAULT 0 ,
    [CharacterId] BIGINT NOT NULL DEFAULT 0 ,
    [AchievementId] INT NOT NULL DEFAULT 0 ,
    [AchievementTimeStamp] BIGINT NOT NULL DEFAULT 0,
    [CompletedByCharacter] BIT NOT NULL DEFAULT 0 ,
)

GO CREATE INDEX [IX_CharacterAchievements_AccountId] ON [dbo].[Characters_Achievements] ([AccountId])
GO CREATE INDEX [IX_CharacterAchievements_CharacterId] ON [dbo].[Characters_Achievements] ([CharacterId])
GO CREATE INDEX [IX_CharacterAchievements_AchievementId] ON [dbo].[Characters_Achievements] ([AchievementId])