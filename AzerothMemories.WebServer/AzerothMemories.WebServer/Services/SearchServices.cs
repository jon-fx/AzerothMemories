using LinqKit;

namespace AzerothMemories.WebServer.Services;

[RegisterComputeService]
[RegisterAlias(typeof(ISearchServices))]
public class SearchServices : DbServiceBase<AppDbContext>, ISearchServices
{
    private readonly CommonServices _commonServices;

    private readonly int _startYear = 2000;
    private readonly int _endYear = 2023;
    private readonly int _totalYearValue = 0;

    public SearchServices(IServiceProvider services, CommonServices commonServices) : base(services)
    {
        _commonServices = commonServices;
    }

    [ComputeMethod]
    public virtual async Task<DailyActivityResults> TryGetDailyActivity(Session session, string timeZoneId, byte inZoneDay, byte inZoneMonth, ServerSideLocale locale)
    {
        var accountId = 0;
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session).ConfigureAwait(false);
        if (activeAccount != null)
        {
            accountId = activeAccount.Id;
        }

        var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId);
        if (timeZone == null)
        {
            timeZoneId = DateTimeZone.Utc.Id;
        }

        inZoneDay = Math.Clamp(inZoneDay, (byte)1, (byte)31);
        inZoneMonth = Math.Clamp(inZoneMonth, (byte)1, (byte)12);

        return await TryGetDailyActivity(accountId, timeZoneId, inZoneDay, inZoneMonth, locale).ConfigureAwait(false);
    }

    [ComputeMethod]
    protected virtual async Task<DailyActivityResults> TryGetDailyActivity(int accountId, string timeZoneId, byte inZoneDay, byte inZoneMonth, ServerSideLocale locale)
    {
        var allResults = await TryGetDailyActivityFull(timeZoneId, inZoneDay, inZoneMonth, locale).ConfigureAwait(false);
        var userResults = new Dictionary<int, DailyActivityResultsUser>();
        if (accountId > 0)
        {
            userResults = await TryGetUserActivityFull(accountId, timeZoneId, inZoneDay, inZoneMonth, locale).ConfigureAwait(false);
        }

        allResults.TryGetValue(_totalYearValue, out var mainActivity);
        userResults.TryGetValue(_totalYearValue, out var userActivity);

        return new DailyActivityResults { Year = _totalYearValue, Main = mainActivity, User = userActivity };
    }

    [ComputeMethod]
    public virtual async Task<DailyActivityResults[]> TryGetDailyActivityFull(Session session, string timeZoneId, byte inZoneDay, byte inZoneMonth, ServerSideLocale locale)
    {
        var accountId = 0;
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session).ConfigureAwait(false);
        if (activeAccount != null)
        {
            accountId = activeAccount.Id;
        }

        var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId);
        if (timeZone == null)
        {
            timeZoneId = DateTimeZone.Utc.Id;
        }

        inZoneDay = Math.Clamp(inZoneDay, (byte)1, (byte)31);
        inZoneMonth = Math.Clamp(inZoneMonth, (byte)1, (byte)12);

        return await TryGetDailyActivityFull(accountId, timeZoneId, inZoneDay, inZoneMonth, locale).ConfigureAwait(false);
    }

    [ComputeMethod]
    protected virtual async Task<DailyActivityResults[]> TryGetDailyActivityFull(int accountId, string timeZoneId, byte inZoneDay, byte inZoneMonth, ServerSideLocale locale)
    {
        var allResults = await TryGetDailyActivityFull(timeZoneId, inZoneDay, inZoneMonth, locale).ConfigureAwait(false);
        var userResults = new Dictionary<int, DailyActivityResultsUser>();
        if (accountId > 0)
        {
            userResults = await TryGetUserActivityFull(accountId, timeZoneId, inZoneDay, inZoneMonth, locale).ConfigureAwait(false);
        }

        var resultList = new List<DailyActivityResults>();
        for (var i = _startYear; i < _endYear; i++)
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
    protected virtual async Task<Dictionary<int, DailyActivityResultsMain>> TryGetDailyActivityFull(string timeZoneId, byte inZoneDay, byte inZoneMonth, ServerSideLocale locale)
    {
        var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId);
        if (timeZone == null)
        {
            return new Dictionary<int, DailyActivityResultsMain>();
        }

        var topValueCount = 10;
        var results = new Dictionary<int, DailyActivityResultsMain>();
        var totals = new DailyActivityResultsMain { Year = _totalYearValue };
        for (var year = _startYear; year < _endYear; year++)
        {
            var currentActivitySet = await TryGetMainActivitySet(timeZoneId, inZoneDay, inZoneMonth, year).ConfigureAwait(false);
            if (currentActivitySet.AchievementCounts.Count == 0 && currentActivitySet.PostTags.Count == 0)
            {
                continue;
            }

            var daily = new DailyActivityResultsMain
            {
                Year = year,
                ZoneId = timeZoneId,
                StartTimeMs = currentActivitySet.StartTime.ToUnixTimeMilliseconds(),
                EndTimeMs = currentActivitySet.EndTime.ToUnixTimeMilliseconds(),
            };

            var dailyTopAchievements = currentActivitySet.AchievementCounts.OrderByDescending(x => x.Value).Take(topValueCount);
            foreach (var kvp in dailyTopAchievements)
            {
                var tag = await _commonServices.TagServices.GetTagInfo(PostTagType.Achievement, kvp.Key, null, locale).ConfigureAwait(false);
                daily.TopAchievements.Add(tag);
            }

            var dailyTopTags = currentActivitySet.PostTags.OrderByDescending(x => x.Value).Take(topValueCount);
            foreach (var kvp in dailyTopTags)
            {
                if (!ZExtensions.ParseTagInfoFrom(kvp.Key, out var postTagInfo))
                {
                    continue;
                }

                var tag = await _commonServices.TagServices.GetTagInfo(postTagInfo.Type, postTagInfo.Id, postTagInfo.Text, locale).ConfigureAwait(false);
                daily.TopTags.Add(tag);
            }

            daily.TotalTags = currentActivitySet.PostTags.Count;
            daily.TotalAchievements = currentActivitySet.TotalAchievements;

            foreach (var firstTag in currentActivitySet.FirstTags)
            {
                if (!ZExtensions.ParseTagInfoFrom(firstTag, out var postTagInfo))
                {
                    continue;
                }

                var tag = await _commonServices.TagServices.GetTagInfo(postTagInfo.Type, postTagInfo.Id, postTagInfo.Text, locale).ConfigureAwait(false);
                daily.FirstTags.Add(tag);
            }

            foreach (var firstAchievement in currentActivitySet.FirstAchievements)
            {
                var tag = await _commonServices.TagServices.GetTagInfo(PostTagType.Achievement, firstAchievement, null, locale).ConfigureAwait(false);
                daily.FirstAchievements.Add(tag);
            }

            totals.TotalTags += daily.TotalTags;
            totals.TotalAchievements += daily.TotalAchievements;

            totals.FirstTags.AddRange(daily.FirstTags);
            totals.FirstAchievements.AddRange(daily.FirstAchievements);

            results.Add(daily.Year, daily);
        }

        var totalsActivitySet = await TryGetMainActivitySet(timeZoneId, inZoneDay, inZoneMonth, _totalYearValue).ConfigureAwait(false);
        var allTimeTopAchievement = totalsActivitySet.AchievementCounts.OrderByDescending(x => x.Value).Take(topValueCount);
        foreach (var kvp in allTimeTopAchievement)
        {
            var tag = await _commonServices.TagServices.GetTagInfo(PostTagType.Achievement, kvp.Key, null, locale).ConfigureAwait(false);
            totals.TopAchievements.Add(tag);
        }

        var allTimeTopTags = totalsActivitySet.PostTags.OrderByDescending(x => x.Value).Take(topValueCount);
        foreach (var kvp in allTimeTopTags)
        {
            if (!ZExtensions.ParseTagInfoFrom(kvp.Key, out var postTagInfo))
            {
                continue;
            }

            var tag = await _commonServices.TagServices.GetTagInfo(postTagInfo.Type, postTagInfo.Id, postTagInfo.Text, locale).ConfigureAwait(false);
            totals.TopTags.Add(tag);
        }

        results.Add(totals.Year, totals);

        return results;
    }

    [ComputeMethod]
    protected virtual async Task<ActivitySetMain> TryGetMainActivitySet(string timeZoneId, int inZoneDay, int inZoneMonth, int inZoneYear)
    {
        var results = await TryGetMainActivitySetFull(timeZoneId, inZoneDay, inZoneMonth).ConfigureAwait(false);
        if (!results.TryGetValue(inZoneYear, out var result))
        {
            result = new ActivitySetMain();
        }

        return result;
    }

    [ComputeMethod]
    protected virtual async Task<Dictionary<int, ActivitySetMain>> TryGetMainActivitySetFull(string timeZoneId, int inZoneDay, int inZoneMonth)
    {
        var results = new Dictionary<int, ActivitySetMain>();
        var totals = new ActivitySetMain { Year = _totalYearValue };
        results.Add(totals.Year, totals);

        var postRecordPredicate = PredicateBuilder.New<PostRecord>();
        var achievementRecordPredicate = PredicateBuilder.New<CharacterAchievementRecord>();
        var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId);

        Exceptions.ThrowIf(timeZone == null);

        for (var year = _startYear; year < _endYear; year++)
        {
            var set = new ActivitySetMain
            {
                Year = year,
                StartTime = timeZone.AtStartOfDay(new LocalDate(year, inZoneMonth, inZoneDay)).ToInstant(),
                EndTime = timeZone.AtStartOfDay(new LocalDate(year, inZoneMonth, inZoneDay).PlusDays(1)).ToInstant(),
            };

            results[year] = set;

            postRecordPredicate = postRecordPredicate.Or(x => x.PostTime >= set.StartTime && x.PostTime < set.EndTime);
            achievementRecordPredicate = achievementRecordPredicate.Or(x => x.AchievementTimeStamp >= set.StartTime && x.AchievementTimeStamp < set.EndTime);
        }

        await using var database = CreateDbContext();

        var dailyAchievementsQuery = database.CharacterAchievements.AsExpandableEFCore().Where(achievementRecordPredicate).Select(x => new { x.Id, x.AchievementId, x.AchievementTimeStamp });
        var dailyAchievementsById = await dailyAchievementsQuery.ToArrayAsync().ConfigureAwait(false);
        foreach (var kvp in dailyAchievementsById)
        {
            var localDateTime = kvp.AchievementTimeStamp.InZone(timeZone).LocalDateTime;
            var activitySet = results[localDateTime.Year];

            activitySet.AchievementCounts.TryGetValue(kvp.AchievementId, out var counter);
            activitySet.AchievementCounts[kvp.AchievementId] = counter + 1;
            activitySet.TotalAchievements++;

            totals.AchievementCounts.TryGetValue(kvp.AchievementId, out var totalsCounter);
            totals.AchievementCounts[kvp.AchievementId] = totalsCounter + 1;
            totals.TotalAchievements++;
        }

        var dailyPostsQuery = database.Posts.AsExpandableEFCore().Include(p => p.PostTags).Where(postRecordPredicate).Select(x => new { x.Id, x.PostTags, x.PostTime });
        var dailyPosts = await dailyPostsQuery.ToArrayAsync().ConfigureAwait(false);
        foreach (var postRecord in dailyPosts)
        {
            var localDateTime = postRecord.PostTime.InZone(timeZone).LocalDateTime;
            foreach (var postTag in postRecord.PostTags)
            {
                if (postTag.TagKind != PostTagKind.Post)
                {
                    continue;
                }

                var activitySet = results[localDateTime.Year];
                activitySet.PostTags.TryGetValue(postTag.TagString, out var tagCount);
                activitySet.PostTags[postTag.TagString] = tagCount + 1;

                totals.PostTags.TryGetValue(postTag.TagString, out var totalsTagCount);
                totals.PostTags[postTag.TagString] = totalsTagCount + 1;
            }
        }

        var firstAchievements = await GetAllFirstAchievements().ConfigureAwait(false);
        foreach (var firstAchievement in firstAchievements)
        {
            var itemZonedDateTime = firstAchievement.Item2.InZone(timeZone);
            if (itemZonedDateTime.Day == inZoneDay && itemZonedDateTime.Month == inZoneMonth && results.TryGetValue(itemZonedDateTime.Year, out var activitySet))
            {
                activitySet.FirstAchievements.Add(firstAchievement.Item1);
                totals.FirstAchievements.Add(firstAchievement.Item1);
            }
        }

        var firstTags = await GetAllFirstTags().ConfigureAwait(false);
        foreach (var firstTag in firstTags)
        {
            var itemZonedDateTime = firstTag.Item2.InZone(timeZone);
            if (itemZonedDateTime.Day == inZoneDay && itemZonedDateTime.Month == inZoneMonth && results.TryGetValue(itemZonedDateTime.Year, out var activitySet))
            {
                activitySet.FirstTags.Add(firstTag.Item1);
                totals.FirstTags.Add(firstTag.Item1);
            }
        }

        return results;
    }

    [ComputeMethod(AutoInvalidateTime = 60 * 10)]
    protected virtual async Task<Tuple<int, Instant>[]> GetAllFirstAchievements()
    {
        await using var database = CreateDbContext();

        var firstAchievementsQuery = database.CharacterAchievements.TagWith("GetAllFirstAchievements").GroupBy(achievements => achievements.AchievementId).Select(g => new Tuple<int, Instant>(g.Key, g.Min(e => e.AchievementTimeStamp)));

        return await firstAchievementsQuery.ToArrayAsync().ConfigureAwait(false);
    }

    [ComputeMethod(AutoInvalidateTime = 60 * 10)]
    protected virtual async Task<Tuple<string, Instant>[]> GetAllFirstTags()
    {
        await using var database = CreateDbContext();

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
                                    select new Tuple<string, Instant>(g.Key, g.Min(e => e.PostTime));

        return await firstTagsGroupedQuery.TagWith("GetAllFirstTags").ToArrayAsync().ConfigureAwait(false);
    }

    [ComputeMethod]
    protected virtual async Task<Dictionary<int, DailyActivityResultsUser>> TryGetUserActivityFull(int accountId, string timeZoneId, byte inZoneDay, byte inZoneMonth, ServerSideLocale locale)
    {
        var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId);
        if (timeZone == null)
        {
            return new Dictionary<int, DailyActivityResultsUser>();
        }

        var results = new Dictionary<int, DailyActivityResultsUser>();
        var totals = new DailyActivityResultsUser { Year = _totalYearValue };
        for (var year = _startYear; year < _endYear; year++)
        {
            var currentActivitySet = await TryGetUserActivitySet(accountId, timeZoneId, inZoneDay, inZoneMonth, year).ConfigureAwait(false);
            if (currentActivitySet.Achievements.Count == 0 && currentActivitySet.FirstAchievements.Count == 0 && currentActivitySet.MyMemories.Count == 0)
            {
                continue;
            }

            var daily = new DailyActivityResultsUser
            {
                Year = year,
                ZoneId = timeZoneId,
                StartTimeMs = currentActivitySet.StartTime.ToUnixTimeMilliseconds(),
                EndTimeMs = currentActivitySet.EndTime.ToUnixTimeMilliseconds(),
            };

            foreach (var achievement in currentActivitySet.Achievements)
            {
                var tag = await _commonServices.TagServices.GetTagInfo(PostTagType.Achievement, achievement, null, locale).ConfigureAwait(false);
                daily.Achievements.Add(tag);
            }

            foreach (var achievement in currentActivitySet.FirstAchievements)
            {
                var tag = await _commonServices.TagServices.GetTagInfo(PostTagType.Achievement, achievement, null, locale).ConfigureAwait(false);
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

    [ComputeMethod]
    protected virtual async Task<ActivitySetUser> TryGetUserActivitySet(int accountId, string timeZoneId, int inZoneDay, int inZoneMonth, int inZoneYear)
    {
        var results = await TryGetUserActivitySetFull(accountId, timeZoneId, inZoneDay, inZoneMonth).ConfigureAwait(false);
        if (!results.TryGetValue(inZoneYear, out var result))
        {
            result = new ActivitySetUser();
        }

        return result;
    }

    [ComputeMethod(AutoInvalidateTime = 60 * 10)]
    protected virtual async Task<Dictionary<int, ActivitySetUser>> TryGetUserActivitySetFull(int accountId, string timeZoneId, int inZoneDay, int inZoneMonth)
    {
        await _commonServices.PostServices.DependsOnPostsBy(accountId).ConfigureAwait(false);
        await _commonServices.AccountServices.DependsOnAccountAchievements(accountId).ConfigureAwait(false);

        var results = new Dictionary<int, ActivitySetUser>();
        var totals = new ActivitySetUser { Year = _totalYearValue };
        results.Add(totals.Year, totals);

        var postRecordPredicate = PredicateBuilder.New<PostRecord>();
        var achievementRecordPredicate = PredicateBuilder.New<CharacterAchievementRecord>();
        var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId);
        Exceptions.ThrowIf(timeZone == null);
        for (var year = _startYear; year < _endYear; year++)
        {
            var set = new ActivitySetUser
            {
                Year = year,
                StartTime = timeZone.AtStartOfDay(new LocalDate(year, inZoneMonth, inZoneDay)).ToInstant(),
                EndTime = timeZone.AtStartOfDay(new LocalDate(year, inZoneMonth, inZoneDay).PlusDays(1)).ToInstant(),
            };

            results[year] = set;

            postRecordPredicate = postRecordPredicate.Or(x => x.PostTime >= set.StartTime && x.PostTime < set.EndTime);
            achievementRecordPredicate = achievementRecordPredicate.Or(x => x.AchievementTimeStamp >= set.StartTime && x.AchievementTimeStamp < set.EndTime);
        }

        await using var database = CreateDbContext();

        var achievementRecords = database.CharacterAchievements.Where(x => x.AccountId == accountId).Where(achievementRecordPredicate).Select(x => new { x.AchievementId, x.AchievementTimeStamp });
        var dailyAchievementsId = await achievementRecords.ToArrayAsync().ConfigureAwait(false);

        foreach (var dailyAchievement in dailyAchievementsId)
        {
            var itemZonedDateTime = dailyAchievement.AchievementTimeStamp.InZone(timeZone);
            var activitySet = results[itemZonedDateTime.Year];

            activitySet.Achievements.Add(dailyAchievement.AchievementId);
            totals.Achievements.Add(dailyAchievement.AchievementId);
        }

        var firstAchievementsQuery = from achievement in database.CharacterAchievements
                                     where achievement.AccountId == accountId
                                     group achievement by achievement.AchievementId into g
                                     select new
                                     {
                                         AchievementId = g.Key,
                                         AchievementTimeStamp = g.Min(e => e.AchievementTimeStamp),
                                     };

        var firstAchievements = await firstAchievementsQuery.ToArrayAsync().ConfigureAwait(false);
        foreach (var firstAchievement in firstAchievements)
        {
            var itemZonedDateTime = firstAchievement.AchievementTimeStamp.InZone(timeZone);
            if (itemZonedDateTime.Day == inZoneDay && itemZonedDateTime.Month == inZoneMonth)
            {
                var activitySet = results[itemZonedDateTime.Year];
                activitySet.FirstAchievements.Add(firstAchievement.AchievementId);
                totals.FirstAchievements.Add(firstAchievement.AchievementId);
            }
        }

        var accountString = PostTagInfo.GetTagString(PostTagType.Account, accountId);
        var memoriesQuery = from tag in database.PostTags
                            join post in database.Posts
                                on tag.PostId equals post.Id
                            where tag.TagString == accountString && post.DeletedTimeStamp == 0 && (tag.TagKind == PostTagKind.Post || tag.TagKind == PostTagKind.PostRestored)
                            select new { post.Id, post.AccountId, post.PostTime, post.PostCreatedTime, post.PostCommentMark, post.BlobNames };

        var memories = await memoriesQuery.ToArrayAsync().ConfigureAwait(false);
        var memoriesById = new HashSet<int>();
        foreach (var memory in memories)
        {
            var itemZonedDateTime = memory.PostTime.InZone(timeZone);
            if (itemZonedDateTime.Day != inZoneDay || itemZonedDateTime.Month != inZoneMonth)
            {
                continue;
            }

            if (!memoriesById.Add(memory.Id))
            {
                continue;
            }

            var userTagInfo = await _commonServices.TagServices.TryGetUserTagInfo(PostTagType.Account, memory.AccountId).ConfigureAwait(false);
            var blobNames = Array.Empty<string>();
            if (string.IsNullOrEmpty(memory.BlobNames))
            {
            }
            else
            {
                blobNames = memory.BlobNames.Split('|');
            }

            var activitySet = results[itemZonedDateTime.Year];
            activitySet.MyMemories.Add(new DailyActivityResultsUserPostInfo
            {
                PostId = memory.Id,
                AccountId = memory.AccountId,
                PostTime = memory.PostTime.ToUnixTimeMilliseconds(),
                PostCreatedTime = memory.PostCreatedTime.ToUnixTimeMilliseconds(),
                BlobInfo = PostViewModelBlobInfo.CreateBlobInfo(userTagInfo.Name, memory.PostCommentMark, blobNames),
            });
        }

        return results;
    }

    [ComputeMethod]
    public virtual async Task<MainSearchResult[]> TrySearch(Session session, MainSearchType searchType, string searchString)
    {
        return await TrySearch(searchType, searchString).ConfigureAwait(false);
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
            var results = await TrySearchAccounts(searchString).ConfigureAwait(false);
            allResults.AddRange(results);
        }

        if ((searchType & MainSearchType.Character) == MainSearchType.Character)
        {
            var results = await TrySearchCharacters(searchString).ConfigureAwait(false);
            allResults.AddRange(results);
        }

        if ((searchType & MainSearchType.Guild) == MainSearchType.Guild)
        {
            var results = await TrySearchGuilds(searchString).ConfigureAwait(false);
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

        var results = await query.Take(50).ToArrayAsync().ConfigureAwait(false);
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

        var results = await query.Take(50).ToArrayAsync().ConfigureAwait(false);
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

        var results = await query.Take(50).ToArrayAsync().ConfigureAwait(false);
        return results;
    }

    [ComputeMethod]
    public virtual async Task<RecentPostsResults> TryGetRecentPosts(Session session, RecentPostsType postsType, PostSortMode sortMode, int currentPage, ServerSideLocale locale)
    {
        var account = await _commonServices.AccountServices.TryGetActiveAccount(session).ConfigureAwait(false);
        var allSearchResult = Array.Empty<int>();
        if (account != null && postsType == RecentPostsType.Default)
        {
            allSearchResult = await TryGetRecentPosts(account.Id).ConfigureAwait(false);
        }

        if (allSearchResult.Length == 0)
        {
            allSearchResult = await TryGetRecentPosts().ConfigureAwait(false);
        }

        var allPostViewModels = Array.Empty<PostViewModel>();
        var totalPages = (int)Math.Ceiling(allSearchResult.Length / (float)CommonConfig.PostsPerPage);
        if (allSearchResult.Length > 0)
        {
            currentPage = Math.Clamp(currentPage, 1, totalPages);
            allPostViewModels = await GetPostViewModelsForPage(session, allSearchResult, currentPage, CommonConfig.PostsPerPage, locale).ConfigureAwait(false);
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
    protected virtual async Task<int[]> TryGetRecentPosts()
    {
        await using var database = CreateDbContext();

        await _commonServices.PostServices.DependsOnNewPosts().ConfigureAwait(false);

        var query = from p in database.Posts
                    where p.DeletedTimeStamp == 0 && p.PostVisibility == 0
                    orderby p.PostCreatedTime descending
                    select p.Id;

        return await query.ToArrayAsync().ConfigureAwait(false);
    }

    [ComputeMethod]
    protected virtual async Task<int[]> TryGetRecentPosts(int accountId)
    {
        var following = await _commonServices.FollowingServices.TryGetAccountFollowing(accountId).ConfigureAwait(false);
        var allFollowingIds = new HashSet<int> { accountId };
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
            await _commonServices.PostServices.DependsOnPostsBy(followingViewModel).ConfigureAwait(false);
        }

        await using var database = CreateDbContext();
        var query = from p in database.Posts
                    where p.DeletedTimeStamp == 0 && allFollowingIds.Contains(p.AccountId)
                    orderby p.PostCreatedTime descending
                    select p.Id;

        return await query.ToArrayAsync().ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<SearchPostsResults> TrySearchPosts(Session session, string[] tagStrings, PostSortMode sortMode, int currentPage, long postMinTime, long postMaxTime, ServerSideLocale locale)
    {
        postMinTime = Math.Clamp(postMinTime, 0, SystemClock.Instance.GetCurrentInstant().ToUnixTimeMilliseconds());
        postMaxTime = Math.Clamp(postMaxTime, 0, SystemClock.Instance.GetCurrentInstant().ToUnixTimeMilliseconds());

        var searchPostTags = Array.Empty<PostTagInfo>();
        var serverSideTagStrings = new HashSet<string>();

        if (tagStrings != null)
        {
            var result = await GetPostSearchTags(tagStrings, locale).ConfigureAwait(false);
            searchPostTags = result.Tags;
            serverSideTagStrings = result.Strings;
        }

        var allSearchResult = await TrySearchPosts(serverSideTagStrings, sortMode, postMinTime, postMaxTime).ConfigureAwait(false);
        var allPostViewModels = Array.Empty<PostViewModel>();
        var totalPages = (int)Math.Ceiling(allSearchResult.Length / (float)CommonConfig.PostsPerPage);
        if (allSearchResult.Length > 0)
        {
            currentPage = Math.Clamp(currentPage, 1, totalPages);
            allPostViewModels = await GetPostViewModelsForPage(session, allSearchResult, currentPage, CommonConfig.PostsPerPage, locale).ConfigureAwait(false);
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

    private async Task<PostViewModel[]> GetPostViewModelsForPage(Session session, int[] allSearchResult, int currentPage, int postsPerPage, ServerSideLocale locale)
    {
        var viewModels = new List<PostViewModel>();
        for (var i = (currentPage - 1) * postsPerPage; i < allSearchResult.Length; i++)
        {
            var postViewModel = await _commonServices.PostServices.TryGetPostViewModel(session, allSearchResult[i], locale).ConfigureAwait(false);
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
    protected virtual async Task<(PostTagInfo[] Tags, HashSet<string> Strings)> GetPostSearchTags(string[] tagStrings, ServerSideLocale locale)
    {
        var searchPostTags = new List<PostTagInfo>();
        var serverSideTagStrings = new HashSet<string>();

        foreach (var tagString in tagStrings)
        {
            if (!ZExtensions.ParseTagInfoFrom(tagString, out var postTagInfo))
            {
                continue;
            }

            var tagInfo = await _commonServices.TagServices.GetTagInfo(postTagInfo.Type, postTagInfo.Id, postTagInfo.Text, locale).ConfigureAwait(false);
            if (tagInfo != null && serverSideTagStrings.Add(tagInfo.TagString))
            {
                searchPostTags.Add(tagInfo);
            }
        }

        return (searchPostTags.ToArray(), serverSideTagStrings);
    }

    [ComputeMethod]
    protected virtual async Task<int[]> TrySearchPosts(HashSet<string> tagStrings, PostSortMode sortMode, long minTime, long maxTime)
    {
        await using var database = CreateDbContext();

        foreach (var tagString in tagStrings)
        {
            await _commonServices.PostServices.DependsOnPostsWithTagString(tagString).ConfigureAwait(false);
        }

        var query = from p in GetPostSearchQuery(database, tagStrings, sortMode, minTime, maxTime)
                    select p.Id;

        return await query.ToArrayAsync().ConfigureAwait(false);
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