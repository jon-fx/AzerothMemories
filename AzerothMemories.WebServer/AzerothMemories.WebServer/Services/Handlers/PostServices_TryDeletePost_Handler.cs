namespace AzerothMemories.WebServer.Services.Handlers;

internal sealed class PostServices_TryDeletePost_Handler : IMoaCommandHandler<Post_TryDeletePost, long>
{
    private readonly CommonServices _commonServices;
    private readonly Func<Task<AppDbContext>> _databaseContextGenerator;

    public PostServices_TryDeletePost_Handler(CommonServices commonServices, Func<Task<AppDbContext>> databaseContextGenerator)
    {
        _commonServices = commonServices;
        _databaseContextGenerator = databaseContextGenerator;
    }

    public async Task<long> TryHandle(Post_TryDeletePost command)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invPost = context.Operation().Items.Get<Post_InvalidatePost>();
            if (invPost != null && invPost.PostId > 0)
            {
                _ = _commonServices.PostServices.DependsOnPost(invPost.PostId);
            }

            var invAccount = context.Operation().Items.Get<Post_InvalidateAccount>();
            if (invAccount != null && invAccount.AccountId > 0)
            {
                _ = _commonServices.PostServices.DependsOnPostsBy(invAccount.AccountId);
                _ = _commonServices.AccountServices.GetPostCount(invAccount.AccountId);
            }

            var invRecentPosts = context.Operation().Items.Get<Post_InvalidateRecentPost>();
            if (invRecentPosts != null)
            {
                _ = _commonServices.PostServices.DependsOnNewPosts();
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

        var now = SystemClock.Instance.GetCurrentInstant().ToUnixTimeMilliseconds();
        if (activeAccount.Id == postRecord.AccountId)
        {
        }
        else if (activeAccount.CanDeleteAnyPost())
        {
            now = -now;
        }
        else
        {
            return 0;
        }

        await using var database = await _databaseContextGenerator().ConfigureAwait(false);
        database.Attach(postRecord);
        postRecord.DeletedTimeStamp = now;

        var imageBlobNames = postRecord.BlobNames.Split('|');
        foreach (var blobName in imageBlobNames)
        {
            var record = await database.UploadLogs.FirstOrDefaultAsync(x => x.AccountId == postRecord.AccountId && x.BlobName == blobName).ConfigureAwait(false);
            if (record != null)
            {
                record.UploadStatus = AccountUploadLogStatus.DeletePending;
            }
        }

        await database.SaveChangesAsync().ConfigureAwait(false);

        await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
        {
            AccountId = postRecord.AccountId,
            //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.MemoryDeleted,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id,
            OtherAccountId = activeAccount.Id,
        }).ConfigureAwait(false);

        context.Operation().Items.Set(new Post_InvalidatePost(postId));
        context.Operation().Items.Set(new Post_InvalidateAccount(postRecord.AccountId));

        if (postRecord.PostVisibility == 0)
        {
            context.Operation().Items.Set(new Post_InvalidateRecentPost(true));
        }

        return now;
    }
}