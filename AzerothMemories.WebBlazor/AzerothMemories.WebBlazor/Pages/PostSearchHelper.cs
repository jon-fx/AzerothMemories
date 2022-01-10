using System.Collections;

namespace AzerothMemories.WebBlazor.Pages;

public sealed class PostSearchHelper
{
    private readonly IMoaServices _services;

    private SearchPostsResults _searchResults;

    public Instant? MinDateTime;
    public Instant? MaxDateTime;

    private string _sortModeString;
    private string _currentPageString;
    private string _postMinTimeString;
    private string _postMaxTimeString;
    private HashSet<string> _tagStrings;

    private int _currentPage;
    private PostSortMode _sortMode;

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
        var sameTagStrings = StructuralComparisons.StructuralEqualityComparer.Equals(_tagStrings, tagStrings);
        if (sameTagStrings && sortModeString == _sortModeString && currentPageString == _currentPageString && postMinTimeString == _postMinTimeString && postMaxTimeString == _postMaxTimeString)
        {
            return;
        }

        _tagStrings = tagStrings.ToHashSet();
        _sortModeString = sortModeString;
        _currentPageString = currentPageString;
        _postMinTimeString = postMinTimeString;
        _postMaxTimeString = postMaxTimeString;

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

        _sortMode = PostSortMode.PostTimeStampDesc;
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

        var searchResults = await _services.SearchPostsServices.TrySearchPosts(null, _tagStrings.ToArray(), _sortMode, _currentPage, minTime, maxTime);

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

    private void NavigateToNewQuery(bool resetPage)
    {
        var dictionary = new Dictionary<string, object>
        {
            { "tag", _tagStrings.ToArray() }
        };

        var oldPath = _services.NavigationManager.Uri;
        var sortMode = (int)_sortMode;
        if (sortMode > 0 || _sortModeString != null)
        {
            dictionary.Add("sort", sortMode);
        }

        if (_currentPage > 1 || _currentPageString != null)
        {
            if (resetPage)
            {
                dictionary.Add("page", 0);
            }
            else
            {
                dictionary.Add("page", _currentPage);
            }
        }

        dictionary.Add("ptmin", MinDateTime?.ToUnixTimeMilliseconds());
        dictionary.Add("ptmax", MaxDateTime?.ToUnixTimeMilliseconds());

        var newPath = _services.NavigationManager.GetUriWithQueryParameters(dictionary);
        if (newPath == oldPath)
        {
            return;
        }

        Services.NavigationManager.NavigateTo(newPath);
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
}