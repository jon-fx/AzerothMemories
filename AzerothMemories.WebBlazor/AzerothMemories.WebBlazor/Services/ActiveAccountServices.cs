namespace AzerothMemories.WebBlazor.Services;

public sealed class ActiveAccountServices
{
    private readonly IAccountServices _accountServices;
    private readonly ICharacterServices _characterServices;
    private readonly TimeProviderEx _timeProvider;
    private readonly ISnackbar _snackbarService;
    private readonly IStringLocalizer<BlizzardResources> _stringLocalizer;
    private readonly Session _session;

    private IActiveCommentContext? _activeCommentContext;

    public ActiveAccountServices(IAccountServices accountServices, ICharacterServices characterServices, TimeProviderEx timeProvider, ISnackbar snackbar, IStringLocalizer<BlizzardResources> stringLocalizer, Session session)
    {
        _accountServices = accountServices;
        _characterServices = characterServices;
        _timeProvider = timeProvider;
        _snackbarService = snackbar;
        _stringLocalizer = stringLocalizer;
        _session = session;
    }

    public Session Session => _session;

    public AccountViewModel? AccountViewModel { get; private set; }

    public AccountHistoryViewModel[]? AccountHistoryViewModels { get; private set; }

    public IActiveCommentContext? ActiveCommentContext
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

        AccountViewModel = await _accountServices.TryGetActiveAccount(Session);

        if (AccountViewModel == null)
        {
            AccountHistoryViewModels = [];
        }
        else
        {
            var newHistory = await _accountServices.TryGetAccountHistory(Session);
            var newHistoryViewModels = newHistory?.ViewModels ?? [];
            var oldHistory = AccountHistoryViewModels;

            if (oldHistory != null && oldHistory.Length != 0)
            {
                var oldSet = oldHistory.Select(x => x.Id).ToHashSet();
                foreach (var newItem in newHistoryViewModels)
                {
                    if (oldSet.Contains(newItem.Id))
                    {
                    }
                    else
                    {
                        var displayText = newItem.GetDisplayText(AccountViewModel, _stringLocalizer);

                        _snackbarService.Add((MarkupString)$"{_timeProvider.GetTimeAsLocalStringAgo(newItem.CreatedTime, true)}<br>{displayText}", Severity.Normal, config =>
                        {
                            config.HideIcon = true;
                            config.VisibleStateDuration = 5000;
                            config.ShowCloseIcon = true;
                            config.OnClick = _ => Task.CompletedTask;
                        });
                    }
                }
            }

            AccountHistoryViewModels = newHistoryViewModels;
            AccountHistoryViewModels ??= [];
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