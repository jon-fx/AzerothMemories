namespace AzerothMemories.WebBlazor.Components;

public sealed class RecentPostsHelper
{
    private readonly IMoaServices _services;

    private RecentPostsResults _searchResults;

    private int _currentPage;
    private PostSortMode _sortMode;
    private RecentPostsType _recentPostType;

    public RecentPostsHelper(IMoaServices services)
    {
        _services = services;
        _searchResults = new RecentPostsResults();

        IsLoading = true;
    }

    public bool NoResults => _searchResults.PostViewModels.Length == 0;

    public int CurrentPage => _searchResults.CurrentPage;

    public int TotalPages => _searchResults.TotalPages;

    public RecentPostsType CurrentType => _searchResults.PostsType;

    public PostViewModel[] CurrentPosts => _searchResults.PostViewModels;

    public RecentPostsResults SearchResults => _searchResults;

    public bool IsLoading { get; private set; }

    public void SetSearchResults(RecentPostsResults recentPostsResults)
    {
        _searchResults = recentPostsResults;
        _currentPage = _searchResults.CurrentPage;
        _recentPostType = _searchResults.PostsType;
        _sortMode = _searchResults.SortMode;

        IsLoading = false;
    }

    public async Task<RecentPostsResults> ComputeState(string currentPageString, string sortModeString, string postTypeString)
    {
        if (int.TryParse(currentPageString, out _currentPage) && _currentPage > 0)
        {
            if (NoResults)
            {
            }
            else
            {
                _currentPage = Math.Clamp(_currentPage, 1, _currentPage);
            }
        }

        _recentPostType = RecentPostsType.Default;
        if (int.TryParse(postTypeString, out var typeInt) && Enum.IsDefined(typeof(RecentPostsType), typeInt))
        {
            _recentPostType = (RecentPostsType)typeInt;
        }

        _sortMode = PostSortMode.PostTimeStampDescending;
        if (int.TryParse(sortModeString, out var sortModeInt) && Enum.IsDefined(typeof(PostSortMode), sortModeInt))
        {
            _sortMode = (PostSortMode)sortModeInt;
        }

        IsLoading = true;

        var searchResults = await _services.ComputeServices.SearchServices.TryGetRecentPosts(null, _recentPostType, _sortMode, _currentPage, ServerSideLocaleExt.GetServerSideLocale());

        _searchResults = searchResults;

        IsLoading = false;

        return _searchResults;
    }

    public void OnShowAllChanged(bool showAll)
    {
        var newValue = showAll ? RecentPostsType.Two : RecentPostsType.Default;
        if (_recentPostType == newValue)
        {
            return;
        }

        if (_searchResults.PostsType == newValue)
        {
            return;
        }

        _recentPostType = newValue;

        NavigateToNewQuery(false);
    }

    public void TryChangePage(int currentPage)
    {
        if (_currentPage == currentPage)
        {
            return;
        }

        if (_searchResults.CurrentPage == currentPage)
        {
            return;
        }

        _currentPage = currentPage;

        NavigateToNewQuery(false);
    }

    private void NavigateToNewQuery(bool resetPage)
    {
        var dictionary = new Dictionary<string, object>();

        ZExtensions.AddToDictOrNull(dictionary, "sort", (int)_sortMode, _sortMode == 0);
        ZExtensions.AddToDictOrNull(dictionary, "page", _currentPage, _currentPage <= 1 || resetPage);
        ZExtensions.AddToDictOrNull(dictionary, "type", (int)_recentPostType, _recentPostType == 0);

        var oldPath = _services.ClientServices.NavigationManager.Uri;
        var newPath = _services.ClientServices.NavigationManager.GetUriWithQueryParameters(dictionary);
        if (newPath == oldPath)
        {
            return;
        }

        _services.ClientServices.NavigationManager.NavigateTo(newPath);
    }
}