namespace AzerothMemories.WebServer.Database.Records
{
    [Table("Posts_Reactions")]
    public sealed class PostReactionRecord : IDatabaseRecord
    {
        [Column(IsPrimaryKey = true, IsIdentity = true)] public long Id { get; set; }

        [Column, NotNull] public long AccountId;

        [Column, NotNull] public long PostId;

        [Column, NotNull] public PostReaction Reaction;

        [Column, NotNull] public Instant LastUpdateTime;
    }
}