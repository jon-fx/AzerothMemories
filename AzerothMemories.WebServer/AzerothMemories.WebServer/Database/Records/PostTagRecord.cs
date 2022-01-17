namespace AzerothMemories.WebServer.Database.Records;

[Table("Posts_Tags")]
public sealed class PostTagRecord : IDatabaseRecord
{
    [Column(IsPrimaryKey = true, IsIdentity = true)] public long Id { get; set; }

    [Column, NotNull] public PostTagKind TagKind;

    [Column, NotNull] public PostTagType TagType;

    [Column, NotNull] public long PostId;

    [Column, NotNull] public long? CommentId;

    [Column, NotNull] public long TagId;

    [Column, NotNull] public string TagString;

    [Column, NotNull] public int TotalReportCount;

    [Column, NotNull] public Instant CreatedTime;

    public static bool ValidateTagCounts(HashSet<PostTagRecord> tagRecords)
    {
        var array = new int[CommonConfig.TagCountsPerPost.Length];

        foreach (var tagRecord in tagRecords)
        {
            var id = (int)tagRecord.TagType;
            if (id > array.Length)
            {
                continue;
            }

            array[id]++;
        }

        for (var i = 0; i < array.Length; i++)
        {
            var count = array[i];
            var minMax = CommonConfig.TagCountsPerPost[i];
            if (count >= minMax.Min && count <= minMax.Max)
            {
            }
            else
            {
                return false;
            }
        }

        return true;
    }
}