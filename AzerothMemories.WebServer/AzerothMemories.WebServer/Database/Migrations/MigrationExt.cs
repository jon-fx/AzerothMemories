using FluentMigrator.Builders.Create.Table;

namespace AzerothMemories.WebServer.Database.Migrations;

public static class MigrationExt
{
    public static ICreateTableWithColumnSyntax WithUpdateJobInfo(this ICreateTableWithColumnSyntax table)
    {
        return table
            .WithColumn(nameof(IBlizzardGrainUpdateRecord.UpdateJob)).AsString(60).Nullable()
            .WithColumn(nameof(IBlizzardGrainUpdateRecord.UpdateJobQueueTime)).AsDateTimeOffset().Nullable()
            .WithColumn(nameof(IBlizzardGrainUpdateRecord.UpdateJobStartTime)).AsDateTimeOffset().Nullable()
            .WithColumn(nameof(IBlizzardGrainUpdateRecord.UpdateJobEndTime)).AsDateTimeOffset().Nullable()
            .WithColumn(nameof(IBlizzardGrainUpdateRecord.UpdateJobLastResult)).AsInt16().WithDefaultValue(0);
    }

    public static ICreateTableWithColumnSyntax WithReactionInfo(this ICreateTableWithColumnSyntax table)
    {
        return table
            .WithColumn(nameof(PostRecord.ReactionCount1)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(PostRecord.ReactionCount2)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(PostRecord.ReactionCount3)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(PostRecord.ReactionCount4)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(PostRecord.ReactionCount5)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(PostRecord.ReactionCount6)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(PostRecord.ReactionCount7)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(PostRecord.ReactionCount8)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(PostRecord.ReactionCount9)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(PostRecord.TotalReactionCount)).AsInt32().WithDefaultValue(0);
    }
}