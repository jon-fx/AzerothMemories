namespace AzerothMemories.WebBlazor.Components;

public sealed class RecentPostsHelper
{
    private readonly IMoaServices _services;
    private RecentPostsResults _searchResults;

    public RecentPostsHelper(IMoaServices services)
    {
        _services = services;
        _searchResults = new RecentPostsResults();

        IsLoading = true;
    }

    public RecentPostsResults SearchResults => _searchResults;

    public bool IsLoading { get; private set; }

    public void SetSearchResults(RecentPostsResults recentPostsResults)
    {
        _searchResults = recentPostsResults;

        IsLoading = false;
    }

    public async Task<RecentPostsResults> ComputeState(int currentPage, PostSortMode sortMode, RecentPostsType recentPostType)
    {
        if (currentPage == _searchResults.CurrentPage && sortMode == _searchResults.SortMode && recentPostType == _searchResults.PostsType && _searchResults.TotalPages > 0)
        {
            return _searchResults;
        }

        currentPage = Math.Clamp(currentPage, 0, _searchResults.TotalPages);

        IsLoading = true;

        _searchResults = await _services.ComputeServices.SearchServices.TryGetRecentPosts(Session.Default, recentPostType, sortMode, currentPage, ServerSideLocaleExt.GetServerSideLocale());

        IsLoading = false;

        return _searchResults;
    }

    public async Task OnTryChangeShowAll(bool showAll)
    {
        var newValue = showAll ? RecentPostsType.Two : RecentPostsType.Default;

        await NavigateToNewQuery(newValue, _searchResults.SortMode, _searchResults.CurrentPage, false);
    }

    public async Task OnTryChangePage(int currentPage)
    {
        await NavigateToNewQuery(_searchResults.PostsType, _searchResults.SortMode, currentPage, false);
    }

    private async Task NavigateToNewQuery(RecentPostsType recentPostType, PostSortMode sortMode, int currentPage, bool resetPage)
    {
        var dictionary = new Dictionary<string, object>();

        ZExtensions.AddToDictOrNull(dictionary, "sort", (int)sortMode, sortMode == 0);
        ZExtensions.AddToDictOrNull(dictionary, "page", currentPage, currentPage <= 1 || resetPage);
        ZExtensions.AddToDictOrNull(dictionary, "type", (int)recentPostType, recentPostType == 0);

        var oldPath = _services.ClientServices.NavigationManager.Uri;
        var newPath = _services.ClientServices.NavigationManager.GetUriWithQueryParameters(dictionary);
        if (newPath == oldPath)
        {
            return;
        }

        _services.ClientServices.NavigationManager.NavigateTo(newPath);

        await ComputeState(currentPage, sortMode, recentPostType);
    }
}