namespace AzerothMemories.WebBlazor.Services;

[BasePath("search")]
public interface ISearchServices
{
    [ComputeMethod]
    [Get(nameof(TryGetDailyActivity))]
    Task<DailyActivityResults> TryGetDailyActivity(Session session, [Query] string timeZoneId, [Query] byte inZoneDay, [Query] byte inZoneMonth, [Query] ServerSideLocale locale);

    [ComputeMethod]
    [Get(nameof(TryGetDailyActivityFull))]
    Task<DailyActivityResults[]> TryGetDailyActivityFull(Session session, [Query] string timeZoneId, [Query] byte inZoneDay, [Query] byte inZoneMonth, [Query] ServerSideLocale locale);

    [ComputeMethod]
    [Get(nameof(TrySearch))]
    Task<MainSearchResult[]> TrySearch(Session session, [Query] MainSearchType searchType, [Query] string searchString);

    [ComputeMethod]
    [Get(nameof(TryGetRecentPosts))]
    Task<RecentPostsResults> TryGetRecentPosts(Session session, [Query] RecentPostsType postsType, [Query] PostSortMode sortMode, [Query] int currentPage, [Query] ServerSideLocale locale);

    [ComputeMethod]
    [Get(nameof(TrySearchPosts))]
    Task<SearchPostsResults> TrySearchPosts(Session session, [Query] string[] tagStrings, [Query] PostSortMode sortMode, [Query] int currentPage, [Query] long postMinTime, [Query] long postMaxTime, [Query] ServerSideLocale locale);
}