namespace AzerothMemories.WebBlazor.Pages;

public sealed class OnThisDayPageViewModel : PersistentStateViewModel
{
    private string _currentDay;
    private string _currentMonth;

    public DailyActivityResults[] DailyActivityResults { get; set; }

    public OnThisDayPageViewModel()
    {
        AddPersistentState(() => DailyActivityResults, x => DailyActivityResults = x, UpdateDailyActivityResults);
    }

    public void OnParametersChanged(string currentDay, string currentMonth)
    {
        _currentDay = currentDay;
        _currentMonth = currentMonth;
    }

    public override async Task ComputeState(CancellationToken cancellationToken)
    {
        await base.ComputeState(cancellationToken);

        DailyActivityResults = await UpdateDailyActivityResults();
    }

    private Task<DailyActivityResults[]> UpdateDailyActivityResults()
    {
        var timeZone = Services.ClientServices.TimeProvider.GetCurrentTimeZone();
        var inZone = SystemClock.Instance.GetCurrentInstant().InZone(timeZone).Date;

        if (!byte.TryParse(_currentDay, out var day))
        {
            day = (byte)inZone.Day;
        }
        if (!byte.TryParse(_currentMonth, out var month))
        {
            month = (byte)inZone.Month;
        }

        return Services.ComputeServices.SearchServices.TryGetDailyActivityFull(null, timeZone.Id, day, month, ServerSideLocaleExt.GetServerSideLocale());
    }
}