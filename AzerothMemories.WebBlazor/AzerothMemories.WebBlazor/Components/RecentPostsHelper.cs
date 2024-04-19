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

    public bool IsLoading { get; private set; }

    public void SetSearchResults(RecentPostsResults recentPostsResults)
    {
        _searchResults = recentPostsResults;

        IsLoading = false;
    }

    public async Task<RecentPostsResults> ComputeState(string currentPageString, string sortModeString, string postTypeString)
    {
        if (int.TryParse(currentPageString, out var currentPage) && currentPage != 0)
        {
            if (_searchResults.PostViewModels == null || _searchResults.PostViewModels.Length == 0)
            {
            }
            else
            {
                currentPage = Math.Clamp(currentPage, 1, _searchResults.TotalPages);
            }
        }
        else
        {
            currentPage = 0;
        }

        RecentPostsType recentPostType;
        if (int.TryParse(postTypeString, out var typeInt) && Enum.IsDefined(typeof(RecentPostsType), typeInt))
        {
            recentPostType = (RecentPostsType)typeInt;
        }
        else
        {
            recentPostType = RecentPostsType.Default;
        }

        PostSortMode sortMode;
        if (int.TryParse(sortModeString, out var sortModeInt) && Enum.IsDefined(typeof(PostSortMode), sortModeInt))
        {
            sortMode = (PostSortMode)sortModeInt;
        }
        else
        {
            sortMode = PostSortMode.PostTimeStampDescending;
        }

        IsLoading = true;

        _searchResults = await _services.ComputeServices.SearchServices.TryGetRecentPosts(Session.Default, recentPostType, sortMode, currentPage, ServerSideLocaleExt.GetServerSideLocale());

        IsLoading = false;

        return _searchResults;
    }

    public void OnTryChangeShowAll(bool showAll)
    {
        var newValue = showAll ? RecentPostsType.Two : RecentPostsType.Default;
        if (_searchResults.PostsType == newValue)
        {
            return;
        }

        NavigateToNewQuery(newValue, _searchResults.SortMode, _searchResults.CurrentPage, false);
    }

    public void OnTryChangePage(int currentPage)
    {
        if (_searchResults.CurrentPage == currentPage)
        {
            return;
        }

        NavigateToNewQuery(_searchResults.PostsType, _searchResults.SortMode, currentPage, false);
    }

    private void NavigateToNewQuery(RecentPostsType recentPostType, PostSortMode sortMode, int currentPage, bool resetPage)
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
    }
}