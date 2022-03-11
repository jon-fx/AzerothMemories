using AzerothMemories.WebServer.Database.Records;
using FluentMigrator.Builders.Create.Table;

namespace AzerothMemories.Database.Migrations;

public static class MigrationExt
{
    public static ICreateTableColumnOptionOrWithColumnSyntax AsText(this ICreateTableColumnAsTypeSyntax createTableColumnAsTypeSyntax)
    {
        return createTableColumnAsTypeSyntax.AsCustom("Text");
    }

    public static ICreateTableColumnOptionOrWithColumnSyntax AsDateTimeOffsetWithDefault(this ICreateTableColumnAsTypeSyntax createTableColumnAsTypeSyntax)
    {
        return createTableColumnAsTypeSyntax.AsDateTimeOffset().NotNullable().WithDefaultValue(DateTimeOffset.UnixEpoch);
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