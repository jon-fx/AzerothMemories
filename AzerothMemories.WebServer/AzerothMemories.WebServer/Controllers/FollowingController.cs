namespace AzerothMemories.WebServer.Controllers;

[ApiController]
[JsonifyErrors]
[Route("api/[controller]/[action]")]
public sealed class FollowingController : ControllerBase, IFollowingServices
{
    private readonly CommonServices _commonServices;

    public FollowingController(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    [HttpPost]
    public Task<AccountFollowingStatus?> TryStartFollowing([FromBody] Following_TryStartFollowing command, CancellationToken cancellationToken = default)
    {
        return _commonServices.Commander.Call(command, cancellationToken);
    }

    [HttpPost]
    public Task<AccountFollowingStatus?> TryStopFollowing([FromBody] Following_TryStopFollowing command, CancellationToken cancellationToken = default)
    {
        return _commonServices.Commander.Call(command, cancellationToken);
    }

    [HttpPost]
    public Task<AccountFollowingStatus?> TryAcceptFollower([FromBody] Following_TryAcceptFollower command, CancellationToken cancellationToken = default)
    {
        return _commonServices.Commander.Call(command, cancellationToken);
    }

    [HttpPost]
    public Task<AccountFollowingStatus?> TryRemoveFollower([FromBody] Following_TryRemoveFollower command, CancellationToken cancellationToken = default)
    {
        return _commonServices.Commander.Call(command, cancellationToken);
    }
}