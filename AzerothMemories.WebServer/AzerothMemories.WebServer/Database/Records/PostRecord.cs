namespace AzerothMemories.WebServer.Database.Records;

[Table("Posts")]
public sealed class PostRecord : IDatabaseRecord
{
    [Column(IsPrimaryKey = true, IsIdentity = true)] public long Id { get; set; }

    [Column, NotNull] public long AccountId;

    [Column, NotNull] public string PostComment;

    [Column, Nullable] public string PostAvatar;

    [Column, Nullable] public byte PostVisibility;

    [Column, NotNull] public PostFlags PostFlags;

    [Column, NotNull] public Instant PostTime;

    [Column, NotNull] public Instant PostEditedTime;

    [Column, NotNull] public Instant PostCreatedTime;

    [Column, NotNull] public string BlobNames;

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

    [Column, NotNull] public int TotalCommentCount;

    [Column, NotNull] public int TotalReportCount;

    [Column, NotNull] public long DeletedTimeStamp;

    public PostViewModel CreatePostViewModel(AccountViewModel accountViewModel, bool canSeePost, PostReactionViewModel reactionRecord, PostTagInfo[] postTagRecords)
    {
        var viewModel = new PostViewModel
        {
            Id = Id,
            AccountId = AccountId,
            AccountUsername = accountViewModel.Username,
            AccountAvatar = accountViewModel.Avatar,
            PostComment = PostComment,
            PostVisibility = PostVisibility,
            PostTime = PostTime.ToUnixTimeMilliseconds(),
            PostCreatedTime = PostCreatedTime.ToUnixTimeMilliseconds(),
            PostEditedTime = PostEditedTime.ToUnixTimeMilliseconds(),
            ImageBlobNames = BlobNames.Split('|'),
            ReactionId = reactionRecord?.Id ?? 0,
            Reaction = reactionRecord?.Reaction ?? 0,
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
            },
            TotalReactionCount = TotalReactionCount,
            TotalCommentCount = TotalCommentCount,
            DeletedTimeStamp = DeletedTimeStamp,
            SystemTags = postTagRecords,
        };

        if (PostAvatar != null)
        {
            viewModel.PostAvatar = viewModel.SystemTags.First(x => x.TagString == PostAvatar).Image;
        }

        if (!canSeePost)
        {
            viewModel.PostComment = null;
            viewModel.PostVisibility = 255;
            viewModel.ImageBlobNames = Array.Empty<string>();
            viewModel.TotalReactionCount = 0;
            viewModel.TotalCommentCount = 0;

            Array.Fill(viewModel.ReactionCounters, 0);
        }

        return viewModel;
    }
}