namespace AzerothMemories.WebBlazor.Pages;

public sealed class PostSearchHelper
{
    private readonly IMoaServices _services;

    private SearchPostsResults _searchResults;

    public Instant? MinDateTime;
    public Instant? MaxDateTime;

    public PostSearchHelper(IMoaServices services)
    {
        _services = services;
        _searchResults = new SearchPostsResults();

        IsLoading = true;
    }

    public IMoaServices Services => _services;

    public bool NoResults => _searchResults.PostViewModels.Length == 0;

    public int CurrentPage => _searchResults.CurrentPage;

    public int TotalPages => _searchResults.TotalPages;

    public PostSortMode PostSortMode => _searchResults.SortMode;

    public PostTagInfo[] SelectedSearchTags => _searchResults.Tags;

    public PostViewModel[] CurrentPosts => _searchResults.PostViewModels;

    public bool IsLoading { get; private set; }

    public SearchPostsResults SearchResults => _searchResults;

    public async Task<SearchPostsResults> ComputeState(string[] tagStrings, string sortModeString, string currentPageString, string postMinTimeString, string postMaxTimeString)
    {
        if (int.TryParse(currentPageString, out var currentPage) && currentPage != 0)
        {
            if (NoResults)
            {
            }
            else
            {
                currentPage = Math.Clamp(currentPage, 1, TotalPages);
            }
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

        if (long.TryParse(postMinTimeString, out var minTime))
        {
            minTime = Math.Clamp(minTime, ZExtensions.MinPostTime.ToUnixTimeMilliseconds(), SystemClock.Instance.GetCurrentInstant().ToUnixTimeMilliseconds());
        }

        if (long.TryParse(postMaxTimeString, out var maxTime))
        {
            maxTime = Math.Clamp(maxTime, ZExtensions.MinPostTime.ToUnixTimeMilliseconds(), SystemClock.Instance.GetCurrentInstant().ToUnixTimeMilliseconds());
        }

        IsLoading = true;

        var searchResults = await _services.ComputeServices.SearchServices.TrySearchPosts(Session.Default, tagStrings, sortMode, currentPage, minTime, maxTime, ServerSideLocaleExt.GetServerSideLocale());

        _searchResults = searchResults;

        MinDateTime = _searchResults.MinTime > 0 ? Instant.FromUnixTimeMilliseconds(_searchResults.MinTime) : null;
        MaxDateTime = _searchResults.MaxTime > 0 ? Instant.FromUnixTimeMilliseconds(_searchResults.MaxTime) : null;

        foreach (var info in _searchResults.Tags)
        {
            info.IsChipClosable = true;
        }

        IsLoading = false;

        return _searchResults;
    }

    public void SetSearchResults(SearchPostsResults searchPostsResults)
    {
        _searchResults = searchPostsResults;

        MinDateTime = _searchResults.MinTime > 0 ? Instant.FromUnixTimeMilliseconds(_searchResults.MinTime) : null;
        MaxDateTime = _searchResults.MaxTime > 0 ? Instant.FromUnixTimeMilliseconds(_searchResults.MaxTime) : null;

        IsLoading = false;
    }

    public void OnSortChanged(PostSortMode sortMode)
    {
        if (_searchResults.SortMode == sortMode)
        {
            return;
        }

        NavigateToNewQuery(_searchResults.CurrentPage, _searchResults.SortMode, _searchResults.Tags.Select(x => x.TagString).ToArray(), MinDateTime, MaxDateTime, false);
    }

    public void OnMinDateTimeChanged(Instant? instant)
    {
        if (MinDateTime == instant)
        {
            return;
        }

        MinDateTime = instant;

        NavigateToNewQuery(_searchResults.CurrentPage, _searchResults.SortMode, _searchResults.Tags.Select(x => x.TagString).ToArray(), MinDateTime, MaxDateTime, true);
    }

    public void OnMaxDateTimeChanged(Instant? instant)
    {
        if (MaxDateTime == instant)
        {
            return;
        }

        MaxDateTime = instant;

        NavigateToNewQuery(_searchResults.CurrentPage, _searchResults.SortMode, _searchResults.Tags.Select(x => x.TagString).ToArray(), MinDateTime, instant, true);
    }

    public void AddSearchDataToTags(PostTagInfo tagInfo)
    {
        Add(tagInfo);
    }

    public void OnSelectedChipClose(MudChip mudChip)
    {
        if (mudChip.Value is not PostTagInfo tagInfo)
        {
            return;
        }

        Remove(tagInfo);
    }

    public void OnTagChipClickedCallback(PostTagInfo tagInfo)
    {
        if (Add(tagInfo))
        {
        }
        else
        {
            Remove(tagInfo);
        }
    }

    private bool Add(PostTagInfo tagInfo)
    {
        var tagStrings = _searchResults.Tags.Select(x => x.TagString).ToHashSet();
        if (tagStrings.Contains(tagInfo.TagString) || tagStrings.Contains(tagInfo.GetTagValue()))
        {
            return false;
        }

        if (!tagStrings.Add(tagInfo.TagString))
        {
            return false;
        }

        NavigateToNewQuery(_searchResults.CurrentPage, _searchResults.SortMode, tagStrings.ToArray(), MinDateTime, MaxDateTime, true);

        return true;
    }

    private bool Remove(PostTagInfo tagInfo)
    {
        var tagStrings = _searchResults.Tags.Select(x => x.TagString).ToHashSet();
        if (tagStrings.Remove(tagInfo.TagString) || tagStrings.Remove(tagInfo.GetTagValue()))
        {
            NavigateToNewQuery(_searchResults.CurrentPage, _searchResults.SortMode, tagStrings.ToArray(), MinDateTime, MaxDateTime, true);
            return true;
        }

        return false;
    }

    public void TryChangePage(int currentPage)
    {
        if (_searchResults.CurrentPage == currentPage)
        {
            return;
        }

        NavigateToNewQuery(currentPage, _searchResults.SortMode, _searchResults.Tags.Select(x => x.TagString).ToArray(), MinDateTime, MaxDateTime, false);
    }

    private void NavigateToNewQuery(int currentPage, PostSortMode sortMode, string[] tagStrings, Instant? minDateTime, Instant? maxDateTime, bool resetPage)
    {
        var dictionary = new Dictionary<string, object>
        {
            { "tag", tagStrings}
        };

        ZExtensions.AddToDictOrNull(dictionary, "sort", (int)sortMode, sortMode == 0);
        ZExtensions.AddToDictOrNull(dictionary, "page", currentPage, currentPage <= 1 || resetPage);

        dictionary.Add("ptmin", minDateTime?.ToUnixTimeMilliseconds());
        dictionary.Add("ptmax", maxDateTime?.ToUnixTimeMilliseconds());

        var oldPath = _services.ClientServices.NavigationManager.Uri;
        var newPath = _services.ClientServices.NavigationManager.GetUriWithQueryParameters(dictionary);
        if (newPath == oldPath)
        {
            return;
        }

        _services.ClientServices.NavigationManager.NavigateTo(newPath);
    }
}