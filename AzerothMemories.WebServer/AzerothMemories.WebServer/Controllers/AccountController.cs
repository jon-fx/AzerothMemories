namespace AzerothMemories.WebServer.Controllers;

[ApiController]
[JsonifyErrors]
[UseDefaultSession]
//[AutoValidateAntiforgeryToken]
[Route("api/[controller]/[action]")]
public sealed class AccountController : ControllerBase, IAccountServices
{
    private readonly CommonServices _commonServices;

    public AccountController(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    [HttpGet]
    public Task<AccountViewModel> TryGetActiveAccount(Session session)
    {
        return _commonServices.AccountServices.TryGetActiveAccount(session);
    }

    [HttpGet("{accountId}")]
    public Task<AccountViewModel> TryGetAccountById(Session session, [FromRoute] int accountId)
    {
        return _commonServices.AccountServices.TryGetAccountById(session, accountId);
    }

    [HttpGet("{username}")]
    public Task<AccountViewModel> TryGetAccountByUsername(Session session, [FromRoute] string username)
    {
        return _commonServices.AccountServices.TryGetAccountByUsername(session, username);
    }

    [HttpPost]
    public Task<bool> TryEnqueueUpdate(Session session)
    {
        return _commonServices.AccountServices.TryEnqueueUpdate(session);
    }

    [HttpGet("{username}")]
    public Task<bool> CheckIsValidUsername(Session session, [FromRoute] string username)
    {
        return _commonServices.AccountServices.CheckIsValidUsername(session, username);
    }

    [HttpPost]
    public Task<bool> TryChangeUsername([FromBody] Account_TryChangeUsername command, CancellationToken cancellationToken)
    {
        return _commonServices.Commander.Call(command, cancellationToken);
    }

    [HttpPost]
    public Task<bool> TryChangeIsPrivate([FromBody] Account_TryChangeIsPrivate command, CancellationToken cancellationToken = default)
    {
        return _commonServices.Commander.Call(command, cancellationToken);
    }

    [HttpPost]
    public Task<bool> TryChangeBattleTagVisibility([FromBody] Account_TryChangeBattleTagVisibility command, CancellationToken cancellationToken = default)
    {
        return _commonServices.Commander.Call(command, cancellationToken);
    }

    [HttpPost]
    public Task<string> TryChangeAvatar([FromBody] Account_TryChangeAvatar command, CancellationToken cancellationToken = default)
    {
        return _commonServices.Commander.Call(command, cancellationToken);
    }

    [HttpPost]
    public Task<string> TryChangeAvatarUpload([FromBody] Account_TryChangeAvatarUpload command, CancellationToken cancellationToken = default)
    {
        return _commonServices.Commander.Call(command, cancellationToken);
    }

    [HttpPost]
    public Task<string> TryChangeSocialLink([FromBody] Account_TryChangeSocialLink command, CancellationToken cancellationToken = default)
    {
        return _commonServices.Commander.Call(command, cancellationToken);
    }

    [HttpPost]
    public Task<bool> TryDisconnectAccount([FromBody] Account_TryDisconnectAccount command, CancellationToken cancellationToken = default)
    {
        return _commonServices.Commander.Call(command, cancellationToken);
    }

    [HttpGet("{timeStamp}/{diffInSeconds}")]
    public Task<PostTagInfo[]> TryGetAchievementsByTime(Session session, [FromRoute] long timeStamp, [FromRoute] int diffInSeconds, [FromQuery] ServerSideLocale locale)
    {
        return _commonServices.AccountServices.TryGetAchievementsByTime(session, timeStamp, diffInSeconds, locale);
    }

    [HttpGet]
    public Task<AccountHistoryPageResult> TryGetAccountHistory(Session session, [FromQuery] int currentPage)
    {
        return _commonServices.AccountServices.TryGetAccountHistory(session, currentPage);
    }
}