namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class PostCommentTreeNode
{
    public readonly int Id;
    public readonly int PostId;
    public readonly int PostersAccountId;

    public int ReactionId;
    public PostReaction Reaction;

    public bool IsFocused;

    public bool ShowChildren;
    public bool ShowReactions;
    public bool ShowReactionIsLoading;
    public PostReactionViewModel[]? ReactionData;

    public PostCommentTreeNode? Parent;
    public readonly List<PostCommentTreeNode> Children = [];

    public PostCommentTreeNode(int postersAccountId, int postId, int commentId)
    {
        Id = commentId;
        PostId = postId;
        PostersAccountId = postersAccountId;
    }

    public required PostCommentViewModel Comment { get; set; }

    public int ParentId => Comment.ParentId;

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

        var reactionData = await services.ComputeServices.PostServices.TryGetCommentReactionData(Session.Default, PostId, Id);
        if (reactionData == null)
        {
            ReactionData = null;
        }
        else if (reactionData.Length == 0)
        {
            ReactionData = [];
        }
        else
        {
            ReactionData = reactionData.OrderBy(x => x.LastUpdateTime).ToArray();
        }

        ShowReactionIsLoading = false;
    }
}