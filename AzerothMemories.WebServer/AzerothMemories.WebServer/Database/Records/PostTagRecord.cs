namespace AzerothMemories.WebServer.Database.Records
{
    [Table("Posts_Tags")]
    public sealed class PostTagRecord : IDatabaseRecord
    {
        [Column(IsPrimaryKey = true, IsIdentity = true)] public long Id { get; set; }

        [Column, NotNull] public PostTagKind TagKind;

        [Column, NotNull] public PostTagType TagType;

        [Column, NotNull] public long PostId;

        [Column, NotNull] public long TagId;

        [Column, NotNull] public string TagString;

        //[Column, NotNull] public int ReportedCount;

        [Column, NotNull] public Instant CreatedTime;
    }
}