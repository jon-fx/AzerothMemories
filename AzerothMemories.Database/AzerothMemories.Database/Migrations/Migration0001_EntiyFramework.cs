using FluentMigrator;

namespace AzerothMemories.Database.Migrations;

[Migration(MigrationId)]
public sealed class Migration0001_EntiyFramework : Migration
{
    public const int MigrationId = 1;

    public override void Up()
    {
    }

    public override void Down()
    {
        Delete.Table("UserIdentities");
        Delete.Table("Users");

        Delete.Table("_KeyValues");
        Delete.Table("_Operations");
        Delete.Table("_Sessions");
        Delete.Table("__EFMigrationsHistory");
    }
}