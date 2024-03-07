using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class PostReactionRecord : IDatabaseRecordWithVersion
{
    public const string TableName = "Posts_Reactions";

    [Key] public int Id { get; init; }

    [Column] public int AccountId { get; init; }

    [Column] public int PostId { get; init; }

    [Column] public PostReaction Reaction { get; set; }

    [Column] public Instant LastUpdateTime { get; set; }

    public uint RowVersion { get; set; }
}