namespace AzerothMemories.WebServer.Services.Handlers;

internal static class PostServices_TrySetPostVisibility
{
    public static async Task<byte?> TryHandle(CommonServices commonServices, IDatabaseContextProvider databaseContextProvider, Post_TrySetPostVisibility command)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invPost = context.Operation().Items.Get<Post_InvalidatePost>();
            if (invPost != null && invPost.PostId > 0)
            {
                _ = commonServices.PostServices.DependsOnPost(invPost.PostId);
            }

            var invRecentPosts = context.Operation().Items.Get<Post_InvalidateRecentPost>();
            if (invRecentPosts != null)
            {
                _ = commonServices.PostServices.DependsOnNewPosts();
            }

            return default;
        }

        var activeAccount = await commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return null;
        }

        var postId = command.PostId;
        var postRecord = await commonServices.PostServices.TryGetPostRecord(postId).ConfigureAwait(false);
        if (postRecord == null)
        {
            return null;
        }

        if (activeAccount.Id == postRecord.AccountId)
        {
        }
        else if (activeAccount.CanChangeAnyPostVisibility())
        {
        }
        else
        {
            return null;
        }

        var newVisibility = Math.Clamp(command.NewVisibility, (byte)0, (byte)1);

        await using var database = await databaseContextProvider.CreateCommandDbContext().ConfigureAwait(false);
        database.Attach(postRecord);
        postRecord.PostVisibility = newVisibility;

        await database.SaveChangesAsync().ConfigureAwait(false);

        context.Operation().Items.Set(new Post_InvalidatePost(postId));
        context.Operation().Items.Set(new Post_InvalidateRecentPost(true));

        return newVisibility;
    }
}