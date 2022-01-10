namespace AzerothMemories.WebBlazor.Pages
{
    public sealed class AccountPageViewModel : ViewModelBase
    {
        private long _accountId;
        private string _accountUsername;
        private string _sortModeString;
        private string _currentPageString;

        public AccountPageViewModel()
        {
        }

        public string ErrorMessage { get; private set; }

        public AccountViewModel AccountViewModel { get; private set; }

        public PostSearchHelper PostSearchHelper { get; private set; }

        public bool IsLoading => AccountViewModel == null || PostSearchHelper == null;

        public override Task OnInitialized()
        {
            PostSearchHelper = new PostSearchHelper(Services);

            return Task.CompletedTask;
        }

        public Task OnParametersChanged(long accountId, string accountUsername, string sortModeString, string currentPageString)
        {
            _accountId = accountId;
            _accountUsername = accountUsername;
            _sortModeString = sortModeString;
            _currentPageString = currentPageString;

            return Task.CompletedTask;
        }

        public override async Task ComputeState()
        {
            var accountViewModel = AccountViewModel;
            if (_accountId > 0)
            {
                accountViewModel = await Services.AccountServices.TryGetAccountById(null, _accountId);
            }
            else if (!string.IsNullOrWhiteSpace(_accountUsername))
            {
                accountViewModel = await Services.AccountServices.TryGetAccountByUsername(null, _accountUsername);
            }

            if (accountViewModel == null)
            {
                ErrorMessage = "Invalid Account";
                return;
            }

            AccountViewModel = accountViewModel;

            var accountTag = new PostTagInfo(PostTagType.Account, AccountViewModel.Id, AccountViewModel.Username, AccountViewModel.Avatar);
            await PostSearchHelper.OnParametersChanged(new[] { accountTag.TagString }, _sortModeString, _currentPageString, null, null);
        }
    }
}