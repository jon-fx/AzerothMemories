using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class PostCommentReactionRecord : IDatabaseRecordWithVersion
{
    public const string TableName = "Posts_Comments_Reactions";

    [Key] public int Id { get; set; }

    [Column] public int AccountId { get; set; }

    [Column] public int CommentId { get; set; }

    [Column] public PostReaction Reaction { get; set; }

    [Column] public Instant LastUpdateTime { get; set; }

    public uint RowVersion { get; set; }

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