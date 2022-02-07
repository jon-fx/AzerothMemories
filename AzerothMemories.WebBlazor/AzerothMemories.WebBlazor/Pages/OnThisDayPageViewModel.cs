namespace AzerothMemories.WebBlazor.Pages;

public sealed class OnThisDayPageViewModel : ViewModelBase
{
    public ActivityResults Results { get; set; }

    public override async Task ComputeState()
    {
        Results = await Services.ComputeServices.SearchServices.TryGetDailyActivityFull(null, Services.TimeProvider.GetCurrentTimeZone().Id, CultureInfo.CurrentCulture.Name);
    }
}