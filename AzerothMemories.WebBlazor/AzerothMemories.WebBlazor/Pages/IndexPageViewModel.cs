namespace AzerothMemories.WebBlazor.Pages;

public sealed class IndexPageViewModel : PersistentStateViewModel, IViewModel<IndexPageViewModel>
{
    private int _currentPage;
    private PostSortMode _sortMode;
    private RecentPostsType _postType;

    public IndexPageViewModel(IMoaServices services, Action onViewModelChanged) : base(services, onViewModelChanged)
    {
        RecentPostsHelper = new RecentPostsHelper(Services);

        AddPersistentState(() => AccountViewModel, x => AccountViewModel = x, () => Services.ComputeServices.AccountServices.TryGetActiveAccount(Session.Default));
        AddPersistentState(() => OnThisDay, x => OnThisDay = x, TryUpdateOnThisDay);
        AddPersistentState(() => RecentPostsHelper.SearchResults, x => RecentPostsHelper.SetSearchResults(x), () => RecentPostsHelper.ComputeState(_currentPage, _sortMode, _postType));
    }

    public AccountViewModel AccountViewModel { get; private set; }

    public DailyActivityResults OnThisDay { get; private set; }

    public RecentPostsHelper RecentPostsHelper { get; }

    public void OnParametersChanged(string currentPageString, string sortModeString, string postTypeString)
    {
        if (int.TryParse(currentPageString, out _currentPage) && _currentPage != 0)
        {
            _currentPage = Math.Clamp(_currentPage, 1, RecentPostsHelper.SearchResults.TotalPages);
        }
        else
        {
            _currentPage = 0;
        }

        if (int.TryParse(postTypeString, out var typeInt) && Enum.IsDefined(typeof(RecentPostsType), typeInt))
        {
            _postType = (RecentPostsType)typeInt;
        }
        else
        {
            _postType = RecentPostsType.Default;
        }

        if (int.TryParse(sortModeString, out var sortModeInt) && Enum.IsDefined(typeof(PostSortMode), sortModeInt))
        {
            _sortMode = (PostSortMode)sortModeInt;
        }
        else
        {
            _sortMode = PostSortMode.PostTimeStampDescending;
        }
    }

    public override async Task ComputeState(CancellationToken cancellationToken)
    {
        await base.ComputeState(cancellationToken);

        OnThisDay = await TryUpdateOnThisDay();
        AccountViewModel = await Services.ComputeServices.AccountServices.TryGetActiveAccount(Session.Default);

        await RecentPostsHelper.ComputeState(_currentPage, _sortMode, _postType);
    }

    private Task<DailyActivityResults> TryUpdateOnThisDay()
    {
        var timeZone = Services.ClientServices.TimeProvider.GetCurrentTimeZone();
        var inZone = SystemClock.Instance.GetCurrentInstant().InZone(timeZone).Date;

        return Services.ComputeServices.SearchServices.TryGetDailyActivity(Session.Default, timeZone.Id, (byte)inZone.Day, (byte)inZone.Month, ServerSideLocaleExt.GetServerSideLocale());
    }

    public static IndexPageViewModel CreateViewModel(IMoaServices services, Action onViewModelChanged)
    {
        return new IndexPageViewModel(services, onViewModelChanged);
    }
}