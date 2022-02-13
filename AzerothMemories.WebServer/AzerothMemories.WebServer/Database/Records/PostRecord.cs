using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class PostRecord : IDatabaseRecord
{
    public const string TableName = "Posts";

    [Key] public long Id { get; set; }

    [Column] public long AccountId { get; set; }

    [Column] public string PostComment { get; set; }

    [Column] public string PostAvatar { get; set; }

    [Column] public byte PostVisibility { get; set; }

    [Column] public PostFlags PostFlags { get; set; }

    [Column] public Instant PostTime { get; set; }

    [Column] public Instant PostEditedTime { get; set; }

    [Column] public Instant PostCreatedTime { get; set; }

    [Column] public string BlobNames { get; set; }

    [Column] public int ReactionCount1 { get; set; }

    [Column] public int ReactionCount2 { get; set; }

    [Column] public int ReactionCount3 { get; set; }

    [Column] public int ReactionCount4 { get; set; }

    [Column] public int ReactionCount5 { get; set; }

    [Column] public int ReactionCount6 { get; set; }

    [Column] public int ReactionCount7 { get; set; }

    [Column] public int ReactionCount8 { get; set; }

    [Column] public int ReactionCount9 { get; set; }

    [Column] public int TotalReactionCount { get; set; }

    [Column] public int TotalCommentCount { get; set; }

    [Column] public int TotalReportCount { get; set; }

    [Column] public long DeletedTimeStamp { get; set; }

    public ICollection<PostTagRecord> PostTags { get; set; }

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