namespace AzerothMemories.WebBlazor.Pages;

public sealed class IndexPageViewModel : PersistentStateViewModel, IPageHeaderInfoProvider
{
    private string _currentPageString;
    private string _sortModeString;
    private string _postTypeString;

    public IndexPageViewModel()
    {
        AddPersistentState(() => AccountViewModel, x => AccountViewModel = x, () => Services.ComputeServices.AccountServices.TryGetActiveAccount(Session.Default));
        AddPersistentState(() => OnThisDay, x => OnThisDay = x, TryUpdateOnThisDay);
        AddPersistentState(() => RecentPostsHelper.SearchResults, x => RecentPostsHelper.SetSearchResults(x), () => RecentPostsHelper.ComputeState(_currentPageString, _sortModeString, _postTypeString));
    }

    public AccountViewModel AccountViewModel { get; private set; }

    public DailyActivityResults OnThisDay { get; private set; }

    public RecentPostsHelper RecentPostsHelper { get; private set; }

    public void OnParametersChanged(string currentPageString, string sortModeString, string postTypeString)
    {
        _currentPageString = currentPageString;
        _sortModeString = sortModeString;
        _postTypeString = postTypeString;
    }

    public override async Task OnInitialized()
    {
        RecentPostsHelper = new RecentPostsHelper(Services);

        await base.OnInitialized();
    }

    public override async Task ComputeState(CancellationToken cancellationToken)
    {
        await base.ComputeState(cancellationToken);

        OnThisDay = await TryUpdateOnThisDay();
        AccountViewModel = await Services.ComputeServices.AccountServices.TryGetActiveAccount(Session.Default);

        if (RecentPostsHelper == null)
        {
        }
        else
        {
            await RecentPostsHelper.ComputeState(_currentPageString, _sortModeString, _postTypeString);
        }
    }

    private Task<DailyActivityResults> TryUpdateOnThisDay()
    {
        var timeZone = Services.ClientServices.TimeProvider.GetCurrentTimeZone();
        var inZone = SystemClock.Instance.GetCurrentInstant().InZone(timeZone).Date;

        return Services.ComputeServices.SearchServices.TryGetDailyActivity(Session.Default, timeZone.Id, (byte)inZone.Day, (byte)inZone.Month, ServerSideLocaleExt.GetServerSideLocale());
    }

    public string GetPageTitle()
    {
        return "Memories of Azeroth";
    }

    public string GetPageDescription()
    {
        return "Memories of Azeroth is a site dedicated to organising, storing and sharing your World of Warcraft screenshots.";
    }

    public string GetPageImage()
    {
        return "header-banner.png";
    }

    public string GetPageImageAlt()
    {
        return "Memories of Azeroth header banner";
    }
}