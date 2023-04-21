namespace AzerothMemories.WebServer.Services.Handlers;

internal static class PostServices_TryRestoreMemory
{
    public static async Task<bool> TryHandle(CommonServices commonServices, Post_TryRestoreMemory command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invPost = context.Operation().Items.Get<Post_InvalidatePost>();
            if (invPost != null && invPost.PostId > 0)
            {
                _ = commonServices.PostServices.GetAllPostTags(invPost.PostId);
            }

            var invAccount = context.Operation().Items.Get<Post_InvalidateAccount>();
            if (invAccount != null && invAccount.AccountId > 0)
            {
                _ = commonServices.AccountServices.GetMemoryCount(invAccount.AccountId);
            }

            return default;
        }

        var activeAccount = await commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return false;
        }

        if (!activeAccount.CanRestoreMemory())
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

        var newTagKind = PostTagKind.PostRestored;
        int? accountTagToAdd = command.NewCharacterId >= 0 ? activeAccount.Id : null;
        int? characterTagToAdd = command.NewCharacterId > 0 ? command.NewCharacterId : null;
        int? accountTagToRemove = command.NewCharacterId >= 0 ? null : activeAccount.Id;

        if (activeAccount.Id == postRecord.AccountId)
        {
            accountTagToRemove = null;
            newTagKind = PostTagKind.Post;
        }

        var accountCharacters = activeAccount.GetAllCharactersSafe();
        if (characterTagToAdd != null && accountCharacters.FirstOrDefault(x => x.Id == characterTagToAdd.Value) == null)
        {
            return false;
        }

        await using var database = await commonServices.DatabaseHub.CreateCommandDbContext(cancellationToken).ConfigureAwait(false);

        if (accountTagToRemove != null)
        {
            var tagString = PostTagInfo.GetTagString(PostTagType.Account, accountTagToRemove.Value);
            var myAccountTag = await database.PostTags.FirstOrDefaultAsync(x => x.PostId == postId && x.TagString == tagString, cancellationToken).ConfigureAwait(false);
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
            var newAccountTag = await database.PostTags.FirstOrDefaultAsync(x => x.PostId == postId && x.TagString == tagString, cancellationToken).ConfigureAwait(false);
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

                await database.PostTags.AddAsync(newAccountTag, cancellationToken).ConfigureAwait(false);
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
        var myCharacterTags = await database.PostTags.Where(x => x.PostId == postId && characterTagStrings.Contains(x.TagString)).ToListAsync(cancellationToken).ConfigureAwait(false);
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
            var newCharacterTag = await database.PostTags.FirstOrDefaultAsync(x => x.PostId == postId && x.TagString == tagString, cancellationToken).ConfigureAwait(false);
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

                await database.PostTags.AddAsync(newCharacterTag, cancellationToken).ConfigureAwait(false);
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

        await commonServices.Commander.Call(new Account_AddNewHistoryItem
        {
            AccountId = activeAccount.Id,
            OtherAccountId = postRecord.AccountId,
            Type = AccountHistoryType.MemoryRestoredExternal1,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id
        }, cancellationToken).ConfigureAwait(false);

        if (activeAccount.Id != postRecord.AccountId)
        {
            await commonServices.Commander.Call(new Account_AddNewHistoryItem
            {
                AccountId = postRecord.AccountId,
                OtherAccountId = activeAccount.Id,
                Type = AccountHistoryType.MemoryRestoredExternal2,
                TargetId = postRecord.AccountId,
                TargetPostId = postRecord.Id
            }, cancellationToken).ConfigureAwait(false);
        }

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        context.Operation().Items.Set(new Post_InvalidatePost(postId));
        context.Operation().Items.Set(new Post_InvalidateAccount(activeAccount.Id));

        return true;
    }
}