﻿using LinqToDB.Tools;

namespace AzerothMemories.WebServer.Services;

[RegisterComputeService]
[RegisterAlias(typeof(ISearchPostsServices))]
public class SearchPostsServices : ISearchPostsServices
{
    private readonly CommonServices _commonServices;

    public SearchPostsServices(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    [ComputeMethod]
    public virtual async Task<SearchPostsResults> TrySearchPosts(Session session, string[] tagStrings, PostSortMode sortMode, int currentPage, long postMinTime, long postMaxTime, string locale = null)
    {
        postMinTime = Math.Clamp(postMinTime, 0, SystemClock.Instance.GetCurrentInstant().ToUnixTimeMilliseconds());
        postMaxTime = Math.Clamp(postMaxTime, 0, SystemClock.Instance.GetCurrentInstant().ToUnixTimeMilliseconds());

        //{
        //    var min = Math.Min(postMinTime, postMaxTime);
        //    var max = Math.Max(postMinTime, postMaxTime);

        //    postMinTime = min;
        //    postMaxTime = max;
        //}

        var searchPostTags = Array.Empty<PostTagInfo>();
        var serverSideTagStrings = new HashSet<string>();

        if (tagStrings != null)
        {
            var result = await GetPostSearchTags(tagStrings, locale);
            searchPostTags = result.Tags;
            serverSideTagStrings = result.Strings;
        }

        var postsPerPage = 5;
        var allSearchResult = await TrySearchPosts(serverSideTagStrings, sortMode, postMinTime, postMaxTime);
        var allPostViewModels = new List<PostViewModel>();
        var totalPages = (int)Math.Ceiling(allSearchResult.Length / (float)postsPerPage);
        if (allSearchResult.Length > 0)
        {
            currentPage = Math.Clamp(currentPage, 1, totalPages);

            var pagedResults = allSearchResult.Skip((currentPage - 1) * postsPerPage).Take(postsPerPage).ToArray();
            foreach (var pagedResult in pagedResults)
            {
                var postViewModel = await _commonServices.PostServices.TryGetPostViewModel(session, pagedResult, locale);

                allPostViewModels.Add(postViewModel);
            }
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

            var tagInfo = await _commonServices.TagServices.GetTagInfo(postTagInfo.Type, postTagInfo.Id, locale);
            if (tagInfo != null && serverSideTagStrings.Add(tagInfo.TagString))
            {
                searchPostTags.Add(tagInfo);
            }
        }

        return (searchPostTags.ToArray(), serverSideTagStrings);
    }

    [ComputeMethod(AutoInvalidateTime = 60)]
    protected virtual async Task<long[]> TrySearchPosts(HashSet<string> tagStrings, PostSortMode sortMode, long minTime, long maxTime)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        var query = from p in GetPostSearchQuery(database, tagStrings, sortMode, minTime, maxTime)
                    select p.Id;

        return await query.ToArrayAsync();
    }

    private IQueryable<PostRecord> GetPostSearchQuery(DatabaseConnection database, HashSet<string> serverSideTagStrings, PostSortMode sortMode, long minTimeStamp, long maxTimeStamp)
    {
        IQueryable<PostRecord> query;
        if (serverSideTagStrings.Count > 0)
        {
            var groupQuery = from tag in database.PostTags.Where(x => x.TagString.In(serverSideTagStrings) && (x.TagKind == PostTagKind.Post || x.TagKind == PostTagKind.PostComment || x.TagKind == PostTagKind.PostRestored))
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
            case PostSortMode.PostTimeStampDesc:
            {
                query = from p in query
                        orderby p.PostTime descending, p.PostCreatedTime descending
                        select p;
                break;
            }
            case PostSortMode.PostCreatedTimeStamp:
            {
                query = from p in query
                        orderby p.PostCreatedTime descending
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