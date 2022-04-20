namespace AzerothMemories.WebServer.Services.Handlers;

internal static class PostServices_TryReactToPostComment
{
    public static async Task<int> TryHandle(CommonServices commonServices, Post_TryReactToPostComment command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invPost = context.Operation().Items.Get<Post_InvalidatePost>();
            if (invPost != null && invPost.PostId > 0)
            {
                _ = commonServices.PostServices.TryGetAllPostComments(invPost.PostId);
                _ = commonServices.PostServices.TryGetPostCommentReactions(invPost.PostId);
            }

            var invAccount = context.Operation().Items.Get<Post_InvalidateAccount>();
            if (invAccount != null && invAccount.AccountId > 0)
            {
                _ = commonServices.PostServices.TryGetMyCommentReactions(invAccount.AccountId, invPost?.PostId ?? 0);
                _ = commonServices.AccountServices.GetReactionCount(invAccount.AccountId);
            }

            return default;
        }

        if (!Enum.IsDefined(command.NewReaction))
        {
            return 0;
        }

        var activeAccount = await commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return 0;
        }

        if (!activeAccount.CanReactToComment())
        {
            return 0;
        }

        var postId = command.PostId;
        var postRecord = await commonServices.PostServices.TryGetPostRecord(postId).ConfigureAwait(false);
        if (postRecord == null)
        {
            return 0;
        }

        var canSeePost = await commonServices.PostServices.CanAccountSeePost(activeAccount.Id, postRecord).ConfigureAwait(false);
        if (!canSeePost)
        {
            return 0;
        }

        var commentId = command.CommentId;
        var allCommentPages = await commonServices.PostServices.TryGetAllPostComments(postId).ConfigureAwait(false);
        var allComments = allCommentPages[0].AllComments;
        if (!allComments.TryGetValue(commentId, out var commentViewModel))
        {
            return 0;
        }

        await using var database = await commonServices.DatabaseHub.CreateCommandDbContext(cancellationToken).ConfigureAwait(false);
        var commentRecord = database.PostComments.First(x => x.Id == commentId);

        var newReaction = command.NewReaction;
        var reactionRecord = await database.PostCommentReactions.FirstOrDefaultAsync(x => x.AccountId == activeAccount.Id && x.CommentId == commentId, cancellationToken).ConfigureAwait(false);
        if (reactionRecord == null)
        {
            if (newReaction == PostReaction.None)
            {
                return 0;
            }

            reactionRecord = new PostCommentReactionRecord
            {
                AccountId = activeAccount.Id,
                CommentId = commentId,
                Reaction = newReaction,
                LastUpdateTime = SystemClock.Instance.GetCurrentInstant()
            };

            await database.PostCommentReactions.AddAsync(reactionRecord, cancellationToken).ConfigureAwait(false);

            ModifyPostCommentWithReaction(commentRecord, newReaction, +1, true);
        }
        else
        {
            if (newReaction == reactionRecord.Reaction)
            {
                return reactionRecord.Id;
            }

            var previousReaction = reactionRecord.Reaction;
            if (previousReaction != PostReaction.None)
            {
                reactionRecord.Reaction = PostReaction.None;

                ModifyPostCommentWithReaction(commentRecord, previousReaction, -1, newReaction == PostReaction.None);
            }

            if (newReaction != PostReaction.None)
            {
                reactionRecord.Reaction = newReaction;

                ModifyPostCommentWithReaction(commentRecord, newReaction, +1, previousReaction == PostReaction.None);
            }

            reactionRecord.LastUpdateTime = SystemClock.Instance.GetCurrentInstant();
        }

        if (newReaction != PostReaction.None)
        {
            await commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
            {
                AccountId = activeAccount.Id,
                OtherAccountId = commentViewModel.AccountId,
                //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                Type = AccountHistoryType.ReactedToComment1,
                TargetId = postRecord.AccountId,
                TargetPostId = postRecord.Id,
                TargetCommentId = commentId
            }, cancellationToken).ConfigureAwait(false);

            if (activeAccount.Id != commentViewModel.AccountId)
            {
                await commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
                {
                    AccountId = commentViewModel.AccountId,
                    OtherAccountId = activeAccount.Id,
                    //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                    Type = AccountHistoryType.ReactedToComment2,
                    TargetId = postRecord.AccountId,
                    TargetPostId = postRecord.Id,
                    TargetCommentId = commentId
                }, cancellationToken).ConfigureAwait(false);
            }
        }

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        context.Operation().Items.Set(new Post_InvalidatePost(postId));
        context.Operation().Items.Set(new Post_InvalidateAccount(activeAccount.Id));

        return reactionRecord.Id;
    }

    public static void ModifyPostCommentWithReaction(PostCommentRecord record, PostReaction reaction, int change, bool modifyTotal)
    {
        switch (reaction)
        {
            case PostReaction.None:
            {
                //return query;
                break;
            }
            case PostReaction.Reaction1:
            {
                record.ReactionCount1 += change;
                break;
            }
            case PostReaction.Reaction2:
            {
                record.ReactionCount2 += change;
                break;
            }
            case PostReaction.Reaction3:
            {
                record.ReactionCount3 += change;
                break;
            }
            case PostReaction.Reaction4:
            {
                record.ReactionCount4 += change;
                break;
            }
            case PostReaction.Reaction5:
            {
                record.ReactionCount5 += change;
                break;
            }
            case PostReaction.Reaction6:
            {
                record.ReactionCount6 += change;
                break;
            }
            case PostReaction.Reaction7:
            {
                record.ReactionCount7 += change;
                break;
            }
            case PostReaction.Reaction8:
            {
                record.ReactionCount8 += change;
                break;
            }
            case PostReaction.Reaction9:
            {
                record.ReactionCount9 += change;
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        if (modifyTotal)
        {
            record.TotalReactionCount += change;
        }
    }
}