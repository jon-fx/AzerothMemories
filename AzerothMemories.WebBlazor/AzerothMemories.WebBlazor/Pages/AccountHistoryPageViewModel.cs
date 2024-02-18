namespace AzerothMemories.WebBlazor.Pages;

public sealed class AccountHistoryPageViewModel : ViewModelBase, IViewModel<AccountHistoryPageViewModel>
{
    private AccountHistoryPageResult _searchResults;
    private string _currentPageString;

    public AccountHistoryPageViewModel(IMoaServices services, Action onViewModelChanged) : base(services, onViewModelChanged)
    {
        _searchResults = new AccountHistoryPageResult();
    }

    public int CurrentPage => _searchResults.CurrentPage;

    public int TotalPages => _searchResults.TotalPages;

    public AccountHistoryViewModel[] HistoryViewModels => _searchResults?.ViewModels;

    public bool NoResults => _searchResults.ViewModels.Length == 0;

    public void OnParametersChanged(string currentPageString)
    {
        _currentPageString = currentPageString;
    }

    public override async Task ComputeState(CancellationToken cancellationToken)
    {
        await base.ComputeState(cancellationToken);

        if (int.TryParse(_currentPageString, out var currentPage) && currentPage > 0)
        {
            if (NoResults)
            {
            }
            else
            {
                currentPage = Math.Clamp(currentPage, 1, currentPage);
            }
        }

        _searchResults = await Services.ComputeServices.AccountServices.TryGetAccountHistory(Session.Default, currentPage);
    }

    public void TryChangePage(int currentPage)
    {
        if (currentPage == CurrentPage)
        {
            return;
        }

        var newPath = Services.ClientServices.NavigationManager.GetUriWithQueryParameter("page", currentPage);
        Services.ClientServices.NavigationManager.NavigateTo(newPath);
    }

    public static AccountHistoryPageViewModel CreateViewModel(IMoaServices services, Action onViewModelChanged)
    {
        return new AccountHistoryPageViewModel(services, onViewModelChanged);
    }
}