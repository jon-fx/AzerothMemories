namespace AzerothMemories.WebServer.Services.Handlers;

internal static class PostServices_TryPublishComment
{
    public static async Task<int> TryHandle(ILogger<PostServices> services, CommonServices commonServices, Post_TryPublishComment command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invPost = context.Operation().Items.Get<Post_InvalidatePost>();
            if (invPost != null && invPost.PostId > 0)
            {
                _ = commonServices.PostServices.DependsOnPost(invPost.PostId);
                _ = commonServices.PostServices.TryGetAllPostComments(invPost.PostId);
            }

            var invAccount = context.Operation().Items.Get<Post_InvalidateAccount>();
            if (invAccount != null && invAccount.AccountId > 0)
            {
                _ = commonServices.AccountServices.GetCommentCount(invAccount.AccountId);
            }

            _ = commonServices.PostServices.DependsOnNewComments();

            return default;
        }

        var activeAccount = await commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return 0;
        }

        if (!activeAccount.CanPublishComment())
        {
            return 0;
        }

        var postId = command.PostId;
        var postRecord = await commonServices.PostServices.TryGetPostRecord(postId).ConfigureAwait(false);
        if (postRecord == null)
        {
            return 0;
        }

        var canSeePost = await commonServices.PostServices.CanAccountSeePost(activeAccount.Id, postRecord.AccountId, postRecord.PostVisibility).ConfigureAwait(false);
        if (!canSeePost)
        {
            return 0;
        }

        var commentText = command.CommentText;
        if (string.IsNullOrWhiteSpace(commentText))
        {
            return 0;
        }

        if (commentText.Length >= 2048)
        {
            return 0;
        }

        PostCommentViewModel parentComment = null;
        var parentCommentId = command.ParentCommentId;
        var allCommentPages = await commonServices.PostServices.TryGetAllPostComments(postId).ConfigureAwait(false);
        var allComments = allCommentPages[0].AllComments;
        if (parentCommentId > 0 && !allComments.TryGetValue(parentCommentId, out parentComment))
        {
            return 0;
        }

        {
            var depth = -1;
            var parentId = command.ParentCommentId;
            while (parentId > 0 && allComments.TryGetValue(parentId, out var temp))
            {
                depth++;
                parentId = temp.ParentId;
            }

            if (depth > ZExtensions.MaxCommentDepth)
            {
                return 0;
            }
        }

        var usersThatCanBeTagged = new Dictionary<int, string>(activeAccount.GetUserTagList());
        if (parentComment != null)
        {
            usersThatCanBeTagged.TryAdd(parentComment.AccountId, parentComment.AccountUsername);
        }

        var parseResult = commonServices.MarkdownServices.GetCommentText(commentText, usersThatCanBeTagged, false);
        if (parseResult.ResultCode != MarkdownParserResultCode.Success)
        {
            return 0;
        }

        var tagRecords = new HashSet<PostTagRecord>();
        foreach (var accountId in parseResult.AccountsTaggedInComment)
        {
            var tagRecord = await commonServices.TagServices.TryCreateTagRecord(postRecord, PostTagType.Account, accountId, PostTagKind.UserComment).ConfigureAwait(false);
            if (tagRecord == null)
            {
                return 0;
            }

            tagRecords.Add(tagRecord);
        }

        foreach (var hashTag in parseResult.HashTagsTaggedInComment)
        {
            var tagRecord = await commonServices.TagServices.GetHashTagRecord(hashTag, PostTagKind.UserComment).ConfigureAwait(false);
            if (tagRecord == null)
            {
                return 0;
            }

            tagRecords.Add(tagRecord);
        }

        if (tagRecords.Count > 16)
        {
            return 0;
        }

        await using var database = await commonServices.DatabaseHub.CreateCommandDbContext(cancellationToken).ConfigureAwait(false);
        database.Attach(postRecord);

        var commentRecord = new PostCommentRecord
        {
            AccountId = activeAccount.Id,
            PostId = postId,
            ParentId = parentComment?.Id,
            PostCommentRaw = parseResult.CommentText,
            PostCommentMark = parseResult.CommentTextMarkdown,
            PostCommentUserMap = parseResult.AccountsTaggedInCommentMap,
            CreatedTime = SystemClock.Instance.GetCurrentInstant(),
        };

        foreach (var tagRecord in tagRecords)
        {
            tagRecord.PostId = postRecord.Id;
            tagRecord.CommentId = commentRecord.Id;
        }

        postRecord.TotalCommentCount++;
        commentRecord.CommentTags = tagRecords;

        await database.PostComments.AddAsync(commentRecord, cancellationToken).ConfigureAwait(false);
        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await commonServices.Commander.Call(new Account_AddNewHistoryItem
        {
            AccountId = activeAccount.Id,
            OtherAccountId = postRecord.AccountId,
            Type = AccountHistoryType.Commented1,
            TargetId = postRecord.AccountId,
            TargetPostId = postId,
            TargetCommentId = commentRecord.Id
        }, cancellationToken).ConfigureAwait(false);

        if (activeAccount.Id != postRecord.AccountId)
        {
            await commonServices.Commander.Call(new Account_AddNewHistoryItem
            {
                AccountId = postRecord.AccountId,
                OtherAccountId = activeAccount.Id,
                Type = AccountHistoryType.Commented2,
                TargetId = postRecord.AccountId,
                TargetPostId = postId,
                TargetCommentId = commentRecord.Id
            }, cancellationToken).ConfigureAwait(false);
        }

        foreach (var userTag in parseResult.AccountsTaggedInComment)
        {
            await commonServices.Commander.Call(new Account_AddNewHistoryItem
            {
                AccountId = userTag,
                OtherAccountId = activeAccount.Id,
                Type = AccountHistoryType.TaggedComment,
                TargetId = postRecord.AccountId,
                TargetPostId = postRecord.Id,
                TargetCommentId = commentRecord.Id
            }, cancellationToken).ConfigureAwait(false);
        }

        context.Operation().Items.Set(new Post_InvalidatePost(postId));
        context.Operation().Items.Set(new Post_InvalidateAccount(activeAccount.Id));

        return commentRecord.Id;
    }
}