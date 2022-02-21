namespace AzerothMemories.WebServer.Controllers;

[ApiController, JsonifyErrors]
[AutoValidateAntiforgeryToken]
[Route("api/[controller]/[action]")]
public sealed class FollowingController : ControllerBase, IFollowingServices
{
    private readonly CommonServices _commonServices;
    private readonly ISessionResolver _sessionResolver;

    public FollowingController(CommonServices commonServices, ISessionResolver sessionResolver)
    {
        _commonServices = commonServices;
        _sessionResolver = sessionResolver;
    }

    [HttpPost]
    public Task<AccountFollowingStatus?> TryStartFollowing([FromBody] Following_TryStartFollowing command, CancellationToken cancellationToken = default)
    {
        command.UseDefaultSession(_sessionResolver);
        return _commonServices.FollowingServices.TryStartFollowing(command, cancellationToken);
    }

    [HttpPost]
    public Task<AccountFollowingStatus?> TryStopFollowing([FromBody] Following_TryStopFollowing command, CancellationToken cancellationToken = default)
    {
        command.UseDefaultSession(_sessionResolver);
        return _commonServices.FollowingServices.TryStopFollowing(command, cancellationToken);
    }

    [HttpPost]
    public Task<AccountFollowingStatus?> TryAcceptFollower([FromBody] Following_TryAcceptFollower command, CancellationToken cancellationToken = default)
    {
        command.UseDefaultSession(_sessionResolver);
        return _commonServices.FollowingServices.TryAcceptFollower(command, cancellationToken);
    }

    [HttpPost]
    public Task<AccountFollowingStatus?> TryRemoveFollower([FromBody] Following_TryRemoveFollower command, CancellationToken cancellationToken = default)
    {
        command.UseDefaultSession(_sessionResolver);
        return _commonServices.FollowingServices.TryRemoveFollower(command, cancellationToken);
    }
}