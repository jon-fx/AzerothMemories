using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table("Posts_Comments_Reactions")]
public sealed class PostCommentReactionRecord : IDatabaseRecord
{
    [Key] public long Id { get; set; }

    [Column] public long AccountId { get; set; }

    [Column] public long CommentId { get; set; }

    [Column] public PostReaction Reaction { get; set; }

    [Column] public Instant LastUpdateTime { get; set; }

    public PostCommentReactionViewModel CreatePostCommentReactionViewModel(string username)
    {
        var viewModel = new PostCommentReactionViewModel
        {
            Id = Id,
            CommentId = CommentId,
            AccountId = AccountId,
            AccountUsername = username,
            Reaction = Reaction,
            LastUpdateTime = LastUpdateTime.ToUnixTimeMilliseconds(),
        };

        return viewModel;
    }
}