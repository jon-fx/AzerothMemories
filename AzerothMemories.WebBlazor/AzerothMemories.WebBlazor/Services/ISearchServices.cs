namespace AzerothMemories.WebBlazor.Services;

public interface ISearchServices : IComputeService
{
    [ComputeMethod]
    Task<DailyActivityResults> TryGetDailyActivity(Session session, string timeZoneId, byte inZoneDay, byte inZoneMonth, ServerSideLocale locale);

    [ComputeMethod]
    Task<DailyActivityResults[]> TryGetDailyActivityFull(Session session, string timeZoneId, byte inZoneDay, byte inZoneMonth, ServerSideLocale locale);

    [ComputeMethod]
    Task<MainSearchResult[]> TrySearch(Session session, MainSearchType searchType, string searchString);

    [ComputeMethod]
    Task<RecentPostsResults> TryGetRecentPosts(Session session, RecentPostsType postsType, PostSortMode sortMode, int currentPage, ServerSideLocale locale);

    [ComputeMethod]
    Task<SearchPostsResults> TrySearchPosts(Session session, string[] tagStrings, PostSortMode sortMode, int currentPage, long postMinTime, long postMaxTime, ServerSideLocale locale);
}