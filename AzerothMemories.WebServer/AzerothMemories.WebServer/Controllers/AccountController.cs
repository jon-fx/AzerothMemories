namespace AzerothMemories.WebServer.Controllers;

[ApiController, JsonifyErrors]
[Route("api/[controller]/[action]")]
public class AccountController : ControllerBase, IAccountServices
{
    private readonly CommonServices _commonServices;

    public AccountController(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    [HttpGet, Publish]
    public Task<ActiveAccountViewModel> TryGetAccount(Session session, CancellationToken cancellationToken = default)
    {
        return _commonServices.AccountServices.TryGetAccount(session, cancellationToken);
    }

    [HttpGet("{accountId}"), Publish]
    public Task<AccountViewModel> TryGetAccount(Session session, [FromRoute] long accountId, CancellationToken cancellationToken = default)
    {
        return _commonServices.AccountServices.TryGetAccount(session, accountId, cancellationToken);
    }

    [HttpGet("{username}"), Publish]
    public Task<AccountViewModel> TryGetAccount(Session session, [FromRoute] string username, CancellationToken cancellationToken = default)
    {
        return _commonServices.AccountServices.TryGetAccount(session, username, cancellationToken);
    }

    [HttpGet("{username}"), Publish]
    public Task<bool> TryReserveUsername(Session session, [FromRoute] string username, CancellationToken cancellationToken = default)
    {
        return _commonServices.AccountServices.TryReserveUsername(session, username, cancellationToken);
    }

    [HttpPost("{newUsername}")]
    public Task<bool> TryChangeUsername(Session session, [FromRoute] string newUsername, CancellationToken cancellationToken = default)
    {
        return _commonServices.AccountServices.TryChangeUsername(session, newUsername, cancellationToken);
    }

    [HttpPost("{newValue}")]
    public Task<bool> TryChangeIsPrivate(Session session, [FromRoute] bool newValue, CancellationToken cancellationToken = default)
    {
        return _commonServices.AccountServices.TryChangeIsPrivate(session, newValue, cancellationToken);
    }

    [HttpPost("{newValue}")]
    public Task<bool> TryChangeBattleTagVisibility(Session session, [FromRoute] bool newValue, CancellationToken cancellationToken = default)
    {
        return _commonServices.AccountServices.TryChangeBattleTagVisibility(session, newValue, cancellationToken);
    }

    [HttpGet("{timeStamp}/{diffInSeconds}"), Publish]
    public Task<PostTagInfo[]> TryGetAchievementsByTime(Session session, [FromRoute] long timeStamp, [FromRoute] int diffInSeconds, CancellationToken cancellationToken = default)
    {
        return _commonServices.AccountServices.TryGetAchievementsByTime(session, timeStamp, diffInSeconds, cancellationToken);
    }

    [HttpPost]
    public Task<(AddMemoryResult Result, long PostId)> TryPostMemory(Session session, [FromBody] AddMemoryTransferData transferData, CancellationToken cancellationToken = default)
    {
        return _commonServices.AccountServices.TryPostMemory(session, transferData, cancellationToken);
    }
}