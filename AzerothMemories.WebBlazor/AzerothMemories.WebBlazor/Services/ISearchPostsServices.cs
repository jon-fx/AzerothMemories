namespace AzerothMemories.WebBlazor.Services;

[BasePath("searchposts")]
public interface ISearchPostsServices
{
    [ComputeMethod]
    [Get(nameof(TrySearchPosts))]
    Task<SearchPostsResults> TrySearchPosts(Session session, [Query] string[] tagStrings, [Query] PostSortMode sortMode, [Query] int currentPage, [Query] long postMinTime, [Query] long postMaxTime, [Query] string locale = null);
}