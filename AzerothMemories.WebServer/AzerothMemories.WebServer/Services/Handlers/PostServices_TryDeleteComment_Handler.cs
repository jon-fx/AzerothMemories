namespace AzerothMemories.WebServer.Services.Handlers;

internal sealed class PostServices_TryDeleteComment_Handler : IMoaCommandHandler<Post_TryDeleteComment, long>
{
    private readonly CommonServices _commonServices;
    private readonly Func<Task<AppDbContext>> _databaseContextGenerator;

    public PostServices_TryDeleteComment_Handler(CommonServices commonServices, Func<Task<AppDbContext>> databaseContextGenerator)
    {
        _commonServices = commonServices;
        _databaseContextGenerator = databaseContextGenerator;
    }

    public async Task<long> TryHandle(Post_TryDeleteComment command)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invPost = context.Operation().Items.Get<Post_InvalidatePost>();
            if (invPost != null && invPost.PostId > 0)
            {
                _ = _commonServices.PostServices.TryGetAllPostComments(invPost.PostId);
            }

            return default;
        }

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return 0;
        }

        var postId = command.PostId;
        var postRecord = await _commonServices.PostServices.TryGetPostRecord(postId).ConfigureAwait(false);
        if (postRecord == null)
        {
            return 0;
        }

        var commentId = command.CommentId;
        var allCommentPages = await _commonServices.PostServices.TryGetAllPostComments(postId).ConfigureAwait(false);
        var allComments = allCommentPages[0].AllComments;
        if (!allComments.TryGetValue(commentId, out var commentViewModel))
        {
            return 0;
        }

        var now = SystemClock.Instance.GetCurrentInstant().ToUnixTimeMilliseconds();
        if (activeAccount.Id == commentViewModel.AccountId)
        {
        }
        else if (activeAccount.CanDeleteAnyComment())
        {
            now = -now;
        }
        else
        {
            return 0;
        }

        await using var database = await _databaseContextGenerator().ConfigureAwait(false);
        var commentRecord = await database.PostComments.FirstAsync(x => x.Id == commentId).ConfigureAwait(false);
        commentRecord.DeletedTimeStamp = now;

        await database.SaveChangesAsync().ConfigureAwait(false);

        await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
        {
            AccountId = postRecord.AccountId,
            Type = AccountHistoryType.CommentDeleted,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id,
            TargetCommentId = commentId,
            OtherAccountId = activeAccount.Id,
        }).ConfigureAwait(false);

        context.Operation().Items.Set(new Post_InvalidatePost(postId));

        return now;
    }
}