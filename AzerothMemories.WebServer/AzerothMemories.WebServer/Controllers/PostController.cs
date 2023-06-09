namespace AzerothMemories.WebServer.Controllers;

[ApiController]
[JsonifyErrors]
[Route("api/[controller]/[action]")]
public sealed class PostController : ControllerBase, IPostServices
{
    private readonly CommonServices _commonServices;

    public PostController(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    [HttpPost]
    public Task<AddMemoryResult> TryPostMemory([FromBody] Post_TryPostMemory command, CancellationToken cancellationToken = default)
    {
        return _commonServices.Commander.Call(command, cancellationToken);
    }

    [HttpGet("{accountId}/{postId}")]
    public Task<PostViewModel> TryGetPostViewModel(Session session, [FromRoute] int accountId, [FromRoute] int postId, [FromQuery] ServerSideLocale locale)
    {
        return _commonServices.PostServices.TryGetPostViewModel(session, accountId, postId, locale);
    }

    [HttpPost]
    public Task<int> TryReactToPost([FromBody] Post_TryReactToPost command, CancellationToken cancellationToken = default)
    {
        return _commonServices.Commander.Call(command, cancellationToken);
    }

    [HttpGet("{postId}")]
    public Task<PostReactionViewModel[]> TryGetReactions(Session session, [FromRoute] int postId)
    {
        return _commonServices.PostServices.TryGetReactions(session, postId);
    }

    [HttpGet("{postId}")]
    public Task<PostCommentPageViewModel> TryGetCommentsPage(Session session, [FromRoute] int postId, [FromQuery] int page = 0, [FromQuery] int focusedCommentId = 0)
    {
        return _commonServices.PostServices.TryGetCommentsPage(session, postId, page, focusedCommentId);
    }

    [HttpGet("{postId}/{commentId}")]
    public Task<PostReactionViewModel[]> TryGetCommentReactionData(Session session, [FromRoute] int postId, [FromRoute] int commentId)
    {
        return _commonServices.PostServices.TryGetCommentReactionData(session, postId, commentId);
    }

    [HttpGet("{postId}")]
    public Task<Dictionary<int, PostCommentReactionViewModel>> TryGetMyCommentReactions(Session session, [FromRoute] int postId)
    {
        return _commonServices.PostServices.TryGetMyCommentReactions(session, postId);
    }

    [HttpPost]
    public Task<bool> TryRestoreMemory([FromBody] Post_TryRestoreMemory command, CancellationToken cancellationToken = default)
    {
        return _commonServices.Commander.Call(command, cancellationToken);
    }

    [HttpPost]
    public Task<int> TryPublishComment([FromBody] Post_TryPublishComment command, CancellationToken cancellationToken = default)
    {
        return _commonServices.Commander.Call(command, cancellationToken);
    }

    [HttpPost]
    public Task<int> TryReactToPostComment([FromBody] Post_TryReactToPostComment command, CancellationToken cancellationToken = default)
    {
        return _commonServices.Commander.Call(command, cancellationToken);
    }

    [HttpPost]
    public Task<byte?> TrySetPostVisibility([FromBody] Post_TrySetPostVisibility command, CancellationToken cancellationToken = default)
    {
        return _commonServices.Commander.Call(command, cancellationToken);
    }

    [HttpPost]
    public Task<long> TryDeletePost([FromBody] Post_TryDeletePost command, CancellationToken cancellationToken = default)
    {
        return _commonServices.Commander.Call(command, cancellationToken);
    }

    [HttpPost]
    public Task<long> TryDeleteComment([FromBody] Post_TryDeleteComment command, CancellationToken cancellationToken = default)
    {
        return _commonServices.Commander.Call(command, cancellationToken);
    }

    [HttpPost]
    public Task<bool> TryReportPost([FromBody] Post_TryReportPost command, CancellationToken cancellationToken = default)
    {
        return _commonServices.Commander.Call(command, cancellationToken);
    }

    [HttpPost]
    public Task<bool> TryReportPostComment([FromBody] Post_TryReportPostComment command, CancellationToken cancellationToken = default)
    {
        return _commonServices.Commander.Call(command, cancellationToken);
    }

    [HttpPost]
    public Task<bool> TryReportPostTags([FromBody] Post_TryReportPostTags command, CancellationToken cancellationToken = default)
    {
        return _commonServices.Commander.Call(command, cancellationToken);
    }

    [HttpPost]
    public Task<AddMemoryResultCode> TryUpdateSystemTags([FromBody] Post_TryUpdateSystemTags command, CancellationToken cancellationToken = default)
    {
        return _commonServices.Commander.Call(command, cancellationToken);
    }
}