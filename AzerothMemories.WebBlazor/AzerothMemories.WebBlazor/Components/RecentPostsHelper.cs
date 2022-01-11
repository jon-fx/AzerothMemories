namespace AzerothMemories.WebBlazor.Components
{
    public sealed class RecentPostsHelper
    {
        private readonly IMoaServices _services;

        private RecentPostsResults _searchResults;

        private string _sortModeString;
        private string _recentPostTypeString;
        private string _currentPageString;

        private int _currentPage;
        private PostSortMode _sortMode;
        private RecentPostsType _recentPostType;

        public RecentPostsHelper(IMoaServices services)
        {
            _services = services;

            IsLoading = true;
        }

        public bool NoResults => _searchResults.PostViewModels.Length == 0;

        public int CurrentPage => _searchResults.CurrentPage;

        public int TotalPages => _searchResults.TotalPages;

        public PostViewModel[] CurrentPosts => _searchResults.PostViewModels;

        public bool IsLoading { get; private set; }

        public async Task ComputeState(string currentPageString, string sortModeString, string postTypeString)
        {
            if (_searchResults == null)
            {
                _searchResults = new RecentPostsResults();
            }
            else if (sortModeString == _sortModeString && currentPageString == _currentPageString && postTypeString == _recentPostTypeString)
            {
                return;
            }

            _sortModeString = sortModeString;
            _currentPageString = currentPageString;
            _recentPostTypeString = postTypeString;

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

            _sortMode = PostSortMode.PostTimeStampDesc;
            if (int.TryParse(sortModeString, out var sortModeInt) && Enum.IsDefined(typeof(PostSortMode), sortModeInt))
            {
                _sortMode = (PostSortMode)sortModeInt;
            }

            IsLoading = true;

            var searchResults = await _services.SearchPostsServices.TryGetRecentPosts(null, _recentPostType, _sortMode, _currentPage, CultureInfo.CurrentCulture.Name);

            _searchResults = searchResults;

            IsLoading = false;
        }

        public Task OnShowAllChanged()
        {
            throw new NotImplementedException();
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
            var oldPath = _services.NavigationManager.Uri;
            var sortMode = (int)_sortMode;
            if (sortMode > 0 || _sortModeString != null)
            {
                dictionary.Add("sort", sortMode);
            }

            if (_currentPage > 1 || _currentPageString != null)
            {
                dictionary.Add("page", resetPage ? 0 : _currentPage);
            }

            var recentPostType = (int)_recentPostType;
            if (recentPostType != 0 || _recentPostTypeString != null)
            {
                dictionary.Add("type", _recentPostType);
            }

            var newPath = _services.NavigationManager.GetUriWithQueryParameters(dictionary);
            if (newPath == oldPath)
            {
                return;
            }

            _services.NavigationManager.NavigateTo(newPath);
        }
    }
}