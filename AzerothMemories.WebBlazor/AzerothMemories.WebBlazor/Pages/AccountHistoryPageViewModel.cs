namespace AzerothMemories.WebBlazor.Pages;

public sealed class AccountHistoryPageViewModel : ViewModelBase
{
    private AccountHistoryPageResult _searchResults;

    public AccountHistoryPageViewModel()
    {
        _searchResults = new AccountHistoryPageResult();
    }

    public int CurrentPage => _searchResults.CurrentPage;

    public int TotalPages => _searchResults.TotalPages;

    public AccountHistoryViewModel[] HistoryViewModels => _searchResults?.ViewModels;

    public bool NoResults => _searchResults.ViewModels.Length == 0;

    public async Task ComputeState(string currentPageString)
    {
        if (int.TryParse(currentPageString, out var currentPage) && currentPage > 0)
        {
            if (NoResults)
            {
            }
            else
            {
                currentPage = Math.Clamp(currentPage, 1, currentPage);
            }
        }

        var searchResults = await Services.ComputeServices.AccountServices.TryGetAccountHistory(null, currentPage);

        _searchResults = searchResults;
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
}