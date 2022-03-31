namespace AzerothMemories.WebServer.Services.Handlers;

internal static class PostServices_TryReportPostComment
{
    public static async Task<bool> TryHandle(CommonServices commonServices, IDatabaseContextProvider databaseContextProvider, Post_TryReportPostComment command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            _ = commonServices.PostServices.DependsOnPostCommentReports();

            return default;
        }

        var activeAccount = await commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return false;
        }

        var postId = command.PostId;
        var postRecord = await commonServices.PostServices.TryGetPostRecord(postId).ConfigureAwait(false);
        if (postRecord == null)
        {
            return false;
        }

        var canSeePost = await commonServices.PostServices.CanAccountSeePost(activeAccount.Id, postRecord).ConfigureAwait(false);
        if (!canSeePost)
        {
            return false;
        }

        var commentId = command.CommentId;
        var allCommentPages = await commonServices.PostServices.TryGetAllPostComments(postId).ConfigureAwait(false);
        var allComments = allCommentPages[0].AllComments;
        if (!allComments.TryGetValue(commentId, out var commentViewModel))
        {
            return false;
        }

        var reason = command.Reason;
        var reasonText = command.ReasonText;

        if (!Enum.IsDefined(reason))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(reasonText))
        {
            return false;
        }

        if (reasonText.Length > ZExtensions.MaxPostCommentLength)
        {
            reasonText = reasonText[..ZExtensions.MaxPostCommentLength];
        }

        await using var database = await databaseContextProvider.CreateCommandDbContextNow(cancellationToken).ConfigureAwait(false);

        var reportQueryResult = await database.PostCommentReports.FirstOrDefaultAsync(r => r.CommentId == commentId && r.AccountId == activeAccount.Id, cancellationToken).ConfigureAwait(false);
        if (reportQueryResult == null)
        {
            reportQueryResult = new PostCommentReportRecord
            {
                AccountId = activeAccount.Id,
                CommentId = commentId,
                Reason = reason,
                ReasonText = reasonText,
                CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                ModifiedTime = SystemClock.Instance.GetCurrentInstant(),
            };

            database.PostCommentReports.Add(reportQueryResult);

            var commentRecord = await database.PostComments.FirstAsync(x => x.Id == commentId, cancellationToken).ConfigureAwait(false);
            commentRecord.TotalReportCount++;
        }
        else
        {
            reportQueryResult.Reason = reason;
            reportQueryResult.ReasonText = reasonText;
            reportQueryResult.ModifiedTime = SystemClock.Instance.GetCurrentInstant();
            reportQueryResult.ResolvedByAccountId = null;
        }

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
        {
            AccountId = activeAccount.Id,
            //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.PostReportedComment,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id,
            TargetCommentId = commentId,
            OtherAccountId = commentViewModel.AccountId,
        }, cancellationToken).ConfigureAwait(false);

        return true;
    }
}