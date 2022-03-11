using AzerothMemories.WebServer.Database.Records;
using FluentMigrator;

namespace AzerothMemories.Database.Migrations;

[Migration(MigrationId)]
public sealed class Migration0002_BlizzardData : Migration
{
    public const int MigrationId = 2;

    public override void Up()
    {
        Create.Table(BlizzardDataRecord.TableName)
            .WithColumn(nameof(BlizzardDataRecord.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(BlizzardDataRecord.TagType)).AsByte()
            .WithColumn(nameof(BlizzardDataRecord.TagId)).AsInt32()
            .WithColumn(nameof(BlizzardDataRecord.Key)).AsText().Unique().NotNullable()
            .WithColumn(nameof(BlizzardDataRecord.Media)).AsText().Nullable()
            .WithColumn(nameof(BlizzardDataRecord.MinTagTime)).AsDateTimeOffsetWithDefault()
            .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.EnUs)}").AsText().Nullable()
            .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.KoKr)}").AsText().Nullable()
            .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.FrFr)}").AsText().Nullable()
            .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.DeDe)}").AsText().Nullable()
            .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.ZhCn)}").AsText().Nullable()
            .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.EsEs)}").AsText().Nullable()
            .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.ZhTw)}").AsText().Nullable()
            .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.EnGb)}").AsText().Nullable()
            .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.EsMx)}").AsText().Nullable()
            .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.RuRu)}").AsText().Nullable()
            .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.PtBr)}").AsText().Nullable()
            .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.ItIt)}").AsText().Nullable()
            .WithColumn($"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.PtPt)}").AsText().Nullable();

        var indexNames = new[]
        {
            $"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.EnUs)}",
            $"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.KoKr)}",
            $"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.FrFr)}",
            $"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.DeDe)}",
            $"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.ZhCn)}",
            $"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.EsEs)}",
            $"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.ZhTw)}",
            $"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.EnGb)}",
            $"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.EsMx)}",
            $"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.RuRu)}",
            $"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.PtBr)}",
            $"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.ItIt)}",
            $"{nameof(BlizzardDataRecord.Name)}_{nameof(BlizzardDataRecordLocal.PtPt)}"
        };

        foreach (var indexName in indexNames)
        {
            Execute.Sql($"CREATE INDEX IX_Blizzard_Data_{indexName} ON \"{BlizzardDataRecord.TableName}\" (LOWER(\"{indexName}\") varchar_pattern_ops)");
        }
    }

    public override void Down()
    {
        Delete.Table(BlizzardDataRecord.TableName);
    }
}