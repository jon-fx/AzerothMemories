namespace AzerothMemories.WebBlazor.Services;

[BasePath("post")]
public interface IPostServices
{
    [Post(nameof(TryPostMemory))]
    Task<AddMemoryResult> TryPostMemory(Session session, [Body] AddMemoryTransferData transferData);

    [ComputeMethod]
    [Get(nameof(TryGetPostViewModel) + "/{accountId}/{postId}")]
    Task<PostViewModel> TryGetPostViewModel(Session session, [Path] long accountId, [Path] long postId, CancellationToken cancellationToken = default);
}