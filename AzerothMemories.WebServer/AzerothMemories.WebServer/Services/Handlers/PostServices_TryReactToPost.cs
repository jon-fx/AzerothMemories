namespace AzerothMemories.WebServer.Services.Handlers;

internal static class PostServices_TryReactToPost
{
    public static async Task<int> TryHandle(CommonServices commonServices, IDatabaseContextProvider databaseContextProvider, Post_TryReactToPost command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invPost = context.Operation().Items.Get<Post_InvalidatePost>();
            if (invPost != null && invPost.PostId > 0)
            {
                _ = commonServices.PostServices.DependsOnPost(invPost.PostId);
                _ = commonServices.PostServices.TryGetPostReactions(invPost.PostId);
            }

            var invAccount = context.Operation().Items.Get<Post_InvalidateAccount>();
            if (invAccount != null && invAccount.AccountId > 0)
            {
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

        if (!activeAccount.CanReactToPost())
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

        await using var database = await databaseContextProvider.CreateCommandDbContextNow(cancellationToken).ConfigureAwait(false);
        database.Attach(postRecord);

        var newReaction = command.NewReaction;
        var reactionRecord = await database.PostReactions.FirstOrDefaultAsync(x => x.AccountId == activeAccount.Id && x.PostId == postId, cancellationToken).ConfigureAwait(false);
        if (reactionRecord == null)
        {
            if (newReaction == PostReaction.None)
            {
                return 0;
            }

            reactionRecord = new PostReactionRecord
            {
                AccountId = activeAccount.Id,
                PostId = postId,
                Reaction = newReaction,
                LastUpdateTime = SystemClock.Instance.GetCurrentInstant()
            };

            await database.PostReactions.AddAsync(reactionRecord, cancellationToken).ConfigureAwait(false);

            ModifyPostWithReaction(postRecord, newReaction, +1, true);
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
                ModifyPostWithReaction(postRecord, previousReaction, -1, newReaction == PostReaction.None);
            }

            if (newReaction != PostReaction.None)
            {
                reactionRecord.Reaction = newReaction;
                ModifyPostWithReaction(postRecord, newReaction, +1, previousReaction == PostReaction.None);
            }

            reactionRecord.LastUpdateTime = SystemClock.Instance.GetCurrentInstant();
        }

        if (newReaction != PostReaction.None)
        {
            await commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
            {
                AccountId = activeAccount.Id,
                OtherAccountId = postRecord.AccountId,
                //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                Type = AccountHistoryType.ReactedToPost1,
                TargetId = postRecord.AccountId,
                TargetPostId = postRecord.Id
            }, cancellationToken).ConfigureAwait(false);

            if (activeAccount.Id != postRecord.AccountId)
            {
                await commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
                {
                    AccountId = postRecord.AccountId,
                    OtherAccountId = activeAccount.Id,
                    //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                    Type = AccountHistoryType.ReactedToPost2,
                    TargetId = postRecord.AccountId,
                    TargetPostId = postRecord.Id
                }, cancellationToken).ConfigureAwait(false);
            }
        }

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        context.Operation().Items.Set(new Post_InvalidatePost(postId));
        context.Operation().Items.Set(new Post_InvalidateAccount(activeAccount.Id));

        return reactionRecord.Id;
    }

    private static void ModifyPostWithReaction(PostRecord record, PostReaction reaction, int change, bool modifyTotal)
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