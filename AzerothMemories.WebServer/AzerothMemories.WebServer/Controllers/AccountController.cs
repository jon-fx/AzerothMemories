namespace AzerothMemories.WebServer.Controllers;

[ApiController, JsonifyErrors]
[Route("api/[controller]/[action]")]
public class AccountController : ControllerBase, IAccountServices
{
    private readonly IAccountServices _accountServices;
    private readonly CommonServices _commonServices;

    public AccountController(IAccountServices accountServices, CommonServices commonServices)
    {
        _accountServices = accountServices;
        _commonServices = commonServices;
    }

    [HttpGet, Publish]
    public Task<ActiveAccountViewModel> TryGetAccount(Session session, CancellationToken cancellationToken = default)
    {
        return _accountServices.TryGetAccount(session, cancellationToken);
    }

    [HttpGet("{accountId}"), Publish]
    public Task<AccountViewModel> TryGetAccount(Session session, long accountId, CancellationToken cancellationToken = default)
    {
        return _accountServices.TryGetAccount(session, accountId, cancellationToken);
    }

    [HttpGet("{username}"), Publish]
    public Task<AccountViewModel> TryGetAccount(Session session, string username, CancellationToken cancellationToken = default)
    {
        return _accountServices.TryGetAccount(session, username, cancellationToken);
    }

    [HttpGet("{username}"), Publish]
    public Task<bool> TryReserveUsername(Session session, string username, CancellationToken cancellationToken = default)
    {
        return _accountServices.TryReserveUsername(session, username, cancellationToken);
    }

    [HttpPost("{newUsername}")]
    public Task<bool> TryChangeUsername(Session session, string newUsername, CancellationToken cancellationToken = default)
    {
        return _accountServices.TryChangeUsername(session, newUsername, cancellationToken);
    }

    [HttpPost("{newValue}")]
    public Task<bool> TryChangeIsPrivate(Session session, bool newValue, CancellationToken cancellationToken = default)
    {
        return _accountServices.TryChangeIsPrivate(session, newValue, cancellationToken);
    }

    [HttpPost("{newValue}")]
    public Task<bool> TryChangeBattleTagVisibility(Session session, bool newValue, CancellationToken cancellationToken = default)
    {
        return _accountServices.TryChangeBattleTagVisibility(session, newValue, cancellationToken);
    }

    //[HttpPost("{newUsername}")]
    //public Task<string> TryChangeUsername(Session session, [FromRoute] string newUsername, CancellationToken cancellationToken = default)
    //{
    //    return _accountServices.TryChangeUsername(session, newUsername, cancellationToken);
    //}

    [HttpGet("{timeStamp}/{diffInSeconds}"), Publish]
    public Task<PostTagInfo[]> TryGetAchievementsByTime(Session session, [FromRoute] long timeStamp, [FromRoute] int diffInSeconds, CancellationToken cancellationToken = default)
    {
        return _accountServices.TryGetAchievementsByTime(session, timeStamp, diffInSeconds, cancellationToken);
    }
}