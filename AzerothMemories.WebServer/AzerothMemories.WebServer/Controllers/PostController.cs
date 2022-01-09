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
    public Task<PostViewModel> TryGetPostViewModel(Session session, [FromRoute] long accountId, [FromRoute] long postId, [FromQuery] string locale = null, CancellationToken cancellationToken = default)
    {
        return _commonServices.PostServices.TryGetPostViewModel(session, accountId, postId, locale, cancellationToken);
    }

    [HttpPost("{postId}/{newReaction}")]
    public Task<long> TryReactToPost(Session session, [FromRoute] long postId, [FromRoute] PostReaction newReaction)
    {
        return _commonServices.PostServices.TryReactToPost(session, postId, newReaction);
    }

    [HttpGet("{postId}"), Publish]
    public Task<PostReactionViewModel[]> TryGetReactions(Session session, [FromRoute] long postId)
    {
        return _commonServices.PostServices.TryGetReactions(session, postId);
    }

    [HttpGet("{postId}"), Publish]
    public Task<PostCommentPageViewModel> TryGetCommentsPage(Session session, [FromRoute] long postId, [FromQuery] int page = 0, [FromQuery] long focusedCommentId = 0)
    {
        return _commonServices.PostServices.TryGetCommentsPage(session, postId, page, focusedCommentId);
    }

    [HttpGet("{postId}"), Publish]
    public Task<Dictionary<long, PostCommentReactionViewModel>> TryGetMyCommentReactions(Session session, [FromRoute] long postId)
    {
        return _commonServices.PostServices.TryGetMyCommentReactions(session, postId);
    }

    [HttpPost("{postId}/{previousCharacterId}/{newCharacterId}")]
    public Task<bool> TryRestoreMemory(Session session, [FromRoute] long postId, [FromRoute] long previousCharacterId, [FromRoute] long newCharacterId)
    {
        return _commonServices.PostServices.TryRestoreMemory(session, postId, previousCharacterId, newCharacterId);
    }

    [HttpPost("{postId}/{parentCommentId}")]
    public Task<long> TryPublishComment(Session session, [FromRoute] long postId, [FromRoute] long parentCommentId, [FromBody] AddCommentTransferData transferData)
    {
        return _commonServices.PostServices.TryPublishComment(session, postId, parentCommentId, transferData);
    }
}