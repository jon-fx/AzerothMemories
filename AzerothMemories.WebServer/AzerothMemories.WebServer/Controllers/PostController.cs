namespace AzerothMemories.WebServer.Controllers;

[ApiController, JsonifyErrors]
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
        return _commonServices.PostServices.TryPostMemory(command, cancellationToken);
    }

    [HttpGet("{accountId}/{postId}"), Publish]
    public Task<PostViewModel> TryGetPostViewModel(Session session, [FromRoute] long accountId, [FromRoute] long postId, [FromQuery] string locale)
    {
        return _commonServices.PostServices.TryGetPostViewModel(session, accountId, postId, locale);
    }

    [HttpPost]
    public Task<long> TryReactToPost([FromBody] Post_TryReactToPost command, CancellationToken cancellationToken = default)
    {
        return _commonServices.PostServices.TryReactToPost(command, cancellationToken);
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

    [HttpGet("{postId}/{commentId}"), Publish]
    public Task<PostReactionViewModel[]> TryGetCommentReactionData(Session session, [FromRoute] long postId, [FromRoute] long commentId)
    {
        return _commonServices.PostServices.TryGetCommentReactionData(session, postId, commentId);
    }

    [HttpGet("{postId}"), Publish]
    public Task<Dictionary<long, PostCommentReactionViewModel>> TryGetMyCommentReactions(Session session, [FromRoute] long postId)
    {
        return _commonServices.PostServices.TryGetMyCommentReactions(session, postId);
    }

    [HttpPost]
    public Task<bool> TryRestoreMemory([FromBody] Post_TryRestoreMemory command, CancellationToken cancellationToken = default)
    {
        return _commonServices.PostServices.TryRestoreMemory(command, cancellationToken);
    }

    [HttpPost]
    public Task<long> TryPublishComment([FromBody] Post_TryPublishComment command, CancellationToken cancellationToken = default)
    {
        return _commonServices.PostServices.TryPublishComment(command, cancellationToken);
    }

    [HttpPost]
    public Task<long> TryReactToPostComment([FromBody] Post_TryReactToPostComment command, CancellationToken cancellationToken = default)
    {
        return _commonServices.PostServices.TryReactToPostComment(command, cancellationToken);
    }

    [HttpPost]
    public Task<byte?> TrySetPostVisibility([FromBody] Post_TrySetPostVisibility command, CancellationToken cancellationToken = default)
    {
        return _commonServices.PostServices.TrySetPostVisibility(command, cancellationToken);
    }

    [HttpPost]
    public Task<long> TryDeletePost([FromBody] Post_TryDeletePost command, CancellationToken cancellationToken = default)
    {
        return _commonServices.PostServices.TryDeletePost(command, cancellationToken);
    }

    [HttpPost]
    public Task<long> TryDeleteComment([FromBody] Post_TryDeleteComment command, CancellationToken cancellationToken = default)
    {
        return _commonServices.PostServices.TryDeleteComment(command, cancellationToken);
    }

    [HttpPost]
    public Task<bool> TryReportPost([FromBody] Post_TryReportPost command, CancellationToken cancellationToken = default)
    {
        return _commonServices.PostServices.TryReportPost(command, cancellationToken);
    }

    [HttpPost]
    public Task<bool> TryReportPostComment([FromBody] Post_TryReportPostComment command, CancellationToken cancellationToken = default)
    {
        return _commonServices.PostServices.TryReportPostComment(command, cancellationToken);
    }

    [HttpPost]
    public Task<bool> TryReportPostTags([FromBody] Post_TryReportPostTags command, CancellationToken cancellationToken = default)
    {
        return _commonServices.PostServices.TryReportPostTags(command, cancellationToken);
    }

    [HttpPost]
    public Task<AddMemoryResultCode> TryUpdateSystemTags([FromBody] Post_TryUpdateSystemTags command, CancellationToken cancellationToken = default)
    {
        return _commonServices.PostServices.TryUpdateSystemTags(command, cancellationToken);
    }
}