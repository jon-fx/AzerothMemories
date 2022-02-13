using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class PostReactionRecord : IDatabaseRecord
{
    public const string TableName = "Posts_Reactions";

    [Key] public long Id { get; set; }

    [Column] public long AccountId { get; set; }

    [Column] public long PostId { get; set; }

    [Column] public PostReaction Reaction { get; set; }

    [Column] public Instant LastUpdateTime { get; set; }
}