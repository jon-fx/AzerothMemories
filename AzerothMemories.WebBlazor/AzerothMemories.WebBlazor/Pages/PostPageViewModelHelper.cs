namespace AzerothMemories.WebBlazor.Pages;

public sealed class PostPageViewModelHelper
{
    private readonly IMoaServices _services;

    private bool _scrollToFocus;
    private PostCommentTreeNode _focusedNode;

    public PostPageViewModelHelper(IMoaServices services)
    {
        _services = services;
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
            accountViewModel = await _services.AccountServices.TryGetAccountById(null, accountId);
        }
        else if (!string.IsNullOrWhiteSpace(username))
        {
            accountViewModel = await _services.AccountServices.TryGetAccountByUsername(null, username);
        }

        if (accountViewModel == null)
        {
            ErrorMessage = "Invalid Account";
            return;
        }

        var postViewModel = await _services.PostServices.TryGetPostViewModel(null, accountViewModel.Id, postId, CultureInfo.CurrentCulture.Name);
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

        var commentReactions = await _services.PostServices.TryGetMyCommentReactions(null, postId);
        var pageViewModel = await _services.PostServices.TryGetCommentsPage(null, postId, currentPage, focusedCommentId);

        foreach (var comment in pageViewModel.AllComments)
        {
            if (!pageViewModel.AllCommentTreeNodes.TryGetValue(comment.Key, out var treeNode))
            {
                pageViewModel.AllCommentTreeNodes.Add(comment.Key, treeNode = new PostCommentTreeNode(PostViewModel.AccountId, postId, comment.Key));
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
                if (pageViewModel.AllCommentTreeNodes.TryGetValue(comment.Value.ParentId, out var parentNode))
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
                await treeNode.TryLoadReactions(_services);
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
            var parentId = _focusedNode.ParentId;
            while (parentId != 0 && pageViewModel.AllCommentTreeNodes.TryGetValue(parentId, out var parentNode))
            {
                parentNode.ShowChildren = true;
                parentId = parentNode.ParentId;
            }

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