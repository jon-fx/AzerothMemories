namespace AzerothMemories.WebServer.Services.Handlers;

internal sealed class PostServices_TryReportPost_Handler : IMoaCommandHandler<Post_TryReportPost, bool>
{
    private readonly CommonServices _commonServices;
    private readonly Func<Task<AppDbContext>> _databaseContextGenerator;

    public PostServices_TryReportPost_Handler(CommonServices commonServices, Func<Task<AppDbContext>> databaseContextGenerator)
    {
        _commonServices = commonServices;
        _databaseContextGenerator = databaseContextGenerator;
    }

    public async Task<bool> TryHandle(Post_TryReportPost command)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            return default;
        }

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return false;
        }

        var postId = command.PostId;
        var postRecord = await _commonServices.PostServices.TryGetPostRecord(postId).ConfigureAwait(false);
        if (postRecord == null)
        {
            return false;
        }

        var canSeePost = await _commonServices.PostServices.CanAccountSeePost(activeAccount.Id, postRecord).ConfigureAwait(false);
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

        await using var database = await _databaseContextGenerator().ConfigureAwait(false);
        database.Attach(postRecord);

        var reportQueryResult = await database.PostReports.FirstOrDefaultAsync(r => r.PostId == postRecord.Id && r.AccountId == activeAccount.Id).ConfigureAwait(false);
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
        }

        await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
        {
            AccountId = activeAccount.Id,
            //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.PostReported,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id,
            OtherAccountId = postRecord.AccountId,
        }).ConfigureAwait(false);

        await database.SaveChangesAsync().ConfigureAwait(false);

        return true;
    }
}