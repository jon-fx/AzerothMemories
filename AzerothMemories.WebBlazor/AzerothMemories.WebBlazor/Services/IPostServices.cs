namespace AzerothMemories.WebBlazor.Services;

[BasePath("post")]
public interface IPostServices
{
    [Post(nameof(TryPostMemory))]
    Task<AddMemoryResult> TryPostMemory(Session session, [Body] AddMemoryTransferData transferData);

    [ComputeMethod]
    [Get(nameof(TryGetPostViewModel) + "/{accountId}/{postId}")]
    Task<PostViewModel> TryGetPostViewModel(Session session, [Path] long accountId, [Path] long postId, [Query] string locale = null, CancellationToken cancellationToken = default);

    [Post(nameof(TryReactToPost) + "/{postId}/{newReaction}")]
    Task<long> TryReactToPost(Session session, [Path] long postId, [Path] PostReaction newReaction);

    [ComputeMethod]
    [Get(nameof(TryGetReactions) + "/{postId}")]
    Task<PostReactionViewModel[]> TryGetReactions(Session session, [Path] long postId);

    [ComputeMethod]
    [Get(nameof(TryGetCommentsPage) + "/{postId}")]
    Task<PostCommentPageViewModel> TryGetCommentsPage(Session session, [Path] long postId, [Query] int page = 0, [Query] long focusedCommentId = 0);

    [ComputeMethod]
    [Get(nameof(TryGetCommentReactionData) + "/{postId}/{commentId}")]
    Task<PostReactionViewModel[]> TryGetCommentReactionData(Session session, [Path] long postId, [Path] long commentId);

    [ComputeMethod]
    [Get(nameof(TryGetMyCommentReactions) + "/{postId}")]
    Task<Dictionary<long, PostCommentReactionViewModel>> TryGetMyCommentReactions(Session session, [Path] long postId);

    [Post(nameof(TryRestoreMemory) + "/{postId}/{previousCharacterId}/{newCharacterId}")]
    Task<bool> TryRestoreMemory(Session session, [Path] long postId, [Path] long previousCharacterId, [Path] long newCharacterId);

    [Post(nameof(TryPublishComment) + "/{postId}/{parentCommentId}")]
    Task<long> TryPublishComment(Session session, [Path] long postId, [Path] long parentCommentId, [Body] AddCommentTransferData transferData);

    [Post(nameof(TryReactToPostComment) + "/{postId}/{commentId}/{newReaction}")]
    Task<long> TryReactToPostComment(Session session, [Path] long postId, [Path] long commentId, [Path] PostReaction newReaction);

    [Post(nameof(TrySetPostVisibility) + "/{postId}/{newVisibility}")]
    Task<byte?> TrySetPostVisibility(Session session, [Path] long postId, [Path] byte newVisibility);

    [Post(nameof(TryDeletePost) + "/{postId}")]
    Task<long> TryDeletePost(Session session, [Path] long postId);

    [Post(nameof(TryDeleteComment) + "/{postId}/{commentId}")]
    Task<long> TryDeleteComment(Session session, [Path] long postId, [Path] long commentId);
}