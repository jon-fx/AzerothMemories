namespace AzerothMemories.WebBlazor.Services;

public interface IPostServices : IComputeService
{
    Task<AddMemoryResult> TryPostMemory(Session session, byte[] toArray);

    [ComputeMethod]
    Task<PostViewModel> TryGetPostViewModel(Session session, int accountId, int postId, ServerSideLocale locale);

    [CommandHandler]
    Task<int> TryReactToPost(Post_TryReactToPost command, CancellationToken cancellationToken = default);

    [ComputeMethod]
    Task<PostReactionViewModel[]> TryGetReactions(Session session, int postId);

    [ComputeMethod]
    Task<PostCommentPageViewModel> TryGetCommentsPage(Session session, int postId, int page = 0, int focusedCommentId = 0);

    [ComputeMethod]
    Task<PostReactionViewModel[]> TryGetCommentReactionData(Session session, int postId, int commentId);

    [ComputeMethod]
    Task<Dictionary<int, PostCommentReactionViewModel>> TryGetMyCommentReactions(Session session, int postId);

    [CommandHandler]
    Task<bool> TryRestoreMemory(Post_TryRestoreMemory command, CancellationToken cancellationToken = default);

    [CommandHandler]
    Task<int> TryPublishComment(Post_TryPublishComment command, CancellationToken cancellationToken = default);

    [CommandHandler]
    Task<int> TryReactToPostComment(Post_TryReactToPostComment command, CancellationToken cancellationToken = default);

    [CommandHandler]
    Task<byte?> TrySetPostVisibility(Post_TrySetPostVisibility command, CancellationToken cancellationToken = default);

    [CommandHandler]
    Task<long> TryDeletePost(Post_TryDeletePost command, CancellationToken cancellationToken = default);

    [CommandHandler]
    Task<long> TryDeleteComment(Post_TryDeleteComment command, CancellationToken cancellationToken = default);

    [CommandHandler]
    Task<bool> TryReportPost(Post_TryReportPost command, CancellationToken cancellationToken = default);

    [CommandHandler]
    Task<bool> TryReportPostComment(Post_TryReportPostComment command, CancellationToken cancellationToken = default);

    [CommandHandler]
    Task<bool> TryReportPostTags(Post_TryReportPostTags command, CancellationToken cancellationToken = default);

    [CommandHandler]
    Task<AddMemoryResultCode> TryUpdateSystemTags(Post_TryUpdateSystemTags command, CancellationToken cancellationToken = default);
}