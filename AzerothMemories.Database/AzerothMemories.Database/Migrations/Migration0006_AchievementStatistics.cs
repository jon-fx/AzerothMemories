using AzerothMemories.WebServer.Database.Records;
using FluentMigrator;
using System.Data;

namespace AzerothMemories.Database.Migrations;

[Migration(MigrationId)]
public sealed class Migration0006_AchievementStatistics : Migration
{
    public const int MigrationId = 6;

    public override void Up()
    {
        const string Id = "Id";

        Create.Table(CharacterAchievementStatisticRecord.TableName)
            .WithColumn(nameof(CharacterAchievementStatisticRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(CharacterAchievementStatisticRecord.AccountId)).AsInt32().ForeignKey(AccountRecord.TableName, Id).OnDelete(Rule.SetNull).Nullable().Indexed()
            .WithColumn(nameof(CharacterAchievementStatisticRecord.CharacterId)).AsInt32().ForeignKey(CharacterRecord.TableName, Id).OnDelete(Rule.Cascade).Indexed()
            .WithColumn(nameof(CharacterAchievementStatisticRecord.StatisticId)).AsInt32().WithDefaultValue(0).Indexed()
            .WithColumn(nameof(CharacterAchievementStatisticRecord.StatisticQuantity)).AsFloat().WithDefaultValue(0).Indexed()
            .WithColumn(nameof(CharacterAchievementStatisticRecord.StatisticTimeStamp)).AsDateTimeOffsetWithDefault().Indexed();
    }

    public override void Down()
    {
        Delete.Table(CharacterAchievementStatisticRecord.TableName);
    }
}