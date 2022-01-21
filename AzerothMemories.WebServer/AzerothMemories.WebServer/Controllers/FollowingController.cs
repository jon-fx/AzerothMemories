namespace AzerothMemories.WebServer.Controllers;

[ApiController, JsonifyErrors]
[Route("api/[controller]/[action]")]
public sealed class FollowingController : ControllerBase, IFollowingServices
{
    private readonly CommonServices _commonServices;

    public FollowingController(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    [HttpPost("{otherAccountId}")]
    public Task<AccountFollowingStatus?> TryStartFollowing(Session session, [FromRoute] long otherAccountId)
    {
        return _commonServices.FollowingServices.TryStartFollowing(session, otherAccountId);
    }

    [HttpPost("{otherAccountId}")]
    public Task<AccountFollowingStatus?> TryStopFollowing(Session session, [FromRoute] long otherAccountId)
    {
        return _commonServices.FollowingServices.TryStopFollowing(session, otherAccountId);
    }

    [HttpPost("{otherAccountId}")]
    public Task<AccountFollowingStatus?> TryAcceptFollower(Session session, [FromRoute] long otherAccountId)
    {
        return _commonServices.FollowingServices.TryAcceptFollower(session, otherAccountId);
    }

    [HttpPost("{otherAccountId}")]
    public Task<AccountFollowingStatus?> TryRemoveFollower(Session session, [FromRoute] long otherAccountId)
    {
        return _commonServices.FollowingServices.TryRemoveFollower(session, otherAccountId);
    }
}