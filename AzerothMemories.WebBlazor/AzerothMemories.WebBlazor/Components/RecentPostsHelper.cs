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

    public int CurrentPage => _searchResults.CurrentPage;

    public int TotalPages
    {
        get
        {
            if (_searchResults.PostInfos.Length > 0)
            {
                return (int)Math.Ceiling(_searchResults.PostInfos.Length / (float)ZExtensions.PostsPerPage);
            }

            return 1;
        }
    }

    public int StartIndex => Math.Clamp((CurrentPage - 1) * ZExtensions.PostsPerPage, 0, _searchResults.PostInfos.Length);

    public int EndIndex => Math.Clamp(StartIndex + ZExtensions.PostsPerPage, 0, _searchResults.PostInfos.Length);

    public void SetSearchResults(RecentPostsResults? recentPostsResults)
    {
        _searchResults = recentPostsResults ?? new RecentPostsResults();

        IsLoading = false;
    }

    public async Task<RecentPostsResults> ComputeState(int currentPage, PostSortMode sortMode, RecentPostType recentPostType)
    {
        currentPage = Math.Clamp(currentPage, 1, Math.Max(TotalPages, 1));

        //if (currentPage == CurrentPage && sortMode == _searchResults.SortMode && recentPostType == _searchResults.PostType && _searchResults.PostInfos.Length > 0 && _searchResults.PostInfos.Length == _searchResults.PostViewModels.Length)
        //{
        //    return _searchResults;
        //}

        IsLoading = true;

        var oldSearchResults = _searchResults;
        var oldViewModels = oldSearchResults.PostViewModels.SafeEnumerable().ToDictionary(x => x.Id, x => x);

        _searchResults = await _services.ComputeServices.SearchServices.TryGetRecentPosts(Session.Default, recentPostType, sortMode);

        var temp = _searchResults.PostViewModels;
        Array.Resize(ref temp, _searchResults.PostInfos.Length);

        _searchResults.PostViewModels = temp;
        _searchResults.CurrentPage = Math.Clamp(currentPage, 1, Math.Max(TotalPages, 1));

        for (var i = 0; i < _searchResults.PostInfos.Length; i++)
        {
            var postId = _searchResults.PostInfos[i].PostId;

            oldViewModels.TryGetValue(postId, out var postViewModel);

            _searchResults.PostViewModels[i] = postViewModel;
        }

        IsLoading = false;

        return _searchResults;
    }

    public async Task OnTryChangeShowAll(bool showAll)
    {
        var newValue = showAll ? RecentPostType.Two : RecentPostType.Default;

        await NavigateToNewQuery(newValue, _searchResults.SortMode, CurrentPage, false);
    }

    public async Task OnTryChangePage(int currentPage)
    {
        await NavigateToNewQuery(_searchResults.PostType, _searchResults.SortMode, currentPage, false);
    }

    private async Task NavigateToNewQuery(RecentPostType recentPostType, PostSortMode sortMode, int currentPage, bool resetPage)
    {
        var dictionary = new Dictionary<string, object?>();

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