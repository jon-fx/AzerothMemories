namespace AzerothMemories.WebBlazor.Pages;

public sealed class PostSearchHelper
{
    private readonly IMoaServices _services;

    private SearchPostsResults _searchResults;

    public Instant? MinDateTime;
    public Instant? MaxDateTime;

    private int _currentPage;
    private PostSortMode _sortMode;
    private HashSet<string> _tagStrings;

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

    public async Task ComputeState(string[] tagStrings, string sortModeString, string currentPageString, string postMinTimeString, string postMaxTimeString)
    {
        _tagStrings = tagStrings.ToHashSet();

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

        _sortMode = PostSortMode.PostTimeStampDescending;
        if (int.TryParse(sortModeString, out var sortModeInt) && Enum.IsDefined(typeof(PostSortMode), sortModeInt))
        {
            _sortMode = (PostSortMode)sortModeInt;
        }

        if (long.TryParse(postMinTimeString, out var minTime))
        {
            minTime = Math.Clamp(minTime, 0, SystemClock.Instance.GetCurrentInstant().ToUnixTimeMilliseconds());
        }

        if (long.TryParse(postMaxTimeString, out var maxTime))
        {
            maxTime = Math.Clamp(maxTime, 0, SystemClock.Instance.GetCurrentInstant().ToUnixTimeMilliseconds());
        }

        IsLoading = true;

        var searchResults = await _services.SearchServices.TrySearchPosts(null, _tagStrings.ToArray(), _sortMode, _currentPage, minTime, maxTime, CultureInfo.CurrentCulture.Name);

        _searchResults = searchResults;

        MinDateTime = _searchResults.MinTime > 0 ? Instant.FromUnixTimeMilliseconds(_searchResults.MinTime) : null;
        MaxDateTime = _searchResults.MaxTime > 0 ? Instant.FromUnixTimeMilliseconds(_searchResults.MaxTime) : null;

        foreach (var info in _searchResults.Tags)
        {
            info.IsChipClosable = true;
        }

        IsLoading = false;
    }

    public void OnSortChanged(PostSortMode sortMode)
    {
        if (_sortMode == sortMode)
        {
            return;
        }

        _sortMode = sortMode;

        NavigateToNewQuery(false);
    }

    public void OnMinDateTimeChanged(Instant? instant)
    {
        if (MinDateTime == instant)
        {
            return;
        }

        MinDateTime = instant;

        NavigateToNewQuery(true);
    }

    public void OnMaxDateTimeChanged(Instant? instant)
    {
        if (MaxDateTime == instant)
        {
            return;
        }

        MaxDateTime = instant;

        NavigateToNewQuery(true);
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
        if (_tagStrings.Contains(tagInfo.TagString) || _tagStrings.Contains(tagInfo.GetTagValue()))
        {
            return false;
        }

        if (!_tagStrings.Add(tagInfo.TagString))
        {
            return false;
        }

        NavigateToNewQuery(true);

        return true;
    }

    private bool Remove(PostTagInfo tagInfo)
    {
        if (_tagStrings.Remove(tagInfo.TagString) || _tagStrings.Remove(tagInfo.GetTagValue()))
        {
            NavigateToNewQuery(true);
            return true;
        }

        return false;
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
        var dictionary = new Dictionary<string, object>
        {
            { "tag", _tagStrings.ToArray() }
        };

        ZExtensions.AddToDictOrNull(dictionary, "sort", (int)_sortMode, _sortMode == 0);
        ZExtensions.AddToDictOrNull(dictionary, "page", _currentPage, _currentPage <= 1 || resetPage);

        dictionary.Add("ptmin", MinDateTime?.ToUnixTimeMilliseconds());
        dictionary.Add("ptmax", MaxDateTime?.ToUnixTimeMilliseconds());

        var oldPath = _services.NavigationManager.Uri;
        var newPath = _services.NavigationManager.GetUriWithQueryParameters(dictionary);
        if (newPath == oldPath)
        {
            return;
        }

        _services.NavigationManager.NavigateTo(newPath);
    }
}