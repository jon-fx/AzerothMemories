namespace AzerothMemories.WebBlazor.Services;

[BasePath("tag")]
public interface ITagServices : IComputeService
{
    [ComputeMethod]
    [Get(nameof(Search) + "/{searchString}")]
    Task<PostTagInfo[]> Search(Session session, [Path] string searchString, [Query] ServerSideLocale locale);
}