namespace AzerothMemories.WebServer.Services;

[RegisterComputeService]
[RegisterAlias(typeof(ISearchServices))]
public class SearchServices : DbServiceBase<AppDbContext>, ISearchServices
{
    private readonly CommonServices _commonServices;

    private readonly int _year2000 = 2000;
    private readonly int _totalYearValue = 0;
    private readonly int _activityStartYear = 1;
    private readonly int _activityEndYear = DateTime.UtcNow.Year - 2000;

    public SearchServices(IServiceProvider services, CommonServices commonServices) : base(services)
    {
        _commonServices = commonServices;
    }

    [ComputeMethod]
    public virtual async Task<DailyActivityResults> TryGetDailyActivity(Session session, string timeZoneId, long unixTime, string locale)
    {
        long accountId = 0;
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session);
        if (activeAccount != null)
        {
            accountId = activeAccount.Id;
        }

        return await TryGetDailyActivity(accountId, timeZoneId, unixTime, locale);
    }

    [ComputeMethod]
    public virtual async Task<DailyActivityResults> TryGetDailyActivity(long accountId, string timeZoneId, long unixTime, string locale)
    {
        var allResults = await TryGetActivity(timeZoneId, unixTime, locale);
        var userResults = new Dictionary<int, DailyActivityResultsUser>();
        if (accountId > 0)
        {
            userResults = await TryGetUserActivity(accountId, timeZoneId, unixTime, locale);
        }

        allResults.TryGetValue(_totalYearValue, out var mainActivity);
        userResults.TryGetValue(_totalYearValue, out var userActivity);

        return new DailyActivityResults { Year = _totalYearValue, Main = mainActivity, User = userActivity };
    }

    [ComputeMethod]
    public virtual async Task<DailyActivityResults[]> TryGetDailyActivityFull(Session session, string timeZoneId, long unixTime, string locale)
    {
        long accountId = 0;
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session);
        if (activeAccount != null)
        {
            accountId = activeAccount.Id;
        }

        return await TryGetDailyActivityFull(accountId, timeZoneId, unixTime, locale);
    }

    [ComputeMethod]
    public virtual async Task<DailyActivityResults[]> TryGetDailyActivityFull(long accountId, string timeZoneId, long unixTime, string locale)
    {
        var allResults = await TryGetActivity(timeZoneId, unixTime, locale);
        var userResults = new Dictionary<int, DailyActivityResultsUser>();
        if (accountId > 0)
        {
            userResults = await TryGetUserActivity(accountId, timeZoneId, unixTime, locale);
        }

        var resultList = new List<DailyActivityResults>();
        for (var i = _year2000; i < DateTime.UtcNow.Year; i++)
        {
            allResults.TryGetValue(i, out var mainActivity);
            userResults.TryGetValue(i, out var userActivity);

            var data = new DailyActivityResults { Year = i, Main = mainActivity, User = userActivity };
            if (data.Main == null && data.User == null)
            {
            }
            else
            {
                resultList.Add(data);
            }
        }

        allResults.TryGetValue(_totalYearValue, out var totalMainActivity);
        userResults.TryGetValue(_totalYearValue, out var totalUserActivity);

        resultList = resultList.OrderByDescending(x => x.Year).ToList();
        resultList.Insert(0, new DailyActivityResults { Year = _totalYearValue, Main = totalMainActivity, User = totalUserActivity });

        return resultList.ToArray();
    }

    [ComputeMethod]
    protected virtual async Task<Dictionary<int, DailyActivityResultsMain>> TryGetActivity(string timeZoneId, long unixTime, string locale)
    {
        var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId);
        if (timeZone == null)
        {
            return new Dictionary<int, DailyActivityResultsMain>();
        }

        var today = SystemClock.Instance.GetCurrentInstant().InZone(timeZone).Date;
        return await TryGetActivity(timeZoneId, locale, today);
    }

    [ComputeMethod]
    protected virtual async Task<Dictionary<int, DailyActivityResultsMain>> TryGetActivity(string timeZoneId, string locale, LocalDate today)
    {
        var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId);
        if (timeZone == null)
        {
            return new Dictionary<int, DailyActivityResultsMain>();
        }

        var results = new Dictionary<int, DailyActivityResultsMain>();
        var totals = new DailyActivityResultsMain { Year = _totalYearValue };
        var topValueCount = 10;
        var totalPostTagCounts = new Dictionary<string, int>();
        var totalAchievementCounts = new Dictionary<int, int>();
        for (var i = _activityStartYear; i < _activityEndYear; i++)
        {
            var inZone = SystemClock.Instance.GetCurrentInstant().InZone(timeZone).Date.Minus(Period.FromYears(i));

            var startTime = timeZone.AtStartOfDay(inZone);
            var endTime = timeZone.AtStartOfDay(inZone.PlusDays(1));

            var daily = new DailyActivityResultsMain
            {
                Year = inZone.Year,
                StartTimeMs = startTime.ToInstant().ToUnixTimeMilliseconds(),
                EndTimeMs = endTime.ToInstant().ToUnixTimeMilliseconds(),
            };

            var currentActivitySet = await TryGetActivitySet(daily.StartTimeMs, daily.EndTimeMs);
            if (currentActivitySet.AchievementCounts.Count == 0 && currentActivitySet.PostTags.Count == 0)
            {
                continue;
            }

            foreach (var kvp in currentActivitySet.PostTags)
            {
                totalPostTagCounts.TryGetValue(kvp.Key, out var currentValue);
                totalPostTagCounts[kvp.Key] = currentValue + kvp.Value;
            }

            foreach (var kvp in currentActivitySet.AchievementCounts)
            {
                totalAchievementCounts.TryGetValue(kvp.Key, out var currentValue);
                totalAchievementCounts[kvp.Key] = currentValue + kvp.Value;
            }

            var dailyTopAchievemnts = currentActivitySet.AchievementCounts.OrderByDescending(x => x.Value).Take(topValueCount);
            foreach (var kvp in dailyTopAchievemnts)
            {
                var tag = await _commonServices.TagServices.GetTagInfo(PostTagType.Achievement, kvp.Key, null, locale);
                daily.TopAchievements.Add(new ActivityResultsTuple(tag, kvp.Value));
            }

            var dailyTopTags = currentActivitySet.PostTags.OrderByDescending(x => x.Value).Take(topValueCount);
            foreach (var kvp in dailyTopTags)
            {
                if (!ZExtensions.ParseTagInfoFrom(kvp.Key, out var postTagInfo))
                {
                    continue;
                }

                var tag = await _commonServices.TagServices.GetTagInfo(postTagInfo.Type, postTagInfo.Id, postTagInfo.Text, locale);
                daily.TopTags.Add(new ActivityResultsTuple(tag, kvp.Value));
            }

            daily.TotalTags = currentActivitySet.PostTags.Count;
            daily.TotalAchievements = currentActivitySet.TotalAchievements;

            foreach (var firstTag in currentActivitySet.FirstTags)
            {
                if (!ZExtensions.ParseTagInfoFrom(firstTag, out var postTagInfo))
                {
                    continue;
                }

                var tag = await _commonServices.TagServices.GetTagInfo(postTagInfo.Type, postTagInfo.Id, postTagInfo.Text, locale);
                daily.FirstTags.Add(tag);
            }

            foreach (var firstAchievement in currentActivitySet.FirstAchievements)
            {
                var tag = await _commonServices.TagServices.GetTagInfo(PostTagType.Achievement, firstAchievement, null, locale);
                daily.FirstAchievements.Add(tag);
            }

            totals.TotalTags += daily.TotalTags;
            totals.TotalAchievements += daily.TotalAchievements;

            totals.FirstTags.AddRange(daily.FirstTags);
            totals.FirstAchievements.AddRange(daily.FirstAchievements);

            results.Add(daily.Year, daily);
        }

        var allTimeTopAchievement = totalAchievementCounts.OrderByDescending(x => x.Value).Take(topValueCount);
        foreach (var kvp in allTimeTopAchievement)
        {
            var tag = await _commonServices.TagServices.GetTagInfo(PostTagType.Achievement, kvp.Key, null, locale);
            totals.TopAchievements.Add(new ActivityResultsTuple(tag, kvp.Value));
        }

        var allTimeTopTags = totalPostTagCounts.OrderByDescending(x => x.Value).Take(topValueCount);
        foreach (var kvp in allTimeTopTags)
        {
            if (!ZExtensions.ParseTagInfoFrom(kvp.Key, out var postTagInfo))
            {
                continue;
            }

            var tag = await _commonServices.TagServices.GetTagInfo(postTagInfo.Type, postTagInfo.Id, postTagInfo.Text, locale);
            totals.TopTags.Add(new ActivityResultsTuple(tag, kvp.Value));
        }

        results.Add(totals.Year, totals);

        return results;
    }

    [ComputeMethod(AutoInvalidateTime = 60 * 10)]
    protected virtual async Task<ActivitySetMain> TryGetActivitySet(long startTimeMs, long endTimeMs)
    {
        await using var database = CreateDbContext();

        var dailyAchievementssQuery = from achievemnts in database.CharacterAchievements
                                      where achievemnts.AchievementTimeStamp >= startTimeMs && achievemnts.AchievementTimeStamp < endTimeMs
                                      select achievemnts;

        var dailyAchievementsIdQuery = from achievement in dailyAchievementssQuery
                                       group achievement by achievement.AchievementId into g
                                       select new
                                       {
                                           Id = g.Key,
                                           Count = g.Count()
                                       };

        var activitySet = new ActivitySetMain();
        var dailyAchievementsCount = await dailyAchievementssQuery.CountAsync();
        var dailyAchievementsById = await dailyAchievementsIdQuery.ToArrayAsync();

        foreach (var kvp in dailyAchievementsById)
        {
            activitySet.AchievementCounts[kvp.Id] = kvp.Count;
        }

        activitySet.TotalAchievements += dailyAchievementsCount;

        var dailyPostsQuery = from post in database.Posts.Include(p => p.PostTags)
                              where post.DeletedTimeStamp == 0 && post.PostTime >= Instant.FromUnixTimeMilliseconds(startTimeMs) && post.PostTime < Instant.FromUnixTimeMilliseconds(endTimeMs)
                              select post;

        var dailyPosts = await dailyPostsQuery.ToArrayAsync();
        foreach (var postRecord in dailyPosts)
        {
            foreach (var postTag in postRecord.PostTags)
            {
                if (postTag.TagKind != PostTagKind.Post)
                {
                    continue;
                }

                activitySet.PostTags.TryGetValue(postTag.TagString, out var tagCount);
                activitySet.PostTags[postTag.TagString] = tagCount + 1;
            }
        }

        var firstAchievementsQuery = from achievement in database.CharacterAchievements
                                     group achievement by achievement.AchievementId into g
                                     where g.Min(e => e.AchievementTimeStamp) >= startTimeMs && g.Min(e => e.AchievementTimeStamp) < endTimeMs
                                     select new
                                     {
                                         Id = g.Key,
                                     };

        var firstAchievements = await firstAchievementsQuery.ToArrayAsync();
        foreach (var firstAchievement in firstAchievements)
        {
            activitySet.FirstAchievements.Add(firstAchievement.Id);
        }

        var firstTagsQuery = from tag in database.PostTags
                             join post in database.Posts
                                 on tag.PostId equals post.Id
                             where post.DeletedTimeStamp == 0 && tag.TagKind == PostTagKind.Post
                             select new
                             {
                                 post.PostTime,
                                 tag.TagString,
                             };

        var firstTagsGroupedQuery = from kvp in firstTagsQuery
                                    group kvp by kvp.TagString into g
                                    where g.Min(e => e.PostTime) >= Instant.FromUnixTimeMilliseconds(startTimeMs) && g.Min(e => e.PostTime) < Instant.FromUnixTimeMilliseconds(endTimeMs)
                                    select new
                                    {
                                        Id = g.Key,
                                    };

        var firstTags = await firstTagsGroupedQuery.ToArrayAsync();
        foreach (var firstTag in firstTags)
        {
            activitySet.FirstTags.Add(firstTag.Id);
        }

        return activitySet;
    }

    [ComputeMethod]
    protected virtual async Task<Dictionary<int, DailyActivityResultsUser>> TryGetUserActivity(long accountId, string timeZoneId, long unixTime, string locale)
    {
        var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId);
        if (timeZone == null)
        {
            return null;
        }

        var today = SystemClock.Instance.GetCurrentInstant().InZone(timeZone).Date;
        return await TryGetUserActivity(accountId, timeZoneId, locale, today);
    }

    [ComputeMethod]
    protected virtual async Task<Dictionary<int, DailyActivityResultsUser>> TryGetUserActivity(long accountId, string timeZoneId, string locale, LocalDate today)
    {
        var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId);
        if (timeZone == null)
        {
            return new Dictionary<int, DailyActivityResultsUser>();
        }

        var results = new Dictionary<int, DailyActivityResultsUser>();
        var totals = new DailyActivityResultsUser { AccountId = accountId, Year = _totalYearValue };
        for (var i = _activityStartYear; i < _activityEndYear; i++)
        {
            var inZone = SystemClock.Instance.GetCurrentInstant().InZone(timeZone).Date.Minus(Period.FromYears(i));

            var startTime = timeZone.AtStartOfDay(inZone);
            var endTime = timeZone.AtStartOfDay(inZone.PlusDays(1));

            var daily = new DailyActivityResultsUser
            {
                Year = inZone.Year,
                AccountId = accountId,
                StartTimeMs = startTime.ToInstant().ToUnixTimeMilliseconds(),
                EndTimeMs = endTime.ToInstant().ToUnixTimeMilliseconds(),
            };

            var currentActivitySet = await TryGetUserActivitySet(accountId, daily.StartTimeMs, daily.EndTimeMs);
            if (currentActivitySet.Achievements.Count == 0 && currentActivitySet.FirstAchievements.Count == 0 && currentActivitySet.FirstTags.Count == 0 && currentActivitySet.MyMemories.Count == 0)
            {
                continue;
            }

            foreach (var achievement in currentActivitySet.Achievements)
            {
                var tag = await _commonServices.TagServices.GetTagInfo(PostTagType.Achievement, achievement, null, locale);
                daily.Achievements.Add(tag);
            }

            foreach (var achievement in currentActivitySet.FirstAchievements)
            {
                var tag = await _commonServices.TagServices.GetTagInfo(PostTagType.Achievement, achievement, null, locale);
                daily.FirstAchievements.Add(tag);
            }

            daily.MyMemories.AddRange(currentActivitySet.MyMemories.OrderByDescending(x => x.PostTime));

            totals.Achievements.AddRange(daily.Achievements);
            totals.FirstAchievements.AddRange(daily.FirstAchievements);
            totals.MyMemories.AddRange(daily.MyMemories);

            results.Add(daily.Year, daily);
        }

        results.Add(totals.Year, totals);

        return results;
    }

    [ComputeMethod(AutoInvalidateTime = 60 * 10)]
    protected virtual async Task<ActivitySetUser> TryGetUserActivitySet(long accountId, long startTimeMs, long endTimeMs)
    {
        await using var database = CreateDbContext();

        var dailyAchievementssQuery = from achievement in database.CharacterAchievements
                                      where achievement.AccountId == accountId && achievement.AchievementTimeStamp >= startTimeMs && achievement.AchievementTimeStamp < endTimeMs
                                      select achievement.AchievementId;

        var activitySet = new ActivitySetUser();
        var dailyAchievementsId = await dailyAchievementssQuery.ToArrayAsync();

        foreach (var kvp in dailyAchievementsId)
        {
            activitySet.Achievements.Add(kvp);
        }

        var firstAchievementsQuery = from achievement in database.CharacterAchievements
                                     where achievement.AccountId == accountId
                                     group achievement by achievement.AchievementId into g
                                     where g.Min(e => e.AchievementTimeStamp) >= startTimeMs && g.Min(e => e.AchievementTimeStamp) < endTimeMs
                                     select new
                                     {
                                         Id = g.Key,
                                     };

        var firstAchievements = await firstAchievementsQuery.ToArrayAsync();
        foreach (var firstAchievement in firstAchievements)
        {
            activitySet.FirstAchievements.Add(firstAchievement.Id);
        }

        var firstTagsQuery = from tag in database.PostTags
                             join post in database.Posts
                                 on tag.PostId equals post.Id
                             where post.AccountId == accountId && post.DeletedTimeStamp == 0 && tag.TagKind == PostTagKind.Post
                             select new
                             {
                                 post.PostTime,
                                 tag.TagString,
                             };

        var firstTagsGroupedQuery = from kvp in firstTagsQuery
                                    group kvp by kvp.TagString into g
                                    where g.Min(e => e.PostTime) >= Instant.FromUnixTimeMilliseconds(startTimeMs) && g.Min(e => e.PostTime) < Instant.FromUnixTimeMilliseconds(endTimeMs)
                                    select new
                                    {
                                        Id = g.Key,
                                    };

        var firstTags = await firstTagsGroupedQuery.ToArrayAsync();
        foreach (var firstTag in firstTags)
        {
            activitySet.FirstTags.Add(firstTag.Id);
        }

        var memoriesQuery = from tag in database.PostTags
                            join post in database.Posts
                                on tag.PostId equals post.Id
                            where post.DeletedTimeStamp == 0 && (tag.TagKind == PostTagKind.Post || tag.TagKind == PostTagKind.PostRestored) && post.PostTime >= Instant.FromUnixTimeMilliseconds(startTimeMs) && post.PostTime < Instant.FromUnixTimeMilliseconds(endTimeMs)
                            select new { post.Id, post.AccountId, post.PostTime, post.PostComment, post.BlobNames };

        var memories = await memoriesQuery.ToArrayAsync();
        var memoriesById = new HashSet<long>();
        foreach (var memory in memories)
        {
            if (!memoriesById.Add(memory.Id))
            {
                continue;
            }

            var userTagInfo = await _commonServices.TagServices.TryGetUserTagInfo(PostTagType.Account, memory.AccountId);
            var blobNames = Array.Empty<string>();
            if (string.IsNullOrEmpty(memory.BlobNames))
            {
            }
            else
            {
                blobNames = memory.BlobNames.Split('|');
            }

            activitySet.MyMemories.Add(new DailyActivityResultsUserPostInfo
            {
                PostId = memory.Id,
                AccountId = memory.AccountId,
                PostTime = memory.PostTime.ToUnixTimeMilliseconds(),
                BlobInfo = PostViewModelBlobInfo.CreateBlobInfo(userTagInfo.Name, memory.PostComment, blobNames),
            });
        }

        return activitySet;
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