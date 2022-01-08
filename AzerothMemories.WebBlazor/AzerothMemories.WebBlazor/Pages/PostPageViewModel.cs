using System.Globalization;

namespace AzerothMemories.WebBlazor.Pages;

public sealed class PostPageViewModel : ViewModelBase
{
    private readonly Dictionary<long, PostCommentTreeNode> _allCommentTreeNodes;

    private long _accountId;
    private string _username;
    private long _postId;

    public PostPageViewModel()
    {
        _allCommentTreeNodes = new Dictionary<long, PostCommentTreeNode>();
    }

    public string ErrorMessage { get; private set; }

    public AccountViewModel AccountViewModel { get; private set; }

    public PostViewModel PostViewModel { get; private set; }

    public PostCommentPageViewModel PostCommentPageViewModel { get; private set; }

    //public Dictionary<long, PostCommentReactionViewModel> CommentReactions { get; private set; }

    public int CurrentPage { get; private set; }

    public long FocusedCommentId { get; private set; }

    public void OnParametersSet(long id, string name, long postId, string pageString, string focusedCommentIdString)
    {
        _accountId = id;
        _username = name;
        _postId = postId;

        if (!int.TryParse(pageString, out var currentPage))
        {
            currentPage = 1;
        }

        if (!long.TryParse(focusedCommentIdString, out var focusedCommentId))
        {
            focusedCommentId = 0;
        }

        CurrentPage = currentPage;
        FocusedCommentId = focusedCommentId;
    }

    public override async Task ComputeState(CancellationToken cancellationToken)
    {
        if (_accountId > 0)
        {
            AccountViewModel = await Services.AccountServices.TryGetAccountById(null, _accountId, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(_username))
        {
            AccountViewModel = await Services.AccountServices.TryGetAccountByUsername(null, _username, cancellationToken);
        }

        if (AccountViewModel != null && _postId > 0)
        {
            PostViewModel = await Services.PostServices.TryGetPostViewModel(null, AccountViewModel.Id, _postId, CultureInfo.CurrentCulture.Name, cancellationToken);
        }

        if (AccountViewModel == null)
        {
            ErrorMessage = "Invalid Account";
        }
        else if (PostViewModel == null)
        {
            ErrorMessage = "Post Not Found";
        }
        else
        {
            ErrorMessage = null;
        }

        if (PostViewModel != null)
        {
            var commentReactions = await Services.PostServices.TryGetMyCommentReactions(null, _postId);
            var pageViewModel = await Services.PostServices.TryGetCommentsPage(null, _postId, CurrentPage, FocusedCommentId);

            foreach (var comment in pageViewModel.AllComments)
            {
                if (!_allCommentTreeNodes.TryGetValue(comment.Key, out var treeNode))
                {
                    _allCommentTreeNodes.Add(comment.Key, treeNode = new PostCommentTreeNode(PostViewModel.AccountId, _postId, comment.Key));
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
            }

            PostCommentPageViewModel = pageViewModel;
        }
    }
}