using System.Data;
using AzerothMemories.WebServer.Database.Records;
using FluentMigrator;

namespace AzerothMemories.Database.Migrations;

[Migration(MigrationId)]
public sealed class Migration0004_MountData : Migration
{
    public const int MigrationId = 4;

    public override void Up()
    {
        const string Id = "Id";

        Create.Table(CharacterMountRecord.TableName)
            .WithColumn(nameof(CharacterMountRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(CharacterMountRecord.AccountId)).AsInt32().ForeignKey(AccountRecord.TableName, Id).OnDelete(Rule.SetNull).Nullable().Indexed()
            .WithColumn(nameof(CharacterMountRecord.CharacterId)).AsInt32().ForeignKey(CharacterRecord.TableName, Id).OnDelete(Rule.Cascade).Indexed()
            .WithColumn(nameof(CharacterMountRecord.MountId)).AsInt32().WithDefaultValue(0).Indexed()
            .WithColumn(nameof(CharacterMountRecord.MountTimeStamp)).AsDateTimeOffsetWithDefault().Indexed();
    }

    public override void Down()
    {
        Delete.Table(CharacterMountRecord.TableName);
    }
}