namespace AzerothMemories.WebServer.Database.Records;

[Table("Posts_Comments")]
public sealed class PostCommentRecord : IDatabaseRecord
{
    [Column(IsPrimaryKey = true, IsIdentity = true)] public long Id { get; set; }

    [Column, NotNull] public long AccountId;

    [Column, NotNull] public long PostId;

    [Column, NotNull] public long? ParentId;

    [Column, NotNull] public string PostComment;

    [Column, NotNull] public int ReactionCount1;

    [Column, NotNull] public int ReactionCount2;

    [Column, NotNull] public int ReactionCount3;

    [Column, NotNull] public int ReactionCount4;

    [Column, NotNull] public int ReactionCount5;

    [Column, NotNull] public int ReactionCount6;

    [Column, NotNull] public int ReactionCount7;

    [Column, NotNull] public int ReactionCount8;

    [Column, NotNull] public int ReactionCount9;

    [Column, NotNull] public int TotalReactionCount;

    [Column, NotNull] public int TotalReportCount;

    [Column, NotNull] public Instant CreatedTime;

    [Column, NotNull] public long DeletedTimeStamp;

    public PostCommentViewModel CreateCommentViewModel(string username, string avatar)
    {
        var viewModel = new PostCommentViewModel
        {
            Id = Id,
            AccountId = AccountId,
            PostId = PostId,
            ParentId = ParentId.GetValueOrDefault(),
            AccountUsername = username,
            AccountAvatar = avatar,
            PostComment = PostComment,
            CreatedTime = CreatedTime.ToUnixTimeMilliseconds(),
            DeletedTimeStamp = DeletedTimeStamp,
            //ReactionId = reaction?.Id ?? 0,
            //Reaction = reaction?.Reaction ?? PostReaction.None,
            TotalReactionCount = TotalReactionCount,
            ReactionCounters = new[]
            {
                ReactionCount1,
                ReactionCount2,
                ReactionCount3,
                ReactionCount4,
                ReactionCount5,
                ReactionCount6,
                ReactionCount7,
                ReactionCount8,
                ReactionCount9
            }
        };

        return viewModel;
    }
}