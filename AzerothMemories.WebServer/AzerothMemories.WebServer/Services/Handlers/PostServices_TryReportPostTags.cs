namespace AzerothMemories.WebServer.Services.Handlers;

internal static class PostServices_TryReportPostTags
{
    public static async Task<bool> TryHandle(CommonServices commonServices, Post_TryReportPostTags command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            _ = commonServices.PostServices.DependsOnPostTagReports();

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

        var allTagRecords = await commonServices.PostServices.GetAllPostTags(postId).ConfigureAwait(false);
        if (allTagRecords == null || allTagRecords.Length == 0)
        {
            return false;
        }

        var tagRecords = new List<PostTagRecord>();
        foreach (var tagString in command.TagStrings)
        {
            var tagRecord = allTagRecords.FirstOrDefault(x => x.TagString == tagString);
            if (tagRecord == null)
            {
                return false;
            }

            tagRecords.Add(tagRecord);
        }

        if (tagRecords.Count == 0)
        {
            return false;
        }

        await using var database = await commonServices.DatabaseHub.CreateCommandDbContext(cancellationToken).ConfigureAwait(false);

        var reportQuery = from r in database.PostTagReports
                          where r.PostId == postRecord.Id && r.AccountId == activeAccount.Id
                          select r.TagId;

        var alreadyReported = await reportQuery.ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var alreadyReportedSet = alreadyReported.ToHashSet();

        foreach (var tagRecord in tagRecords)
        {
            if (alreadyReportedSet.Contains(tagRecord.Id))
            {
            }
            else
            {
                database.Attach(tagRecord);
                tagRecord.TotalReportCount++;

                await database.PostTagReports.AddAsync(new PostTagReportRecord
                {
                    AccountId = activeAccount.Id,
                    PostId = postRecord.Id,
                    TagId = tagRecord.Id,
                    CreatedTime = SystemClock.Instance.GetCurrentInstant()
                }, cancellationToken).ConfigureAwait(false);
            }
        }

        await commonServices.Commander.Call(new Account_AddNewHistoryItem
        {
            AccountId = activeAccount.Id,
            //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.PostReportedTags,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id,
            OtherAccountId = postRecord.AccountId,
        }, cancellationToken).ConfigureAwait(false);

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}