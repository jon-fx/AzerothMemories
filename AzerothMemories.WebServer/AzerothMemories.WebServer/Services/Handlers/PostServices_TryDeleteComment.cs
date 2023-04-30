namespace AzerothMemories.WebServer.Services.Handlers;

internal static class PostServices_TryDeleteComment
{
    public static async Task<long> TryHandle(ILogger<PostServices> services, CommonServices commonServices, Post_TryDeleteComment command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invPost = context.Operation().Items.Get<Post_InvalidatePost>();
            if (invPost != null && invPost.PostId > 0)
            {
                _ = commonServices.PostServices.TryGetAllPostComments(invPost.PostId);
            }

            var invalidateReports = context.Operation().Items.Get<Admin_InvalidateReports>();
            if (invalidateReports != null)
            {
                _ = commonServices.PostServices.DependsOnPostCommentReports();
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

        var commentId = command.CommentId;
        var allCommentPages = await commonServices.PostServices.TryGetAllPostComments(postId).ConfigureAwait(false);
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

        await using var database = await commonServices.DatabaseHub.CreateCommandDbContext(cancellationToken).ConfigureAwait(false);
        var commentRecord = await database.PostComments.FirstAsync(x => x.Id == commentId, cancellationToken).ConfigureAwait(false);
        commentRecord.DeletedTimeStamp = now;

        var reports = await database.PostCommentReports.Where(x => x.CommentId == command.CommentId).ToArrayAsync(cancellationToken).ConfigureAwait(false);
        foreach (var report in reports)
        {
            report.ResolvedByAccountId = activeAccount.Id;
        }

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await commonServices.Commander.Call(new Account_AddNewHistoryItem
        {
            AccountId = postRecord.AccountId,
            Type = AccountHistoryType.CommentDeleted,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id,
            TargetCommentId = commentId,
            OtherAccountId = activeAccount.Id,
        }, cancellationToken).ConfigureAwait(false);

        if (reports.Length > 0)
        {
            context.Operation().Items.Set(new Admin_InvalidateReports(true));
        }

        context.Operation().Items.Set(new Post_InvalidatePost(postId));

        return now;
    }
}