namespace AzerothMemories.WebBlazor.Pages;

public sealed class AccountPageViewModel : PersistentStateViewModel
{
    private string _accountIdString;
    private string _sortModeString;
    private string _currentPageString;

    public AccountPageViewModel()
    {
        AddPersistentState(() => ErrorMessage, x => ErrorMessage = x, () => Task.FromResult<string>(null));
        AddPersistentState(() => AccountViewModel, x => AccountViewModel = x, GetAccountViewModel);
        AddPersistentState(() => PostSearchHelper.SearchResults, x => PostSearchHelper.SetSearchResults(x), UpdateSearchResults);
    }

    public string ErrorMessage { get; private set; }

    public AccountViewModel AccountViewModel { get; private set; }

    public PostSearchHelper PostSearchHelper { get; private set; }

    public bool IsLoading => AccountViewModel == null || PostSearchHelper == null;

    public void OnParametersChanged(string accountIdString, string sortModeString, string currentPageString)
    {
        _accountIdString = accountIdString;
        _sortModeString = sortModeString;
        _currentPageString = currentPageString;
    }

    public override async Task OnInitialized()
    {
        PostSearchHelper = new PostSearchHelper(Services);

        await base.OnInitialized();
    }

    public override async Task ComputeState(CancellationToken cancellationToken)
    {
        await base.ComputeState(cancellationToken);

        AccountViewModel = await GetAccountViewModel();

        await UpdateSearchResults();
    }

    private async Task<AccountViewModel> GetAccountViewModel()
    {
        int.TryParse(_accountIdString, out var accountId);

        var accountViewModel = AccountViewModel;
        if (accountId > 0)
        {
            accountViewModel = await Services.ComputeServices.AccountServices.TryGetAccountById(null, accountId);
        }
        else if (!string.IsNullOrWhiteSpace(_accountIdString))
        {
            accountViewModel = await Services.ComputeServices.AccountServices.TryGetAccountByUsername(null, _accountIdString);
        }

        if (accountViewModel == null)
        {
            ErrorMessage = "Invalid Account";
        }

        return accountViewModel;
    }

    private Task<SearchPostsResults> UpdateSearchResults()
    {
        if (AccountViewModel == null)
        {
            return Task.FromResult(new SearchPostsResults());
        }

        var accountTag = new PostTagInfo(PostTagType.Account, AccountViewModel.Id, AccountViewModel.Username, AccountViewModel.Avatar);
        return PostSearchHelper.ComputeState(new[] { accountTag.TagString }, _sortModeString, _currentPageString, null, null);
    }
}