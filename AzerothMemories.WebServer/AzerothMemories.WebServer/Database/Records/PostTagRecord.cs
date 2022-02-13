using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class PostTagRecord : IDatabaseRecord
{
    public const string TableName = "Posts_Tags";

    [Key] public long Id { get; set; }

    [Column] public PostTagKind TagKind { get; set; }

    [Column] public PostTagType TagType { get; set; }

    [Column] public long PostId { get; set; }

    [Column] public PostRecord Post { get; set; }

    [Column] public long? CommentId { get; set; }

    [Column] public PostCommentRecord Comment { get; set; }

    [Column] public long TagId { get; set; }

    [Column] public string TagString { get; set; }

    [Column] public int TotalReportCount { get; set; }

    [Column] public Instant CreatedTime { get; set; }

    public static bool ValidateTagCounts(HashSet<PostTagRecord> tagRecords)
    {
        var array = new int[ZExtensions.TagCountsPerPost.Length];

        foreach (var tagRecord in tagRecords)
        {
            var id = (int)tagRecord.TagType;
            if (id > array.Length)
            {
                continue;
            }

            if (tagRecord.TagKind == PostTagKind.UserComment || tagRecord.TagKind == PostTagKind.Deleted)
            {
                continue;
            }

            array[id]++;
        }

        for (var i = 0; i < array.Length; i++)
        {
            var count = array[i];
            var minMax = ZExtensions.TagCountsPerPost[i];
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