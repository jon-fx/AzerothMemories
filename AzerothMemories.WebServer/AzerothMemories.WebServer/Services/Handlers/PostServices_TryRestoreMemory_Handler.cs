namespace AzerothMemories.WebServer.Services.Handlers;

internal sealed class PostServices_TryRestoreMemory_Handler : IMoaCommandHandler<Post_TryRestoreMemory, bool>
{
    private readonly CommonServices _commonServices;
    private readonly Func<Task<AppDbContext>> _databaseContextGenerator;

    public PostServices_TryRestoreMemory_Handler(CommonServices commonServices, Func<Task<AppDbContext>> databaseContextGenerator)
    {
        _commonServices = commonServices;
        _databaseContextGenerator = databaseContextGenerator;
    }

    public async Task<bool> TryHandle(Post_TryRestoreMemory command)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invPost = context.Operation().Items.Get<Post_InvalidatePost>();
            if (invPost != null && invPost.PostId > 0)
            {
                _ = _commonServices.PostServices.GetAllPostTags(invPost.PostId);
            }

            var invAccount = context.Operation().Items.Get<Post_InvalidateAccount>();
            if (invAccount != null && invAccount.AccountId > 0)
            {
                _ = _commonServices.AccountServices.GetMemoryCount(invAccount.AccountId);
            }

            return default;
        }

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return false;
        }

        if (!activeAccount.CanRestoreMemory())
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

        var newTagKind = PostTagKind.PostRestored;
        int? accountTagToAdd = command.NewCharacterId > 0 ? activeAccount.Id : null;
        int? characterTagToAdd = command.NewCharacterId > 0 ? command.NewCharacterId : null;
        int? accountTagToRemove = command.NewCharacterId > 0 ? null : activeAccount.Id;

        if (activeAccount.Id == postRecord.AccountId)
        {
            accountTagToRemove = null;
            newTagKind = PostTagKind.Post;
        }
        //else
        //{
        //    var accountRecord = await _commonServices.AccountServices.TryGetAccountRecord(postRecord.AccountId).ConfigureAwait(false);
        //    if (accountRecord.BlizzardRegionId != activeAccount.RegionId)
        //    {
        //        return false;
        //    }
        //}

        var accountCharacters = activeAccount.GetAllCharactersSafe();
        if (characterTagToAdd != null && accountCharacters.FirstOrDefault(x => x.Id == characterTagToAdd.Value) == null)
        {
            return false;
        }

        await using var database = await _databaseContextGenerator().ConfigureAwait(false);

        if (accountTagToRemove != null)
        {
            var tagString = PostTagInfo.GetTagString(PostTagType.Account, accountTagToRemove.Value);
            var myAccountTag = await database.PostTags.FirstOrDefaultAsync(x => x.PostId == postId && x.TagString == tagString).ConfigureAwait(false);
            if (myAccountTag == null)
            {
            }
            else if (myAccountTag.TagKind == PostTagKind.Deleted)
            {
            }
            else if (myAccountTag.TagKind == PostTagKind.DeletedByPoster)
            {
            }
            else
            {
                myAccountTag.TagKind = PostTagKind.Deleted;
            }
        }

        if (accountTagToAdd != null)
        {
            var tagString = PostTagInfo.GetTagString(PostTagType.Account, accountTagToAdd.Value);
            var newAccountTag = await database.PostTags.FirstOrDefaultAsync(x => x.PostId == postId && x.TagString == tagString).ConfigureAwait(false);
            if (newAccountTag == null)
            {
                newAccountTag = new PostTagRecord
                {
                    TagId = accountTagToAdd.Value,
                    TagType = PostTagType.Account,
                    TagString = tagString,
                    PostId = postId,
                    CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                    TagKind = newTagKind
                };

                await database.PostTags.AddAsync(newAccountTag).ConfigureAwait(false);
            }
            else if (newTagKind == PostTagKind.PostRestored && newAccountTag.TagKind == PostTagKind.DeletedByPoster)
            {
                return false;
            }
            else
            {
                newAccountTag.TagKind = newTagKind;
            }
        }

        var characterTagStrings = accountCharacters.Select(x => x.TagString).ToList();
        var myCharacterTags = await database.PostTags.Where(x => x.PostId == postId && characterTagStrings.Contains(x.TagString)).ToListAsync().ConfigureAwait(false);
        foreach (var characterTag in myCharacterTags)
        {
            if (newTagKind == PostTagKind.PostRestored && characterTag.TagKind == PostTagKind.DeletedByPoster)
            {
                return false;
            }

            if (characterTag.TagKind == PostTagKind.Deleted)
            {
                continue;
            }

            characterTag.TagKind = PostTagKind.Deleted;
        }

        if (characterTagToAdd != null)
        {
            var tagString = PostTagInfo.GetTagString(PostTagType.Character, characterTagToAdd.Value);
            var newCharacterTag = await database.PostTags.FirstOrDefaultAsync(x => x.PostId == postId && x.TagString == tagString).ConfigureAwait(false);
            if (newCharacterTag == null)
            {
                newCharacterTag = new PostTagRecord
                {
                    TagId = characterTagToAdd.Value,
                    TagType = PostTagType.Character,
                    TagString = tagString,
                    PostId = postId,
                    CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                    TagKind = newTagKind
                };

                await database.PostTags.AddAsync(newCharacterTag).ConfigureAwait(false);
            }
            else if (newTagKind == PostTagKind.PostRestored && newCharacterTag.TagKind == PostTagKind.DeletedByPoster)
            {
                return false;
            }
            else
            {
                newCharacterTag.TagKind = newTagKind;
            }
        }

        await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
        {
            AccountId = activeAccount.Id,
            OtherAccountId = postRecord.AccountId,
            //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.MemoryRestoredExternal1,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id
        }).ConfigureAwait(false);

        if (activeAccount.Id != postRecord.AccountId)
        {
            await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
            {
                AccountId = postRecord.AccountId,
                OtherAccountId = activeAccount.Id,
                //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                Type = AccountHistoryType.MemoryRestoredExternal2,
                TargetId = postRecord.AccountId,
                TargetPostId = postRecord.Id
            }).ConfigureAwait(false);
        }

        await database.SaveChangesAsync().ConfigureAwait(false);

        context.Operation().Items.Set(new Post_InvalidatePost(postId));
        context.Operation().Items.Set(new Post_InvalidateAccount(activeAccount.Id));

        return true;
    }
}