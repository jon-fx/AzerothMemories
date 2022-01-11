namespace AzerothMemories.WebBlazor.Pages;

public sealed class PostPageViewModel : ViewModelBase
{
    private readonly Dictionary<long, PostCommentTreeNode> _allCommentTreeNodes;

    public PostPageViewModel()
    {
        _allCommentTreeNodes = new Dictionary<long, PostCommentTreeNode>();
    }

    public string ErrorMessage { get; private set; }

    public AccountViewModel AccountViewModel { get; private set; }

    public PostViewModel PostViewModel { get; private set; }

    public PostCommentPageViewModel PostCommentPageViewModel { get; private set; }

    //public int CurrentPage { get; private set; }

    //public long FocusedCommentId { get; private set; }

    public async Task ComputeState(long accountId, string username, long postId, string pageString, string focusedCommentIdString)
    {
        var accountViewModel = AccountViewModel;
        if (accountId > 0)
        {
            accountViewModel = await Services.AccountServices.TryGetAccountById(null, accountId);
        }
        else if (!string.IsNullOrWhiteSpace(username))
        {
            accountViewModel = await Services.AccountServices.TryGetAccountByUsername(null, username);
        }

        if (accountViewModel == null)
        {
            ErrorMessage = "Invalid Account";
            return;
        }

        var postViewModel = await Services.PostServices.TryGetPostViewModel(null, accountViewModel.Id, postId, CultureInfo.CurrentCulture.Name);
        if (postViewModel == null)
        {
            ErrorMessage = "Post Not Found";
            return;
        }

        if (!int.TryParse(pageString, out var currentPage))
        {
            currentPage = 1;
        }

        if (!long.TryParse(focusedCommentIdString, out var focusedCommentId))
        {
            focusedCommentId = 0;
        }

        //CurrentPage = currentPage;
        //FocusedCommentId = focusedCommentId;

        AccountViewModel = accountViewModel;
        PostViewModel = postViewModel;

        var commentReactions = await Services.PostServices.TryGetMyCommentReactions(null, postId);
        var pageViewModel = await Services.PostServices.TryGetCommentsPage(null, postId, currentPage, focusedCommentId);

        foreach (var comment in pageViewModel.AllComments)
        {
            if (!_allCommentTreeNodes.TryGetValue(comment.Key, out var treeNode))
            {
                _allCommentTreeNodes.Add(comment.Key, treeNode = new PostCommentTreeNode(PostViewModel.AccountId, postId, comment.Key));
            }

            treeNode.Comment = comment.Value;

            if (comment.Value.ParentId == 0)
            {
                if (pageViewModel.RootComments.Contains(treeNode))
                {
                }
                else
                {
                    pageViewModel.RootComments.Add(treeNode);
                }
            }
            else
            {
                if (_allCommentTreeNodes.TryGetValue(comment.Value.ParentId, out var parentNode))
                {
                    if (parentNode.Children.Contains(treeNode))
                    {
                    }
                    else
                    {
                        parentNode.Children.Add(treeNode);
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            if (commentReactions.TryGetValue(comment.Key, out var reactionViewModel))
            {
                treeNode.Reaction = reactionViewModel.Reaction;
                treeNode.ReactionId = reactionViewModel.Id;
            }

            if (treeNode.ShowReactions)
            {
                await treeNode.TryLoadReactions(Services);
            }
        }

        PostCommentPageViewModel = pageViewModel;
    }
}