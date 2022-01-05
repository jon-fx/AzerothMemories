namespace AzerothMemories.WebBlazor.Pages
{
    public sealed class PostPageViewModel : ViewModelBase
    {
        private long _accountId;
        private string _username;
        private long _postId;

        public PostPageViewModel()
        {
        }

        public string ErrorMessage { get; private set; }

        public AccountViewModel AccountViewModel { get; private set; }

        public PostViewModel PostViewModel { get; private set; }

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
                PostViewModel = await Services.PostServices.TryGetPostViewModel(null, AccountViewModel.Id, _postId, cancellationToken);
            }

            if (AccountViewModel == null)
            {
                ErrorMessage = "Invalid Account";
            }
            else if (AccountViewModel == null)
            {
                ErrorMessage = "Post Not Found";
            }
            else
            {
                ErrorMessage = null;
            }
        }
    }
}