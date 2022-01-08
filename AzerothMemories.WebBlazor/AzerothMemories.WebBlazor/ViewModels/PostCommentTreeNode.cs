namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class PostCommentTreeNode
{
    public readonly long Id;
    public readonly long PostersAccountId;

    public long ReactionId;
    public PostReaction Reaction;

    public bool IsFocused;
    public PostCommentViewModel Comment;

    public bool ShowChildren;
    public bool ShowReactions;
    public List<PostCommentTreeNode> Children = new();

    public PostCommentTreeNode(long postersAccountId, long postId, long commentId)
    {
        Id = commentId;
        PostersAccountId = postersAccountId;
    }

    public long ParentId => Comment.ParentId;

    public bool HasChild => Children.Count > 0;
}