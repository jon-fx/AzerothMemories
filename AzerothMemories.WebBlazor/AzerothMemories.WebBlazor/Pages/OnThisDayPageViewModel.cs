namespace AzerothMemories.WebBlazor.Pages;

public sealed class OnThisDayPageViewModel : ViewModelBase
{
    public DailyActivityResults[] Results { get; set; }

    public async Task ComputeState(string currentDay, string currentMonth)
    {
        var timeZone = Services.TimeProvider.GetCurrentTimeZone();
        var inZone = SystemClock.Instance.GetCurrentInstant().InZone(timeZone).Date;

        if (!byte.TryParse(currentDay, out var day))
        {
            day = (byte)inZone.Day;
        }
        if (!byte.TryParse(currentMonth, out var month))
        {
            month = (byte)inZone.Month;
        }

        Results = await Services.ComputeServices.SearchServices.TryGetDailyActivityFull(null, timeZone.Id, day, month, CultureInfo.CurrentCulture.Name);
    }
}