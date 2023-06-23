namespace AzerothMemories.WebBlazor.Services;

public interface ITagServices : IComputeService
{
    [ComputeMethod]
    Task<PostTagInfo[]> Search(Session session, string searchString, ServerSideLocale locale);
}