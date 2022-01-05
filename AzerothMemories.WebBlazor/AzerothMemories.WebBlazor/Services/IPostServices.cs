namespace AzerothMemories.WebBlazor.Services;

[BasePath("post")]
public interface IPostServices
{
    [Post(nameof(TryPostMemory))]
    Task<(AddMemoryResult Result, long PostId)> TryPostMemory(Session session, [Body] AddMemoryTransferData transferData);
}