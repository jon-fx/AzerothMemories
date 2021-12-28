namespace AzerothMemories.WebServer.Database.Records;

[Table("Characters_Achievements")]
public sealed class CharacterAchievementRecord
{
    [Column(IsPrimaryKey = true, IsIdentity = true)] public long Id { get; set; }

    [Column, NotNull] public long AccountId;

    [Column, NotNull] public long CharacterId;

    [Column, NotNull] public int AchievementId;

    [Column, NotNull] public long AchievementTimeStamp;

    [Column, NotNull] public bool CompletedByCharacter;
}