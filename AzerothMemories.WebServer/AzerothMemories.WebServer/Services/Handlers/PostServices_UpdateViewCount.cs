//namespace AzerothMemories.WebServer.Services.Handlers;

//internal sealed class PostServices_UpdateViewCount
//{
//    public static async Task<PostRecord> TryHandle(CommonServices commonServices, Post_UpdateViewCount command, CancellationToken cancellationToken)
//    {
//        var context = CommandContext.GetCurrent();
//        if (Computed.IsInvalidating())
//        {
//            var invPost = context.Operation().Items.Get<Post_InvalidatePost>();
//            if (invPost != null && invPost.PostId > 0)
//            {
//                _ = commonServices.PostServices.DependsOnPost(invPost.PostId);
//            }

//            return default;
//        }

//        var postRecord = await commonServices.PostServices.TryGetPostRecord(command.PostId).ConfigureAwait(false);
//        if (postRecord == null)
//        {
//            return null;
//        }

//        await using var database = await commonServices.DatabaseHub.CreateCommandDbContext(cancellationToken).ConfigureAwait(false);
//        database.Attach(postRecord);

//        postRecord.TotalViewCount++;

//        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

//        context.Operation().Items.Set(new Post_InvalidatePost(command.PostId));

//        return postRecord;
//    }
//}