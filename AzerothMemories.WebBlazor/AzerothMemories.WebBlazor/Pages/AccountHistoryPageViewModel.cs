namespace AzerothMemories.WebBlazor.Pages;

public sealed class AccountHistoryPageViewModel : ViewModelBase, IViewModel<AccountHistoryPageViewModel>
{
    private AccountHistoryPageResult? _searchResults;
    private string? _currentPageString;

    private AccountHistoryPageViewModel(IMoaServices services, Action onViewModelChanged) : base(services, onViewModelChanged)
    {
        _searchResults = new AccountHistoryPageResult();
    }

    public int CurrentPage => _searchResults?.CurrentPage ?? 0;

    public int TotalPages => _searchResults?.TotalPages ?? 0;

    public AccountHistoryViewModel[]? HistoryViewModels => _searchResults?.ViewModels;

    public bool NoResults => _searchResults == null || _searchResults.ViewModels.Length == 0;

    public void OnParametersChanged(string? currentPageString)
    {
        _currentPageString = currentPageString;
    }

    public override async Task ComputeState(CancellationToken cancellationToken)
    {
        await base.ComputeState(cancellationToken);

        if (int.TryParse(_currentPageString, out var currentPage) && currentPage != 0)
        {
            if (NoResults)
            {
            }
            else
            {
                currentPage = Math.Clamp(currentPage, 1, TotalPages);
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