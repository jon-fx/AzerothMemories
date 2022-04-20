namespace AzerothMemories.WebServer.Services.Handlers;

internal static class CharacterServices_TrySetCharacterRenamedOrTransferred
{
    public static async Task<bool> TryHandle(CommonServices commonServices, Character_TrySetCharacterRenamedOrTransferred command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Character_TrySetCharacterRenamedOrTransferredInvalidate>();
            if (invRecord != null)
            {
                _ = commonServices.CharacterServices.DependsOnCharacterRecord(invRecord.OldCharacterId);
                _ = commonServices.CharacterServices.DependsOnCharacterRecord(invRecord.NewCharacterId);

                if (invRecord.PostIds != null)
                {
                    foreach (var postId in invRecord.PostIds)
                    {
                        _ = commonServices.PostServices.DependsOnPost(postId);
                        _ = commonServices.PostServices.GetAllPostTags(postId);
                    }
                }
            }

            return default;
        }

        var activeAccount = await commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return false;
        }

        var oldCharacterRecord = await commonServices.CharacterServices.TryGetCharacterRecord(command.OldCharacterId).ConfigureAwait(false);
        if (oldCharacterRecord == null || oldCharacterRecord.AccountId != activeAccount.Id)
        {
            return false;
        }

        var newCharacterRecord = await commonServices.CharacterServices.TryGetCharacterRecord(command.NewCharacterId).ConfigureAwait(false);
        if (newCharacterRecord == null || newCharacterRecord.AccountId != activeAccount.Id)
        {
            return false;
        }

        if (oldCharacterRecord.AccountId != newCharacterRecord.AccountId)
        {
            return false;
        }

        if (oldCharacterRecord.BlizzardRegionId != newCharacterRecord.BlizzardRegionId)
        {
            return false;
        }

        if (oldCharacterRecord.Class != newCharacterRecord.Class)
        {
            return false;
        }

        await using var database = await commonServices.DatabaseHub.CreateCommandDbContext(cancellationToken).ConfigureAwait(false);
        database.Attach(oldCharacterRecord);
        oldCharacterRecord.CharacterStatus = CharacterStatus2.RenamedOrTransferred;

        var oldTag = PostTagInfo.GetTagString(PostTagType.Character, oldCharacterRecord.Id);
        var newTag = PostTagInfo.GetTagString(PostTagType.Character, newCharacterRecord.Id);

        var allPosts = await database.Posts.Where(x => x.PostAvatar == oldTag).ToArrayAsync(cancellationToken).ConfigureAwait(false);
        foreach (var post in allPosts)
        {
            post.PostAvatar = newTag;
        }

        var allPostTags = await database.PostTags.Where(x => x.TagString == oldTag).ToArrayAsync(cancellationToken).ConfigureAwait(false);
        foreach (var postTag in allPostTags)
        {
            postTag.TagId = newCharacterRecord.Id;
            postTag.TagString = newTag;
        }

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var hashSet = new HashSet<int>(allPosts.Select(x => x.Id).ToHashSet());
        hashSet.UnionWith(allPostTags.Select(x => x.PostId));

        var item = new Character_TrySetCharacterRenamedOrTransferredInvalidate(oldCharacterRecord.AccountId.GetValueOrDefault(), oldCharacterRecord.Id, newCharacterRecord.AccountId.GetValueOrDefault(), newCharacterRecord.Id, hashSet);
        context.Operation().Items.Set(item);

        return true;
    }
}