namespace AzerothMemories.WebBlazor.Services;

[BasePath("searchposts")]
public interface ISearchPostsServices
{
    [ComputeMethod]
    [Get(nameof(TryGetRecentPosts))]
    Task<RecentPostsResults> TryGetRecentPosts(Session session, [Query] RecentPostsType postsType, [Query] PostSortMode sortMode, [Query] int currentPage, [Query] string locale);

    [ComputeMethod]
    [Get(nameof(TrySearchPosts))]
    Task<SearchPostsResults> TrySearchPosts(Session session, [Query] string[] tagStrings, [Query] PostSortMode sortMode, [Query] int currentPage, [Query] long postMinTime, [Query] long postMaxTime, [Query] string locale);
}