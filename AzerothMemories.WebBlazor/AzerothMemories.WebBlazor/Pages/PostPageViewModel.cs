namespace AzerothMemories.WebBlazor.Pages;

public sealed class PostPageViewModel : ViewModelBase
{
    private readonly Dictionary<long, PostCommentTreeNode> _allCommentTreeNodes;

    private bool _scrollToFocus;
    private PostCommentTreeNode _focusedNode;

    public PostPageViewModel()
    {
        _allCommentTreeNodes = new Dictionary<long, PostCommentTreeNode>();
    }

    public string ErrorMessage { get; private set; }

    public AccountViewModel AccountViewModel { get; private set; }

    public PostViewModel PostViewModel { get; private set; }

    public PostCommentPageViewModel PostCommentPageViewModel { get; private set; }

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
            currentPage = 0;
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

            if (treeNode.Id == focusedCommentId)
            {
                if (_focusedNode != null && _focusedNode != treeNode)
                {
                    _focusedNode.IsFocused = false;
                }

                _focusedNode = treeNode;
                _focusedNode.IsFocused = true;
            }
        }

        if (currentPage == 0 && _focusedNode != null)
        {
            _scrollToFocus = true;
        }

        PostCommentPageViewModel = pageViewModel;
    }

    public async Task OnAfterRenderAsync(IScrollManager scrollManager, bool firstRender)
    {
        if (!firstRender && _scrollToFocus)
        {
            _scrollToFocus = false;

            await scrollManager.ScrollToFragmentAsync($"moa-top-comment-{_focusedNode.Id}", ScrollBehavior.Smooth);
        }
    }
}