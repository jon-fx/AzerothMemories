namespace AzerothMemories.WebBlazor.Pages;

public sealed class OnThisDayPageViewModel : PersistentStateViewModel, IViewModel<OnThisDayPageViewModel>
{
    private string? _currentDay;
    private string? _currentMonth;

    public DailyActivityResults[]? DailyActivityResults { get; private set; }

    public OnThisDayPageViewModel(IMoaServices services, Action onViewModelChanged) : base(services, onViewModelChanged)
    {
        AddPersistentState(() => DailyActivityResults, x => DailyActivityResults = x, UpdateDailyActivityResults!);
    }

    public void OnParametersChanged(string? currentDay, string? currentMonth)
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

        return Services.ComputeServices.SearchServices.TryGetDailyActivityFull(Services.ClientServices.Session,  timeZone.Id, day, month, ServerSideLocaleExt.GetServerSideLocale());
    }

    public static OnThisDayPageViewModel CreateViewModel(IMoaServices services, Action onViewModelChanged)
    {
        return new OnThisDayPageViewModel(services, onViewModelChanged);
    }
}