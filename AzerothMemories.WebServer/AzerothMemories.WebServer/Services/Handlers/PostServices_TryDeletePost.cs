using ActualLab.Collections;

namespace AzerothMemories.WebServer.Services.Handlers;

internal static class PostServices_TryDeletePost
{
    public static async Task<long> TryHandle(ILogger<PostServices> logger, CommonServices commonServices, Post_TryDeletePost command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        if (Invalidation.IsActive)
        {
            if (context.Operation.Items.KeylessTryGet(out Post_InvalidatePost? invPost) && invPost != null && invPost.PostId > 0)
            {
                _ = commonServices.PostServices.DependsOnPost(invPost.PostId);
            }

            if (context.Operation.Items.KeylessTryGet(out Post_InvalidateAccount? invAccount) && invAccount != null && invAccount.AccountId > 0)
            {
                _ = commonServices.PostServices.DependsOnPostsBy(invAccount.AccountId);
                _ = commonServices.AccountServices.GetPostCount(invAccount.AccountId);
            }

            if (context.Operation.Items.KeylessTryGet(out Post_InvalidateRecentPost? invRecentPosts) && invRecentPosts != null)
            {
                _ = commonServices.PostServices.DependsOnNewPosts();
            }

            if (context.Operation.Items.KeylessTryGet(out Admin_InvalidateReports? invalidateReports) && invalidateReports != null)
            {
                _ = commonServices.PostServices.DependsOnPostReports();
            }

            return default;
        }

        var activeAccount = await commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return 0;
        }

        var postId = command.PostId;
        var postRecord = await commonServices.PostServices.TryGetPostRecord(postId).ConfigureAwait(false);
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

        await using var database = await commonServices.DatabaseHub.CreateOperationDbContext(cancellationToken).ConfigureAwait(false);
        database.Attach(postRecord);
        postRecord.DeletedTimeStamp = now;

        var imageBlobNames = postRecord.BlobNames?.Split('|');
        foreach (var blobName in imageBlobNames.SafeEnumerable())
        {
            var record = await database.UploadLogs.FirstOrDefaultAsync(x => x.AccountId == postRecord.AccountId && x.BlobName == blobName, cancellationToken).ConfigureAwait(false);
            if (record != null)
            {
                record.UploadStatus = AccountUploadLogStatus.DeletePending;
            }
        }

        var reports = await database.PostReports.Where(x => x.PostId == command.PostId).ToArrayAsync(cancellationToken).ConfigureAwait(false);
        foreach (var report in reports)
        {
            report.ResolvedByAccountId = activeAccount.Id;
        }

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await commonServices.Commander.Call(new Account_AddNewHistoryItem
        {
            AccountId = postRecord.AccountId,
            Type = AccountHistoryType.MemoryDeleted,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id,
            OtherAccountId = activeAccount.Id
        }, cancellationToken).ConfigureAwait(false);

        context.Operation.Items.KeylessSet(new Post_InvalidatePost(postId));
        context.Operation.Items.KeylessSet(new Post_InvalidateAccount(postRecord.AccountId));

        if (reports.Length > 0)
        {
            context.Operation.Items.KeylessSet(new Admin_InvalidateReports(true));
        }

        if (postRecord.PostVisibility == 0)
        {
            context.Operation.Items.KeylessSet(new Post_InvalidateRecentPost(true));
        }

        return now;
    }
}