using FluentMigrator.Builders.Create.Table;

namespace AzerothMemories.WebServer.Database.Migrations;

public static class MigrationExt
{
    public static ICreateTableWithColumnSyntax WithUpdateJobInfo(this ICreateTableWithColumnSyntax table)
    {
        return table.WithColumn(nameof(IBlizzardGrainUpdateRecord.UpdateJob)).AsString(60).Nullable()
            .WithColumn(nameof(IBlizzardGrainUpdateRecord.UpdateJobQueueTime)).AsDateTimeOffset().Nullable()
            .WithColumn(nameof(IBlizzardGrainUpdateRecord.UpdateJobStartTime)).AsDateTimeOffset().Nullable()
            .WithColumn(nameof(IBlizzardGrainUpdateRecord.UpdateJobEndTime)).AsDateTimeOffset().Nullable()
            .WithColumn(nameof(IBlizzardGrainUpdateRecord.UpdateJobLastResult)).AsInt16().WithDefaultValue(0);
    }
}