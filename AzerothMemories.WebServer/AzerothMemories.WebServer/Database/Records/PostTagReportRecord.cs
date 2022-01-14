﻿namespace AzerothMemories.WebServer.Database.Records;

[Table("Posts_Reports_Tags")]
public sealed class PostTagReportRecord : IDatabaseRecord
{
    [Column(IsPrimaryKey = true, IsIdentity = true)] public long Id { get; set; }

    [Column, NotNull] public long AccountId;
}