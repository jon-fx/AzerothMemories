namespace AzerothMemories.WebServer.Controllers;

[ApiController, JsonifyErrors]
[Route("api/[controller]/[action]")]
public class PostController : ControllerBase, IPostServices
{
    private readonly CommonServices _commonServices;

    public PostController(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    [HttpPost]
    public Task<AddMemoryResult> TryPostMemory(Session session, [FromBody] AddMemoryTransferData transferData)
    {
        return _commonServices.PostServices.TryPostMemory(session, transferData);
    }

    [HttpGet("{accountId}/{postId}"), Publish]
    public Task<PostViewModel> TryGetPostViewModel(Session session, long accountId, long postId, CancellationToken cancellationToken = default)
    {
        return _commonServices.PostServices.TryGetPostViewModel(session, accountId, postId, cancellationToken);
    }
}