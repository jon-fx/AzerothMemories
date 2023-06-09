﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzerothMemories.WebServer.Database.Records;

[Table(TableName)]
public sealed class PostCommentRecord : IDatabaseRecordWithVersion
{
    public const string TableName = "Posts_Comments";

    [Key] public int Id { get; set; }

    [Column] public int AccountId { get; set; }

    [Column] public int PostId { get; set; }

    [Column] public int? ParentId { get; set; }

    [Column] public string PostCommentRaw { get; set; }

    [Column] public string PostCommentMark { get; set; }

    [Column] public string PostCommentUserMap { get; set; }

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

    [Column] public int TotalReportCount { get; set; }

    [Column] public Instant CreatedTime { get; set; }

    [Column] public long DeletedTimeStamp { get; set; }

    public uint RowVersion { get; set; }

    public ICollection<PostTagRecord> CommentTags { get; set; }

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
            PostComment = PostCommentMark,
            CreatedTime = CreatedTime.ToUnixTimeMilliseconds(),
            DeletedTimeStamp = DeletedTimeStamp,
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