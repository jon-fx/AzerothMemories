﻿namespace AzerothMemories.WebServer.Services;

[RegisterComputeService]
[RegisterAlias(typeof(ISearchServices))]
public class SearchServices : DbServiceBase<AppDbContext>, ISearchServices
{
    private readonly CommonServices _commonServices;

    public SearchServices(IServiceProvider services, CommonServices commonServices) : base(services)
    {
        _commonServices = commonServices;
    }

    [ComputeMethod]
    public virtual async Task<MainSearchResult[]> TrySearch(Session session, MainSearchType searchType, string searchString)
    {
        return await TrySearch(searchType, searchString);
    }

    [ComputeMethod]
    protected virtual async Task<MainSearchResult[]> TrySearch(MainSearchType searchType, string searchString)
    {
        if (string.IsNullOrWhiteSpace(searchString) || searchString.Length < 1)
        {
            return Array.Empty<MainSearchResult>();
        }

        searchString = DatabaseHelpers.GetSearchableName(searchString);
        if (searchString.Length < 1)
        {
            return Array.Empty<MainSearchResult>();
        }

        var allResults = new List<MainSearchResult>();
        if ((searchType & MainSearchType.Account) == MainSearchType.Account)
        {
            var results = await TrySearchAccounts(searchString);
            allResults.AddRange(results);
        }

        if ((searchType & MainSearchType.Character) == MainSearchType.Character)
        {
            var results = await TrySearchCharacters(searchString);
            allResults.AddRange(results);
        }

        if ((searchType & MainSearchType.Guild) == MainSearchType.Guild)
        {
            var results = await TrySearchGuilds(searchString);
            allResults.AddRange(results);
        }

        return allResults.ToArray();
    }

    [ComputeMethod(AutoInvalidateTime = 60)]
    protected virtual async Task<MainSearchResult[]> TrySearchAccounts(string searchString)
    {
        await using var database = CreateDbContext();
        var query = from r in database.Accounts
                    where r.UsernameSearchable.StartsWith(searchString)
                    orderby r.UsernameSearchable.Length
                    select MainSearchResult.CreateAccount(r.Id, r.Username, r.Avatar);

        var results = await query.Take(50).ToArrayAsync();
        return results;
    }

    [ComputeMethod(AutoInvalidateTime = 60)]
    protected virtual async Task<MainSearchResult[]> TrySearchCharacters(string searchString)
    {
        await using var database = CreateDbContext();
        var query = from r in database.Characters
                    where r.NameSearchable.StartsWith(searchString)
                    orderby r.NameSearchable.Length
                    select MainSearchResult.CreateCharacter(r.Id, r.MoaRef, r.Name, r.AvatarLink, r.RealmId, r.Class);

        var results = await query.Take(50).ToArrayAsync();
        return results;
    }

    [ComputeMethod(AutoInvalidateTime = 60)]
    protected virtual async Task<MainSearchResult[]> TrySearchGuilds(string searchString)
    {
        await using var database = CreateDbContext();
        var query = from r in database.Guilds
                    where r.NameSearchable.StartsWith(searchString)
                    orderby r.NameSearchable.Length
                    select MainSearchResult.CreateGuild(r.Id, r.MoaRef, r.Name, null, r.RealmId);

        var results = await query.Take(50).ToArrayAsync();
        return results;
    }

    [ComputeMethod]
    public virtual async Task<RecentPostsResults> TryGetRecentPosts(Session session, RecentPostsType postsType, PostSortMode sortMode, int currentPage, string locale)
    {
        var account = await _commonServices.AccountServices.TryGetActiveAccount(session);
        var allSearchResult = Array.Empty<long>();
        if (account != null && postsType == RecentPostsType.Default)
        {
            allSearchResult = await TryGetRecentPosts(account.Id);
        }

        if (allSearchResult.Length == 0)
        {
            allSearchResult = await TryGetRecentPosts();
        }

        var allPostViewModels = Array.Empty<PostViewModel>();
        var totalPages = (int)Math.Ceiling(allSearchResult.Length / (float)CommonConfig.PostsPerPage);
        if (allSearchResult.Length > 0)
        {
            currentPage = Math.Clamp(currentPage, 1, totalPages);
            allPostViewModels = await GetPostViewModelsForPage(session, allSearchResult, currentPage, CommonConfig.PostsPerPage, locale);
        }

        return new RecentPostsResults
        {
            CurrentPage = currentPage,
            TotalPages = totalPages,
            SortMode = sortMode,
            PostsType = postsType,
            PostViewModels = allPostViewModels.ToArray(),
        };
    }

    [ComputeMethod]
    protected virtual async Task<long[]> TryGetRecentPosts()
    {
        await using var database = CreateDbContext();

        await _commonServices.PostServices.DependsOnNewPosts();

        var query = from p in database.Posts
                    where p.DeletedTimeStamp == 0 && p.PostVisibility == 0
                    orderby p.PostCreatedTime descending
                    select p.Id;

        return await query.ToArrayAsync();
    }

    [ComputeMethod]
    protected virtual async Task<long[]> TryGetRecentPosts(long accountId)
    {
        await using var database = CreateDbContext();

        var following = await _commonServices.FollowingServices.TryGetAccountFollowing(accountId);
        if (following == null || following.Count == 0)
        {
            return Array.Empty<long>();
        }

        var allFollowingIds = new HashSet<long> { accountId };
        foreach (var kvp in following)
        {
            if (kvp.Value.Status != AccountFollowingStatus.Active)
            {
                continue;
            }

            allFollowingIds.Add(kvp.Key);
        }

        foreach (var followingViewModel in allFollowingIds)
        {
            await _commonServices.PostServices.DependsOnPostsBy(followingViewModel);
        }

        var query = from p in database.Posts
                    where p.DeletedTimeStamp == 0 && allFollowingIds.Contains(p.AccountId)
                    orderby p.PostCreatedTime descending
                    select p.Id;

        return await query.ToArrayAsync();
    }

    [ComputeMethod]
    public virtual async Task<SearchPostsResults> TrySearchPosts(Session session, string[] tagStrings, PostSortMode sortMode, int currentPage, long postMinTime, long postMaxTime, string locale)
    {
        postMinTime = Math.Clamp(postMinTime, 0, SystemClock.Instance.GetCurrentInstant().ToUnixTimeMilliseconds());
        postMaxTime = Math.Clamp(postMaxTime, 0, SystemClock.Instance.GetCurrentInstant().ToUnixTimeMilliseconds());

        var searchPostTags = Array.Empty<PostTagInfo>();
        var serverSideTagStrings = new HashSet<string>();

        if (tagStrings != null)
        {
            var result = await GetPostSearchTags(tagStrings, locale);
            searchPostTags = result.Tags;
            serverSideTagStrings = result.Strings;
        }

        var allSearchResult = await TrySearchPosts(serverSideTagStrings, sortMode, postMinTime, postMaxTime);
        var allPostViewModels = Array.Empty<PostViewModel>();
        var totalPages = (int)Math.Ceiling(allSearchResult.Length / (float)CommonConfig.PostsPerPage);
        if (allSearchResult.Length > 0)
        {
            currentPage = Math.Clamp(currentPage, 1, totalPages);
            allPostViewModels = await GetPostViewModelsForPage(session, allSearchResult, currentPage, CommonConfig.PostsPerPage, locale);
        }

        return new SearchPostsResults
        {
            CurrentPage = currentPage,
            MinTime = postMinTime,
            MaxTime = postMaxTime,
            Tags = searchPostTags,
            TotalPages = totalPages,
            SortMode = sortMode,
            PostViewModels = allPostViewModels.ToArray(),
        };
    }

    private async Task<PostViewModel[]> GetPostViewModelsForPage(Session session, long[] allSearchResult, int currentPage, int postsPerPage, string locale)
    {
        var viewModels = new List<PostViewModel>();
        for (var i = (currentPage - 1) * postsPerPage; i < allSearchResult.Length; i++)
        {
            var postViewModel = await _commonServices.PostServices.TryGetPostViewModel(session, allSearchResult[i], locale);
            if (postViewModel == null)
            {
            }
            else
            {
                viewModels.Add(postViewModel);

                if (viewModels.Count >= postsPerPage)
                {
                    break;
                }
            }
        }

        return viewModels.ToArray();
    }

    [ComputeMethod]
    protected virtual async Task<(PostTagInfo[] Tags, HashSet<string> Strings)> GetPostSearchTags(string[] tagStrings, string locale)
    {
        var searchPostTags = new List<PostTagInfo>();
        var serverSideTagStrings = new HashSet<string>();

        foreach (var tagString in tagStrings)
        {
            if (!ZExtensions.ParseTagInfoFrom(tagString, out var postTagInfo))
            {
                continue;
            }

            var tagInfo = await _commonServices.TagServices.GetTagInfo(postTagInfo.Type, postTagInfo.Id, postTagInfo.Text, locale);
            if (tagInfo != null && serverSideTagStrings.Add(tagInfo.TagString))
            {
                searchPostTags.Add(tagInfo);
            }
        }

        return (searchPostTags.ToArray(), serverSideTagStrings);
    }

    [ComputeMethod]
    protected virtual async Task<long[]> TrySearchPosts(HashSet<string> tagStrings, PostSortMode sortMode, long minTime, long maxTime)
    {
        await using var database = CreateDbContext();

        foreach (var tagString in tagStrings)
        {
            await _commonServices.PostServices.DependsOnPostsWithTagString(tagString);
        }

        var query = from p in GetPostSearchQuery(database, tagStrings, sortMode, minTime, maxTime)
                    select p.Id;

        return await query.ToArrayAsync();
    }

    private IQueryable<PostRecord> GetPostSearchQuery(AppDbContext database, HashSet<string> serverSideTagStrings, PostSortMode sortMode, long minTimeStamp, long maxTimeStamp)
    {
        IQueryable<PostRecord> query;
        if (serverSideTagStrings.Count > 0)
        {
            var groupQuery = from tag in database.PostTags.Where(x => serverSideTagStrings.Contains(x.TagString) && (x.TagKind == PostTagKind.Post || x.TagKind == PostTagKind.PostComment || x.TagKind == PostTagKind.PostRestored))
                             group tag by tag.PostId into grp
                             where grp.Count() >= serverSideTagStrings.Count
                             select grp.Key;

            query = from id in groupQuery
                    from post in database.Posts.Where(x => x.Id == id && x.DeletedTimeStamp == 0)
                    select post;
        }
        else
        {
            query = from post in database.Posts.Where(x => x.DeletedTimeStamp == 0)
                    select post;
        }

        if (minTimeStamp > 0 && minTimeStamp == maxTimeStamp)
        {
            query = from post in query
                    where post.PostTime == Instant.FromUnixTimeMilliseconds(minTimeStamp)
                    select post;
        }
        else
        {
            if (minTimeStamp > 0)
            {
                query = from post in query
                        where post.PostTime >= Instant.FromUnixTimeMilliseconds(minTimeStamp)
                        select post;
            }

            if (maxTimeStamp > 0)
            {
                query = from post in query
                        where post.PostTime <= Instant.FromUnixTimeMilliseconds(maxTimeStamp)
                        select post;
            }
        }

        switch (sortMode)
        {
            case PostSortMode.PostTimeStampDescending:
            {
                query = from p in query
                        orderby p.PostTime descending
                        select p;
                break;
            }
            case PostSortMode.PostTimeStampAscending:
            {
                query = from p in query
                        orderby p.PostTime
                        select p;
                break;
            }
            case PostSortMode.PostCreatedTimeStampDescending:
            {
                query = from p in query
                        orderby p.PostCreatedTime descending
                        select p;
                break;
            }
            case PostSortMode.PostCreatedTimeStampAscending:
            {
                query = from p in query
                        orderby p.PostCreatedTime
                        select p;
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException(nameof(sortMode), sortMode, null);
            }
        }

        return query;
    }
}