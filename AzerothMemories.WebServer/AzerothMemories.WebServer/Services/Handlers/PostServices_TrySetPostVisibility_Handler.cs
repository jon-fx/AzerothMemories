namespace AzerothMemories.WebServer.Services.Handlers;

internal sealed class PostServices_TrySetPostVisibility_Handler : IMoaCommandHandler<Post_TrySetPostVisibility, byte?>
{
    private readonly CommonServices _commonServices;
    private readonly Func<Task<AppDbContext>> _databaseContextGenerator;

    public PostServices_TrySetPostVisibility_Handler(CommonServices commonServices, Func<Task<AppDbContext>> databaseContextGenerator)
    {
        _commonServices = commonServices;
        _databaseContextGenerator = databaseContextGenerator;
    }

    public async Task<byte?> TryHandle(Post_TrySetPostVisibility command)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invPost = context.Operation().Items.Get<Post_InvalidatePost>();
            if (invPost != null && invPost.PostId > 0)
            {
                _ = _commonServices.PostServices.DependsOnPost(invPost.PostId);
            }

            var invRecentPosts = context.Operation().Items.Get<Post_InvalidateRecentPost>();
            if (invRecentPosts != null)
            {
                _ = _commonServices.PostServices.DependsOnNewPosts();
            }

            return default;
        }

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return null;
        }

        var postId = command.PostId;
        var postRecord = await _commonServices.PostServices.TryGetPostRecord(postId).ConfigureAwait(false);
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

        await using var database = await _databaseContextGenerator().ConfigureAwait(false);
        database.Attach(postRecord);
        postRecord.PostVisibility = newVisibility;

        await database.SaveChangesAsync().ConfigureAwait(false);

        context.Operation().Items.Set(new Post_InvalidatePost(postId));
        context.Operation().Items.Set(new Post_InvalidateRecentPost(true));

        return newVisibility;
    }
}