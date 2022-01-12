namespace AzerothMemories.WebBlazor.Services;

public sealed class ActiveAccountServices
{
    private readonly IAccountServices _accountServices;
    private readonly ICharacterServices _characterServices;

    private IActiveCommentContext _activeCommentContext;

    public ActiveAccountServices(IAccountServices accountServices, ICharacterServices characterServices)
    {
        _accountServices = accountServices;
        _characterServices = characterServices;
    }

    public long ActiveAccountId => AccountViewModel?.Id ?? 0;

    public ActiveAccountViewModel AccountViewModel { get; private set; }

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

    public async Task ComputeState(CancellationToken cancellationToken)
    {
        AccountViewModel = await _accountServices.TryGetAccount(null, cancellationToken);
    }

    public Dictionary<long, string> GetUserTagList()
    {
        return new Dictionary<long, string>();
    }
}