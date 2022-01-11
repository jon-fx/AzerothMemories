namespace AzerothMemories.WebServer.Controllers;

[ApiController, JsonifyErrors]
[Route("api/[controller]/[action]")]
public class AccountFollowingController : ControllerBase, IAccountFollowingServices
{
    private readonly CommonServices _commonServices;

    public AccountFollowingController(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    [HttpPost("{otherAccountId}")]
    public Task<AccountFollowingStatus?> TryStartFollowing(Session session, [FromRoute] long otherAccountId)
    {
        return _commonServices.AccountFollowingServices.TryStartFollowing(session, otherAccountId);
    }

    [HttpPost("{otherAccountId}")]
    public Task<AccountFollowingStatus?> TryStopFollowing(Session session, [FromRoute] long otherAccountId)
    {
        return _commonServices.AccountFollowingServices.TryStopFollowing(session, otherAccountId);
    }

    [HttpPost("{otherAccountId}")]
    public Task<AccountFollowingStatus?> TryAcceptFollower(Session session, [FromRoute] long otherAccountId)
    {
        return _commonServices.AccountFollowingServices.TryAcceptFollower(session, otherAccountId);
    }

    [HttpPost("{otherAccountId}")]
    public Task<AccountFollowingStatus?> TryRemoveFollower(Session session, [FromRoute] long otherAccountId)
    {
        return _commonServices.AccountFollowingServices.TryRemoveFollower(session, otherAccountId);
    }
}