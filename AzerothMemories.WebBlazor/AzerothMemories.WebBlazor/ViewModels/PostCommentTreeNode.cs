namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class PostCommentTreeNode
{
    public readonly long Id;
    public readonly long PostId;
    public readonly long PostersAccountId;

    public long ReactionId;
    public PostReaction Reaction;

    public bool IsFocused;
    public PostCommentViewModel Comment;

    public bool ShowChildren;
    public bool ShowReactions;
    public bool ShowReactionIsLoading;
    public PostReactionViewModel[] ReactionData;

    public List<PostCommentTreeNode> Children = new();

    public PostCommentTreeNode(long postersAccountId, long postId, long commentId)
    {
        Id = commentId;
        PostId = postId;
        PostersAccountId = postersAccountId;
    }

    public long ParentId => Comment.ParentId;

    public bool HasChild => Children.Count > 0;

    internal async Task TryLoadReactions(IMoaServices services)
    {
        if (!ShowReactions)
        {
            return;
        }

        if (ShowReactionIsLoading)
        {
            return;
        }

        ShowReactionIsLoading = true;

        var reactionData = await services.PostServices.TryGetCommentReactionData(null, PostId, Id);
        if (reactionData == null)
        {
            ReactionData = null;
        }
        else if (reactionData.Length == 0)
        {
            ReactionData = Array.Empty<PostReactionViewModel>();
        }
        else
        {
            ReactionData = reactionData.OrderBy(x => x.LastUpdateTime).ToArray();
        }

        ShowReactionIsLoading = false;
    }
}