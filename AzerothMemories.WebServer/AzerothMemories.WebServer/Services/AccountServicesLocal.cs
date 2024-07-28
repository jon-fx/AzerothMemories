namespace AzerothMemories.WebServer.Services;

public class AccountServicesLocal : IComputeService
{
    private readonly CommonServices _commonServices;
    private readonly ILogger<AccountServices> _logger;
    private readonly IDbSessionInfoRepo<AppDbContext, DbSessionInfo<string>, string> _sessionRepo;

    public AccountServicesLocal(CommonServices commonServices, ILogger<AccountServices> logger, IDbSessionInfoRepo<AppDbContext, DbSessionInfo<string>, string> sessionRepo)
    {
        _logger = logger;
        _commonServices = commonServices;
        _sessionRepo = sessionRepo;
    }

    [CommandHandler]
    public virtual async Task<bool> TryUpdateAuthToken(Account_TryUpdateAuthToken command, CancellationToken cancellationToken = default)
    {
        using var _ = new MethodTimeLogger(_logger);
        return await AccountServices_TryUpdateAuthToken.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<bool> AddNewHistoryItem(Account_AddNewHistoryItem command, CancellationToken cancellationToken = default)
    {
        using var _ = new MethodTimeLogger(_logger);
        return await AccountServices_AddNewHistoryItem.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler(IsFilter = true, Priority = 1)]
    protected virtual async Task OnSignInCommand(AuthBackend_SignIn command, CancellationToken cancellationToken)
    {
        using var _ = new MethodTimeLogger(_logger);
        await AccountServices_OnSignInCommand.TryHandle(_logger, _commonServices, _sessionRepo, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler(IsFilter = true, Priority = 1)]
    protected virtual async Task OnSignOutCommand(Auth_SignOut command, CancellationToken cancellationToken)
    {
        using var _ = new MethodTimeLogger(_logger);
        await AccountServices_OnSignOutCommand.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler(IsFilter = true, Priority = 1)]
    protected virtual async Task OnSetupSessionCommand(AuthBackend_SetupSession command, CancellationToken cancellationToken)
    {
        using var _ = new MethodTimeLogger(_logger);
        await AccountServices_OnSetupSessionCommand.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }
}