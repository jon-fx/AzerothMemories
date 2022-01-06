namespace AzerothMemories.WebServer.Database.Records
{
    [Table("Tags")]
    public sealed class TagRecord : IDatabaseRecord
    {
        [Column(IsPrimaryKey = true, IsIdentity = true)] public long Id { get; set; }

        [Column, NotNull] public string Tag;

        //[Column, NotNull] public long TotalCount;

        [Column, NotNull] public Instant CreatedTime;
    }
}