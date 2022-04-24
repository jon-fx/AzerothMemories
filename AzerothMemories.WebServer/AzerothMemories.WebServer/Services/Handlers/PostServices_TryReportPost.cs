namespace AzerothMemories.WebServer.Services.Handlers;

internal static class PostServices_TryReportPost
{
    public static async Task<bool> TryHandle(CommonServices commonServices, Post_TryReportPost command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            _ = commonServices.PostServices.DependsOnPostReports();

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

        var canSeePost = await commonServices.PostServices.CanAccountSeePost(activeAccount.Id, postRecord.AccountId, postRecord.PostVisibility).ConfigureAwait(false);
        if (!canSeePost)
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

        if (reasonText.Length > ZExtensions.ReportPostCommentMaxLength)
        {
            reasonText = reasonText[..ZExtensions.ReportPostCommentMaxLength];
        }

        await using var database = await commonServices.DatabaseHub.CreateCommandDbContext(cancellationToken).ConfigureAwait(false);
        database.Attach(postRecord);

        var reportQueryResult = await database.PostReports.FirstOrDefaultAsync(r => r.PostId == postRecord.Id && r.AccountId == activeAccount.Id, cancellationToken).ConfigureAwait(false);
        if (reportQueryResult == null)
        {
            reportQueryResult = new PostReportRecord
            {
                AccountId = activeAccount.Id,
                PostId = postRecord.Id,
                Reason = reason,
                ReasonText = reasonText,
                CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                ModifiedTime = SystemClock.Instance.GetCurrentInstant()
            };

            database.PostReports.Add(reportQueryResult);
            postRecord.TotalReportCount++;
        }
        else
        {
            reportQueryResult.Reason = reason;
            reportQueryResult.ReasonText = reasonText;
            reportQueryResult.ModifiedTime = SystemClock.Instance.GetCurrentInstant();
            reportQueryResult.ResolvedByAccountId = null;
        }

        await commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
        {
            AccountId = activeAccount.Id,
            //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.PostReported,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id,
            OtherAccountId = postRecord.AccountId,
        }, cancellationToken).ConfigureAwait(false);

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}