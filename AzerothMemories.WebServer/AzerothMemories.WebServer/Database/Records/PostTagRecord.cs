namespace AzerothMemories.WebServer.Database.Records
{
    [Table("Posts_Tags")]
    public sealed class PostTagRecord : IDatabaseRecord
    {
        [Column(IsPrimaryKey = true, IsIdentity = true)] public long Id { get; set; }

        [Column, NotNull] public long PostId;

        [Column, NotNull] public long TagId;

        //[Column, NotNull] public int ReportedCount;

        [Column, NotNull] public DateTimeOffset CreatedTime;
    }
}