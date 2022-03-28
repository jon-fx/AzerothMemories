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

    public AccountViewModel AccountViewModel { get; private set; }

    public AccountHistoryViewModel[] AccountHistoryViewModels { get; private set; }

    //public bool IsAccountActive => AccountViewModel != null && AccountViewModel.Id > 0;

    //public bool IsAccountActiveAndCanInteract => AccountViewModel != null && AccountViewModel.Id > 0 && AccountViewModel.CanInteract;

    //public bool IsAdmin => AccountViewModel != null && AccountViewModel.Id > 0 && AccountViewModel.IsAdmin();

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

    public bool IsActiveAccount(int accountId)
    {
        if (AccountViewModel == null)
        {
            return false;
        }

        return AccountViewModel.Id == accountId;
    }

    public async Task<bool> ComputeState()
    {
        var previousAccountId = AccountViewModel?.Id;

        AccountViewModel = await _accountServices.TryGetActiveAccount(Session.Default);

        if (AccountViewModel == null)
        {
            AccountHistoryViewModels = Array.Empty<AccountHistoryViewModel>();
        }
        else
        {
            var newHistory = await _accountServices.TryGetAccountHistory(Session.Default);
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
            AccountHistoryViewModels ??= Array.Empty<AccountHistoryViewModel>();
        }

        return previousAccountId != AccountViewModel?.Id;
    }

    public Dictionary<int, string> GetUserTagList()
    {
        if (AccountViewModel == null)
        {
            return new Dictionary<int, string>();
        }

        return AccountViewModel.GetUserTagList();
    }
}