namespace AzerothMemories.WebServer.Database.Records;

[Table("Posts_Comments_Reactions")]
public sealed class PostCommentReactionRecord : IDatabaseRecord
{
    [Column(IsPrimaryKey = true, IsIdentity = true)] public long Id { get; set; }

    [Column, NotNull] public long AccountId;

    [Column, NotNull] public long CommentId;

    [Column, NotNull] public PostReaction Reaction;

    [Column, NotNull] public Instant LastUpdateTime;
}