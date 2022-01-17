namespace AzerothMemories.WebServer.Controllers;

[ApiController, JsonifyErrors]
[Route("api/[controller]/[action]")]
public sealed class AccountController : ControllerBase, IAccountServices
{
    private readonly CommonServices _commonServices;

    public AccountController(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    [HttpGet, Publish]
    public Task<AccountViewModel> TryGetAccount(Session session)
    {
        return _commonServices.AccountServices.TryGetAccount(session);
    }

    [HttpGet("{accountId}"), Publish]
    public Task<AccountViewModel> TryGetAccountById(Session session, [FromRoute] long accountId)
    {
        return _commonServices.AccountServices.TryGetAccountById(session, accountId);
    }

    [HttpGet("{username}"), Publish]
    public Task<AccountViewModel> TryGetAccountByUsername(Session session, [FromRoute] string username)
    {
        return _commonServices.AccountServices.TryGetAccountByUsername(session, username);
    }

    [HttpGet("{username}"), Publish]
    public Task<bool> CheckIsValidUsername(Session session, [FromRoute] string username)
    {
        return _commonServices.AccountServices.CheckIsValidUsername(session, username);
    }

    [HttpPost("{newUsername}")]
    public Task<bool> TryChangeUsername(Session session, [FromRoute] string newUsername)
    {
        return _commonServices.AccountServices.TryChangeUsername(session, newUsername);
    }

    [HttpPost("{newValue}")]
    public Task<bool> TryChangeIsPrivate(Session session, [FromRoute] bool newValue)
    {
        return _commonServices.AccountServices.TryChangeIsPrivate(session, newValue);
    }

    [HttpPost("{newValue}")]
    public Task<bool> TryChangeBattleTagVisibility(Session session, [FromRoute] bool newValue)
    {
        return _commonServices.AccountServices.TryChangeBattleTagVisibility(session, newValue);
    }

    [HttpPost]
    [HttpPost("{newValue}")]
    public Task<string> TryChangeAvatar(Session session, [FromRoute] string newValue)
    {
        return _commonServices.AccountServices.TryChangeAvatar(session, newValue);
    }

    [HttpPost("{linkId}")]
    [HttpPost("{linkId}/{newValue}")]
    public Task<string> TryChangeSocialLink(Session session, [FromRoute] int linkId, [FromRoute] string newValue)
    {
        return _commonServices.AccountServices.TryChangeSocialLink(session, linkId, newValue);
    }

    [HttpGet("{timeStamp}/{diffInSeconds}"), Publish]
    public Task<PostTagInfo[]> TryGetAchievementsByTime(Session session, [FromRoute] long timeStamp, [FromRoute] int diffInSeconds, [FromQuery] string locale)
    {
        return _commonServices.AccountServices.TryGetAchievementsByTime(session, timeStamp, diffInSeconds, locale);
    }

    [HttpGet, Publish]
    public Task<AccountHistoryPageResult> TryGetAccountHistory(Session session, [FromQuery] int currentPage)
    {
        return _commonServices.AccountServices.TryGetAccountHistory(session, currentPage);
    }
}