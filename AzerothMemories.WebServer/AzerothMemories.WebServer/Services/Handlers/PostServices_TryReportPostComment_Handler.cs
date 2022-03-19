namespace AzerothMemories.WebServer.Services.Handlers;

internal sealed class PostServices_TryReportPostComment_Handler : IMoaCommandHandler<Post_TryReportPostComment, bool>
{
    private readonly CommonServices _commonServices;
    private readonly Func<Task<AppDbContext>> _databaseContextGenerator;

    public PostServices_TryReportPostComment_Handler(CommonServices commonServices, Func<Task<AppDbContext>> databaseContextGenerator)
    {
        _commonServices = commonServices;
        _databaseContextGenerator = databaseContextGenerator;
    }

    public async Task<bool> TryHandle(Post_TryReportPostComment command)
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

        var commentId = command.CommentId;
        var allCommentPages = await _commonServices.PostServices.TryGetAllPostComments(postId).ConfigureAwait(false);
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

        await using var database = await _databaseContextGenerator().ConfigureAwait(false);

        var reportQueryResult = await database.PostCommentReports.FirstOrDefaultAsync(r => r.CommentId == commentId && r.AccountId == activeAccount.Id).ConfigureAwait(false);
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

            var commentRecord = await database.PostComments.FirstAsync(x => x.Id == commentId).ConfigureAwait(false);
            commentRecord.TotalReportCount++;
        }
        else
        {
            reportQueryResult.Reason = reason;
            reportQueryResult.ReasonText = reasonText;
            reportQueryResult.ModifiedTime = SystemClock.Instance.GetCurrentInstant();
        }

        await database.SaveChangesAsync().ConfigureAwait(false);

        await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
        {
            AccountId = activeAccount.Id,
            //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.PostReportedComment,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id,
            TargetCommentId = commentId,
            OtherAccountId = commentViewModel.AccountId,
        }).ConfigureAwait(false);

        return true;
    }
}