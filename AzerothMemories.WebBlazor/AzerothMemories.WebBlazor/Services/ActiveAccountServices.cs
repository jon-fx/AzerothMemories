namespace AzerothMemories.WebBlazor.Services;

public sealed class ActiveAccountServices
{
    private readonly IAccountServices _accountServices;
    private readonly ICharacterServices _characterServices;
    private readonly TimeProvider _timeProvider;
    private readonly ISnackbar _snackbarService;
    private readonly IStringLocalizer<BlizzardResources> _stringLocalizer;

    private IActiveCommentContext _activeCommentContext;

    public ActiveAccountServices(IAccountServices accountServices, ICharacterServices characterServices, TimeProvider timeProvider, ISnackbar snackbar, IStringLocalizer<BlizzardResources> stringLocalizer)
    {
        _accountServices = accountServices;
        _characterServices = characterServices;
        _timeProvider = timeProvider;
        _snackbarService = snackbar;
        _stringLocalizer = stringLocalizer;
    }

    public long ActiveAccountId => AccountViewModel?.Id ?? 0;

    public ActiveAccountViewModel AccountViewModel { get; private set; }

    public AccountHistoryViewModel[] AccountHistoryViewModels { get; private set; }

    public bool IsAccountActive => AccountViewModel != null && AccountViewModel.Id > 0;

    public bool IsAdmin => IsAccountActive && AccountViewModel.AccountType == AccountType.Admin;

    public IActiveCommentContext ActiveCommentContext
    {
        get => _activeCommentContext;
        set
        {
            if (_activeCommentContext == value)
            {
                return;
            }

            var previous = _activeCommentContext;

            _activeCommentContext = value;

            previous?.InvokeStateHasChanged();
        }
    }

    public async Task ComputeState()
    {
        AccountViewModel = await _accountServices.TryGetAccount(null);

        if (AccountViewModel != null)
        {
            var newHistory = await _accountServices.TryGetAccountHistory(null);
            var oldHistory = AccountHistoryViewModels;

            if (oldHistory != null && oldHistory.Length != 0)
            {
                var oldSet = oldHistory.Select(x => x.Id).ToHashSet();
                foreach (var newItem in newHistory.ViewModels)
                {
                    if (oldSet.Contains(newItem.Id))
                    {
                    }
                    else
                    {
                        var displayText = newItem.GetDisplayText(AccountViewModel, _stringLocalizer);

                        _snackbarService.Add($"{_timeProvider.GetTimeAsLocalStringAgo(newItem.CreatedTime, true)}<br>{displayText}", Severity.Normal, config =>
                        {
                            config.HideIcon = true;
                            config.VisibleStateDuration = 5000;
                            config.ShowCloseIcon = true;
                            config.Onclick = _ => Task.CompletedTask;
                        });
                    }
                }
            }

            AccountHistoryViewModels = newHistory.ViewModels;
        }

        AccountHistoryViewModels ??= Array.Empty<AccountHistoryViewModel>();
    }

    public Dictionary<long, string> GetUserTagList()
    {
        if (AccountViewModel == null)
        {
            return new Dictionary<long, string>();
        }

        return AccountViewModel.GetUserTagList();
    }
}