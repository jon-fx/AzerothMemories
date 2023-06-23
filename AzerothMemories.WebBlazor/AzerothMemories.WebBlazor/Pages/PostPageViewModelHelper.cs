namespace AzerothMemories.WebBlazor.Pages;

public sealed class PostPageViewModelHelper
{
    private readonly IMoaServices _services;

    private bool _scrollToFocus;
    private int _lastScrollToFocusId;
    private PostCommentTreeNode _focusedNode;

    private readonly Dictionary<int, PostCommentTreeNode> _allCommentTreeNodes = new();

    public PostPageViewModelHelper(IMoaServices services)
    {
        _services = services;
    }

    public string ErrorMessage { get; private set; }

    public AccountViewModel AccountViewModel { get; private set; }

    public PostViewModel PostViewModel { get; private set; }

    public int Page { get; private set; }

    public int TotalPages { get; private set; }

    public List<PostCommentTreeNode> RootComments { get; private set; }

    public PostCommentPageViewModel PostCommentPageViewModel { get; private set; }

    public Dictionary<int, PostCommentReactionViewModel> CommentReactions { get; private set; } = new();

    public void SetErrorMessage(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }

    public async Task<AccountViewModel> UpdateAccount(string accountString)
    {
        int.TryParse(accountString, out var accountId);

        AccountViewModel = null;

        if (accountId > 0)
        {
            AccountViewModel = await _services.ComputeServices.AccountServices.TryGetAccountById(_services.ClientServices.ActiveAccountServices.ActiveSession, accountId);
        }
        else if (!string.IsNullOrWhiteSpace(accountString))
        {
            AccountViewModel = await _services.ComputeServices.AccountServices.TryGetAccountByUsername(_services.ClientServices.ActiveAccountServices.ActiveSession, accountString);
        }

        if (AccountViewModel == null)
        {
            ErrorMessage = "Invalid Account";
        }
        else
        {
            ErrorMessage = null;
        }

        return AccountViewModel;
    }

    public void SetAccountViewModel(AccountViewModel accountViewModel)
    {
        AccountViewModel = accountViewModel;
    }

    public async Task<PostViewModel> UpdatePost(string postIdString)
    {
        int.TryParse(postIdString, out var postId);

        PostViewModel = null;

        if (AccountViewModel != null)
        {
            PostViewModel = await _services.ComputeServices.PostServices.TryGetPostViewModel(_services.ClientServices.ActiveAccountServices.ActiveSession, AccountViewModel.Id, postId, ServerSideLocaleExt.GetServerSideLocale());

            if (PostViewModel == null)
            {
                ErrorMessage = "Post Not Found";
            }
            else
            {
                ErrorMessage = null;
            }
        }

        return PostViewModel;
    }

    public void SetPostViewModel(PostViewModel postViewModel)
    {
        PostViewModel = postViewModel;
    }

    public async Task<PostCommentPageViewModel> UpdateComments(string pageString, string focusedCommentIdString)
    {
        if (AccountViewModel == null)
        {
            return null;
        }

        if (PostViewModel == null)
        {
            return null;
        }

        if (!int.TryParse(pageString, out var currentPage))
        {
            currentPage = 0;
        }

        if (!int.TryParse(focusedCommentIdString, out var focusedCommentId))
        {
            focusedCommentId = 0;
        }

        CommentReactions = await _services.ComputeServices.PostServices.TryGetMyCommentReactions(_services.ClientServices.ActiveAccountServices.ActiveSession, PostViewModel.Id);
        PostCommentPageViewModel = await _services.ComputeServices.PostServices.TryGetCommentsPage(_services.ClientServices.ActiveAccountServices.ActiveSession, PostViewModel.Id, currentPage, focusedCommentId);

        await UpdatePostCommentPageViewModel(pageString, focusedCommentIdString);

        return PostCommentPageViewModel;
    }

    public void SetPostCommentPageViewModel(string pageString, string focusedCommentIdString, PostCommentPageViewModel postCommentPageViewModel)
    {
        PostCommentPageViewModel = postCommentPageViewModel;

        UpdatePostCommentPageViewModel(pageString, focusedCommentIdString).AndForget();
    }

    private async Task UpdatePostCommentPageViewModel(string pageString, string focusedCommentIdString)
    {
        if (PostCommentPageViewModel == null)
        {
            return;
        }

        if (!int.TryParse(pageString, out var currentPage))
        {
            currentPage = 0;
        }

        if (!int.TryParse(focusedCommentIdString, out var focusedCommentId))
        {
            focusedCommentId = 0;
        }

        var rootComments = new List<PostCommentTreeNode>();
        foreach (var comment in PostCommentPageViewModel.AllComments)
        {
            if (!_allCommentTreeNodes.TryGetValue(comment.Key, out var treeNode))
            {
                _allCommentTreeNodes.Add(comment.Key, treeNode = new PostCommentTreeNode(PostViewModel.AccountId, PostViewModel.Id, comment.Key));
            }

            treeNode.Comment = comment.Value;

            if (comment.Value.ParentId == 0)
            {
                if (rootComments.Contains(treeNode))
                {
                }
                else
                {
                    rootComments.Add(treeNode);
                }
            }
            else
            {
                if (_allCommentTreeNodes.TryGetValue(comment.Value.ParentId, out var parentNode))
                {
                    treeNode.Parent = parentNode;

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

            if (CommentReactions.TryGetValue(comment.Key, out var reactionViewModel))
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

        if (currentPage == 0 && _focusedNode != null && _lastScrollToFocusId != _focusedNode.Id)
        {
            var parentNode = _focusedNode.Parent;
            while (parentNode != null)
            {
                parentNode.ShowChildren = true;
                parentNode = parentNode.Parent;
            }

            _scrollToFocus = true;
            _lastScrollToFocusId = _focusedNode.Id;
        }

        Page = PostCommentPageViewModel.Page;
        TotalPages = PostCommentPageViewModel.TotalPages;
        RootComments = rootComments;
    }

    public async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_scrollToFocus)
        {
            _scrollToFocus = false;

            await _services.ClientServices.ScrollManager.ScrollToFragmentAsync($"moa-top-comment-{_focusedNode.Id}", ScrollBehavior.Smooth);
        }
    }
}