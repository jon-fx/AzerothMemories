using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table("Posts_Reactions")]
public sealed class PostReactionRecord : IDatabaseRecord
{
    [Key] public long Id { get; set; }

    [Column] public long AccountId { get; set; }

    [Column] public long PostId { get; set; }

    [Column] public PostReaction Reaction { get; set; }

    [Column] public Instant LastUpdateTime { get; set; }
}