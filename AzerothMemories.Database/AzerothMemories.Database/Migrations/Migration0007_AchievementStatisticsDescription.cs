using AzerothMemories.WebServer.Database.Records;
using FluentMigrator;

namespace AzerothMemories.Database.Migrations;

[Migration(MigrationId)]
public sealed class Migration0007_AchievementStatisticsDescription : Migration
{
    public const int MigrationId = 7;

    public override void Up()
    {
        Alter.Table(CharacterAchievementStatisticRecord.TableName)
            .AddColumn($"{nameof(CharacterAchievementStatisticRecord.StatisticDescription)}_{nameof(BlizzardDataRecordLocal.EnUs)}").AsText().Nullable()
            .AddColumn($"{nameof(CharacterAchievementStatisticRecord.StatisticDescription)}_{nameof(BlizzardDataRecordLocal.KoKr)}").AsText().Nullable()
            .AddColumn($"{nameof(CharacterAchievementStatisticRecord.StatisticDescription)}_{nameof(BlizzardDataRecordLocal.FrFr)}").AsText().Nullable()
            .AddColumn($"{nameof(CharacterAchievementStatisticRecord.StatisticDescription)}_{nameof(BlizzardDataRecordLocal.DeDe)}").AsText().Nullable()
            .AddColumn($"{nameof(CharacterAchievementStatisticRecord.StatisticDescription)}_{nameof(BlizzardDataRecordLocal.ZhCn)}").AsText().Nullable()
            .AddColumn($"{nameof(CharacterAchievementStatisticRecord.StatisticDescription)}_{nameof(BlizzardDataRecordLocal.EsEs)}").AsText().Nullable()
            .AddColumn($"{nameof(CharacterAchievementStatisticRecord.StatisticDescription)}_{nameof(BlizzardDataRecordLocal.ZhTw)}").AsText().Nullable()
            .AddColumn($"{nameof(CharacterAchievementStatisticRecord.StatisticDescription)}_{nameof(BlizzardDataRecordLocal.EnGb)}").AsText().Nullable()
            .AddColumn($"{nameof(CharacterAchievementStatisticRecord.StatisticDescription)}_{nameof(BlizzardDataRecordLocal.EsMx)}").AsText().Nullable()
            .AddColumn($"{nameof(CharacterAchievementStatisticRecord.StatisticDescription)}_{nameof(BlizzardDataRecordLocal.RuRu)}").AsText().Nullable()
            .AddColumn($"{nameof(CharacterAchievementStatisticRecord.StatisticDescription)}_{nameof(BlizzardDataRecordLocal.PtBr)}").AsText().Nullable()
            .AddColumn($"{nameof(CharacterAchievementStatisticRecord.StatisticDescription)}_{nameof(BlizzardDataRecordLocal.ItIt)}").AsText().Nullable()
            .AddColumn($"{nameof(CharacterAchievementStatisticRecord.StatisticDescription)}_{nameof(BlizzardDataRecordLocal.PtPt)}").AsText().Nullable();
    }

    public override void Down()
    {
    }
}