using AzerothMemories.WebBlazor.Components;

namespace AzerothMemories.WebBlazor.Pages;

public sealed class IndexPageViewModel : ViewModelBase
{
    public AccountViewModel AccountViewModel { get; private set; }

    public DailyActivityResults OnThisDay { get; private set; }

    public RecentPostsHelper RecentPostsHelper { get; private set; }

    public override async Task OnInitialized()
    {
        await base.OnInitialized();

        RecentPostsHelper = new RecentPostsHelper(Services);
    }

    public async Task ComputeState(string currentPageString, string sortModeString, string postTypeString)
    {
        var timeZone = Services.TimeProvider.GetCurrentTimeZone();
        var inZone = SystemClock.Instance.GetCurrentInstant().InZone(timeZone).Date;
        OnThisDay = await Services.ComputeServices.SearchServices.TryGetDailyActivity(null, timeZone.Id, (byte)inZone.Day, (byte)inZone.Month, CultureInfo.CurrentCulture.Name);

        AccountViewModel = await Services.ComputeServices.AccountServices.TryGetActiveAccount(null);

        await RecentPostsHelper.ComputeState(currentPageString, sortModeString, postTypeString);
    }
}