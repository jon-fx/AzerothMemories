namespace AzerothMemories.WebBlazor.Pages;

public sealed class OnThisDayPageViewModel : ViewModelBase
{
    public DailyActivityResults[] Results { get; set; }

    public override async Task ComputeState()
    {
        var timeZone = Services.TimeProvider.GetCurrentTimeZone();
        var inZone = SystemClock.Instance.GetCurrentInstant().InZone(timeZone).Date;
        var startTime = timeZone.AtStartOfDay(inZone).ToInstant().ToUnixTimeMilliseconds();
        Results = await Services.ComputeServices.SearchServices.TryGetDailyActivityFull(null, timeZone.Id, startTime, CultureInfo.CurrentCulture.Name);
    }
}