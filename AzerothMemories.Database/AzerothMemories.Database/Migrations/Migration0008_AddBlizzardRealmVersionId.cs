using AzerothMemories.WebServer.Database.Records;
using FluentMigrator;

namespace AzerothMemories.Database.Migrations;

[Migration(MigrationId)]
public sealed class Migration0008_AddBlizzardRealmVersionId : Migration
{
    public const int MigrationId = 8;

    public override void Up()
    {
        Alter.Table(CharacterRecord.TableName)
            .AddColumn(nameof(CharacterRecord.BlizzardRealmVersionId)).AsByte().NotNullable().WithDefaultValue(byte.MinValue);

        Alter.Table(GuildRecord.TableName)
            .AddColumn(nameof(GuildRecord.BlizzardRealmVersionId)).AsByte().NotNullable().WithDefaultValue(byte.MinValue);
    }

    public override void Down()
    {
    }

    public static void CreateBlizzardRealmRecordTable(Migration migration)
    {
        migration.Create.Table(BlizzardRealmRecord.TableName)
            .WithColumn(nameof(BlizzardRealmRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(BlizzardRealmRecord.RealmId)).AsInt32().Unique()
            .WithColumn(nameof(BlizzardRealmRecord.RealmRegion)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(BlizzardRealmRecord.RealmVersion)).AsByte().WithDefaultValue(0)
            .WithColumn(nameof(BlizzardRealmRecord.RealmSlug)).AsText()
            .WithColumn(nameof(BlizzardRealmRecord.RealmNameEnGb)).AsText()
            .WithColumn(nameof(BlizzardRealmRecord.RealmNameEnUs)).AsText();
    }

    public static void DeleteBlizzardRealmRecordTable(Migration migration)
    {
        migration.Delete.Table(BlizzardRealmRecord.TableName);
    }
}