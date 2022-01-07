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

    [Post(nameof(TryRestoreMemory) + "/{postId}/{previousCharacterId}/{newCharacterId}")]
    Task<bool> TryRestoreMemory(Session session, [Path] long postId, [Path] long previousCharacterId, [Path] long newCharacterId);
}