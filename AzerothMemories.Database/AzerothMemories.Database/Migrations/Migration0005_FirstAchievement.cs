using AzerothMemories.WebServer.Database.Records;
using FluentMigrator;

namespace AzerothMemories.Database.Migrations;

[Migration(MigrationId)]
public sealed class Migration0005_FirstAchievement : Migration
{
    public const int MigrationId = 5;

    public override void Up()
    {
        //const string Id = "Id";

        Create.Table(CharacterFirstAchievementRecord.TableName)
            .WithColumn(nameof(CharacterFirstAchievementRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(CharacterFirstAchievementRecord.AchievementId)).AsInt32().Unique()
            .WithColumn(nameof(CharacterFirstAchievementRecord.AchievementTimeStamp)).AsDateTimeOffsetWithDefault().Indexed();
    }

    public override void Down()
    {
        Delete.Table(CharacterFirstAchievementRecord.TableName);
    }
}