namespace AzerothMemories.WebServer.Services.Handlers;

internal sealed class PostServices_TryUpdateSystemTags_Handler : IMoaCommandHandler<Post_TryUpdateSystemTags, AddMemoryResultCode>
{
    private readonly CommonServices _commonServices;
    private readonly Func<Task<AppDbContext>> _databaseContextGenerator;

    public PostServices_TryUpdateSystemTags_Handler(CommonServices commonServices, Func<Task<AppDbContext>> databaseContextGenerator)
    {
        _commonServices = commonServices;
        _databaseContextGenerator = databaseContextGenerator;
    }

    public async Task<AddMemoryResultCode> TryHandle(Post_TryUpdateSystemTags command)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invPost = context.Operation().Items.Get<Post_InvalidatePost>();
            if (invPost != null && invPost.PostId > 0)
            {
                _ = _commonServices.PostServices.DependsOnPost(invPost.PostId);
                _ = _commonServices.PostServices.GetAllPostTags(invPost.PostId);
            }

            var invTags = context.Operation().Items.Get<Post_InvalidateTags>();
            if (invTags != null && invTags.TagStrings != null)
            {
                foreach (var tagString in invTags.TagStrings)
                {
                    _ = _commonServices.PostServices.DependsOnPostsWithTagString(tagString);
                }
            }

            return default;
        }

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return AddMemoryResultCode.SessionNotFound;
        }

        if (!activeAccount.CanUpdateSystemTags())
        {
            return AddMemoryResultCode.SessionCanNotInteract;
        }

        var postId = command.PostId;
        var cachedPostRecord = await _commonServices.PostServices.TryGetPostRecord(postId).ConfigureAwait(false);
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
            accountViewModel = await _commonServices.AccountServices.TryGetAccountById(command.Session, cachedPostRecord.AccountId).ConfigureAwait(false);
        }
        else if (activeAccount.Id != cachedPostRecord.AccountId)
        {
            accountViewModel = null;
        }

        if (accountViewModel == null)
        {
            return AddMemoryResultCode.SessionNotFound;
        }

        await using var database = await _databaseContextGenerator().ConfigureAwait(false);

        var postRecord = await database.Posts.Include(x => x.PostTags).FirstOrDefaultAsync(p => p.DeletedTimeStamp == 0 && p.Id == postId).ConfigureAwait(false);
        if (postRecord == null)
        {
            return AddMemoryResultCode.Failed;
        }

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
                var newRecord = await _commonServices.TagServices.TryCreateTagRecord(systemTag, postRecord, accountViewModel, PostTagKind.Post).ConfigureAwait(false);
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
                    }
                }
                else
                {
                    return AddMemoryResultCode.InvalidTags;
                }
            }

            if (!PostTagRecord.ValidateTagCounts(postRecord.PostTags.ToHashSet()))
            {
                return AddMemoryResultCode.TooManyTags;
            }
        }

        var avatar = command.Avatar;
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

        context.Operation().Items.Set(new Post_InvalidatePost(postId));
        context.Operation().Items.Set(new Post_InvalidateTags(postRecord.PostTags.Select(x => x.TagString).ToHashSet()));

        return AddMemoryResultCode.Success;
    }
}