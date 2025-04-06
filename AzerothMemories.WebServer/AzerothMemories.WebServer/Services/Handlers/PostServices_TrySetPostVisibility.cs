using ActualLab.Collections;

namespace AzerothMemories.WebServer.Services.Handlers;

internal static class PostServices_TrySetPostVisibility
{
    public static async Task<byte?> TryHandle(ILogger<PostServices> logger, CommonServices commonServices, Post_TrySetPostVisibility command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        if (Invalidation.IsActive)
        {
            if (context.Operation.Items.KeylessTryGet(out Post_InvalidatePost? invPost) && invPost != null && invPost.PostId > 0)
            {
                _ = commonServices.PostServices.DependsOnPost(invPost.PostId);
            }

            if (context.Operation.Items.KeylessTryGet(out Post_InvalidateAccount? invAccount) && invAccount != null && invAccount.AccountId > 0)
            {
                _ = commonServices.PostServices.DependsOnPostsBy(invAccount.AccountId);
            }

            if (context.Operation.Items.KeylessTryGet(out Post_InvalidateRecentPost? invRecentPosts) && invRecentPosts != null)
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

        await using var database = await commonServices.DatabaseHub.CreateOperationDbContext(cancellationToken).ConfigureAwait(false);
        database.Attach(postRecord);
        postRecord.PostVisibility = newVisibility;

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        context.Operation.Items.KeylessSet(new Post_InvalidatePost(postId));
        context.Operation.Items.KeylessSet(new Post_InvalidateAccount(postRecord.AccountId));
        context.Operation.Items.KeylessSet(new Post_InvalidateRecentPost(true));

        return newVisibility;
    }
}