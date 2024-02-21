namespace AzerothMemories.WebBlazor.Pages;

public sealed class AccountPageViewModel : PersistentStateViewModel, IViewModel<AccountPageViewModel>, IPageHeaderInfoProvider
{
    private string _accountIdString;
    private string _sortModeString;
    private string _currentPageString;

    public AccountPageViewModel(IMoaServices services, Action onViewModelChanged) : base(services, onViewModelChanged)
    {
        PostSearchHelper = new PostSearchHelper(Services);

        AddPersistentState(() => ErrorMessage, x => ErrorMessage = x, () => Task.FromResult<string>(null));
        AddPersistentState(() => AccountViewModel, x => AccountViewModel = x, GetAccountViewModel);
        AddPersistentState(() => PostSearchHelper.SearchResults, x => PostSearchHelper.SetSearchResults(x), UpdateSearchResults);
    }

    public string ErrorMessage { get; private set; }

    public AccountViewModel AccountViewModel { get; private set; }

    public PostSearchHelper PostSearchHelper { get; }

    public bool IsLoading => AccountViewModel == null || PostSearchHelper == null;

    public void OnParametersChanged(string accountIdString, string sortModeString, string currentPageString)
    {
        _accountIdString = accountIdString;
        _sortModeString = sortModeString;
        _currentPageString = currentPageString;
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
            accountViewModel = await Services.ComputeServices.AccountServices.TryGetAccountById(Session.Default, accountId);
        }
        else if (!string.IsNullOrWhiteSpace(_accountIdString))
        {
            accountViewModel = await Services.ComputeServices.AccountServices.TryGetAccountByUsername(Session.Default, _accountIdString);
        }

        if (accountViewModel == null)
        {
            ErrorMessage = "Invalid Account";
        }
        else
        {
            ErrorMessage = null;
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

    public string GetPageTitle()
    {
        return $"{AccountViewModel.GetDisplayName()}'s Memories of Azeroth";
    }

    public string GetPageDescription()
    {
        var name = AccountViewModel.GetDisplayName();
        var totalPostCount = AccountViewModel.TotalPostCount;
        var totalMemoriesCount = AccountViewModel.TotalPostCount + AccountViewModel.TotalMemoriesCount;

        return $"A collection of Memories of Azeroth from the account {name}. {name} has {totalPostCount.ToMetric()} posts and {totalMemoriesCount.ToMetric()} memories.";
    }

    public string GetPageImage()
    {
        return AccountViewModel.Avatar;
    }

    public string GetPageImageAlt()
    {
        return $"{AccountViewModel.GetDisplayName()}'s Avatar";
    }

    public string GetCanonicalLink()
    {
        return $"account/{AccountViewModel.Id}";
    }

    public static AccountPageViewModel CreateViewModel(IMoaServices services, Action onViewModelChanged)
    {
        return new AccountPageViewModel(services, onViewModelChanged);
    }
}