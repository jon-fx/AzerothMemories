namespace AzerothMemories.WebServer.Services.Handlers;

internal static class PostServices_TryUpdateSystemTags
{
    public static async Task<AddMemoryResultCode> TryHandle(CommonServices commonServices, IDatabaseContextProvider databaseContextProvider, Post_TryUpdateSystemTags command)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invPost = context.Operation().Items.Get<Post_InvalidatePost>();
            if (invPost != null && invPost.PostId > 0)
            {
                _ = commonServices.PostServices.DependsOnPost(invPost.PostId);
                _ = commonServices.PostServices.GetAllPostTags(invPost.PostId);
            }

            var invTags = context.Operation().Items.Get<Post_InvalidateTags>();
            if (invTags != null && invTags.TagStrings != null)
            {
                foreach (var tagString in invTags.TagStrings)
                {
                    _ = commonServices.PostServices.DependsOnPostsWithTagString(tagString);
                }
            }

            var invalidateReports = context.Operation().Items.Get<Admin_InvalidateReports>();
            if (invalidateReports != null)
            {
                _ = commonServices.PostServices.DependsOnPostReports();
            }

            return default;
        }

        var activeAccount = await commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return AddMemoryResultCode.SessionNotFound;
        }

        if (!activeAccount.CanUpdateSystemTags())
        {
            return AddMemoryResultCode.SessionCanNotInteract;
        }

        var postId = command.PostId;
        var cachedPostRecord = await commonServices.PostServices.TryGetPostRecord(postId).ConfigureAwait(false);
        if (cachedPostRecord == null)
        {
            return AddMemoryResultCode.Failed;
        }

        var deletedTagKind = PostTagKind.DeletedByPoster;
        var accountViewModel = activeAccount;
        var isAdmin = activeAccount.CanUpdateSystemTagsOnAnyPost();
        if (isAdmin)
        {
            deletedTagKind = PostTagKind.DeletedByAdmin;
            accountViewModel = await commonServices.AccountServices.TryGetAccountById(command.Session, cachedPostRecord.AccountId).ConfigureAwait(false);
        }
        else if (activeAccount.Id != cachedPostRecord.AccountId)
        {
            accountViewModel = null;
        }

        if (accountViewModel == null)
        {
            return AddMemoryResultCode.SessionNotFound;
        }

        await using var database = await databaseContextProvider.CreateCommandDbContext().ConfigureAwait(false);

        var postRecord = await database.Posts.Include(x => x.PostTags).FirstOrDefaultAsync(p => p.DeletedTimeStamp == 0 && p.Id == postId).ConfigureAwait(false);
        if (postRecord == null)
        {
            return AddMemoryResultCode.Failed;
        }

        var shouldInvalidateReports = false;
        var allCurrentTags = postRecord.PostTags.Where(x => x.IsPostTag).ToDictionary(x => x.TagString, x => x);
        var allActiveTags = allCurrentTags.Where(x => !x.Value.IsDeleted).Select(x => x.Key).ToHashSet();

        var addedSet = new HashSet<string>(command.NewTags);
        addedSet.ExceptWith(allActiveTags);

        var removedSet = new HashSet<string>(allActiveTags);
        removedSet.ExceptWith(command.NewTags);

        if (addedSet.Count > 64)
        {
            return AddMemoryResultCode.TooManyTags;
        }

        if (addedSet.Count > 0 || removedSet.Count > 0)
        {
            foreach (var systemTag in addedSet)
            {
                var newRecord = await commonServices.TagServices.TryCreateTagRecord(systemTag, postRecord, accountViewModel, PostTagKind.Post).ConfigureAwait(false);
                if (newRecord == null)
                {
                    return AddMemoryResultCode.InvalidTags;
                }

                if (allCurrentTags.TryGetValue(newRecord.TagString, out var currentTag))
                {
                    newRecord = currentTag;
                }
                else
                {
                    postRecord.PostTags.Add(newRecord);
                }

                if (isAdmin)
                {
                }
                else if (newRecord.TagKind == PostTagKind.DeletedByAdmin)
                {
                    return AddMemoryResultCode.InvalidTags;
                }

                newRecord.PostId = postRecord.Id;
                newRecord.TagKind = PostTagKind.Post;

                allCurrentTags[newRecord.TagString] = newRecord;
            }

            foreach (var systemTag in removedSet)
            {
                if (allCurrentTags.TryGetValue(systemTag, out var tagRecord))
                {
                    if (tagRecord.TagKind == PostTagKind.DeletedByAdmin)
                    {
                    }
                    else
                    {
                        tagRecord.TagKind = deletedTagKind;

                        var reports = await database.PostTagReports.Where(x => x.TagId == tagRecord.Id).ToArrayAsync().ConfigureAwait(false);
                        foreach (var report in reports)
                        {
                            report.ResolvedByAccountId = activeAccount.Id;
                        }

                        if (reports.Length > 0)
                        {
                            shouldInvalidateReports = true;
                        }
                    }
                }
                else
                {
                    return AddMemoryResultCode.InvalidTags;
                }
            }

            if (!PostTagRecord.ValidateTagCounts(postRecord.PostTags.ToHashSet()))
            {
                return AddMemoryResultCode.InvalidTags;
            }
        }

        var avatar = command.Avatar;
        if (avatar == Post_TryUpdateSystemTags.DefaultAvatar)
        {
            avatar = postRecord.PostAvatar;
        }

        var activeTags = postRecord.PostTags.Where(x => x.IsPostTag && !x.IsDeleted).Select(x => x.TagString).ToHashSet();
        if (!string.IsNullOrWhiteSpace(avatar) && !activeTags.Contains(avatar))
        {
            avatar = postRecord.PostAvatar;
        }

        if (!string.IsNullOrWhiteSpace(avatar) && !activeTags.Contains(avatar))
        {
            avatar = null;
        }

        postRecord.PostAvatar = avatar;

        await database.SaveChangesAsync().ConfigureAwait(false);

        if (shouldInvalidateReports)
        {
            context.Operation().Items.Set(new Admin_InvalidateReports(true));
        }

        context.Operation().Items.Set(new Post_InvalidatePost(postId));
        context.Operation().Items.Set(new Post_InvalidateTags(postRecord.PostTags.Select(x => x.TagString).ToHashSet()));

        return AddMemoryResultCode.Success;
    }
}