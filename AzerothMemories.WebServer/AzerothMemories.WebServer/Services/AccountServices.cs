namespace AzerothMemories.WebServer.Services;

public class AccountServices : IAccountServices
{
    private readonly ILogger<AccountServices> _logger;
    private readonly CommonServices _commonServices;

    public AccountServices(ILogger<AccountServices> logger, CommonServices commonServices)
    {
        _logger = logger;
        _commonServices = commonServices;
    }

    [ComputeMethod]
    public virtual Task<int> DependsOnAccountRecord(int accountId)
    {
        return Task.FromResult(accountId);
    }

    [ComputeMethod]
    public virtual Task<int> DependsOnAccountUsername(int accountId)
    {
        return Task.FromResult(accountId);
    }

    [ComputeMethod]
    public virtual Task<int> DependsOnAccountAvatar(int accountId)
    {
        return Task.FromResult(accountId);
    }

    [ComputeMethod]
    public virtual Task<int> DependsOnAccountAchievements(int accountId)
    {
        return Task.FromResult(accountId);
    }

    [ComputeMethod]
    public virtual async Task<AccountRecord?> TryGetAccountRecord(int id)
    {
        using var _ = new MethodTimeLogger(_logger, new { id }.ToString());
        await DependsOnAccountRecord(id).ConfigureAwait(false);

        await using var database = await _commonServices.DatabaseHub.CreateDbContext().ConfigureAwait(false);
        var accountRecord = await database.Accounts.FirstOrDefaultAsync(a => a.Id == id).ConfigureAwait(false);

        return accountRecord;
    }

    [ComputeMethod]
    public virtual async Task<AccountRecord?> TryGetAccountRecordFusionId(string? fusionId)
    {
        if (string.IsNullOrWhiteSpace(fusionId))
        {
            return null;
        }

        using var _ = new MethodTimeLogger(_logger, new { fusionId }.ToString());
        await using var database = await _commonServices.DatabaseHub.CreateDbContext().ConfigureAwait(false);
        var accountRecord = await database.Accounts.FirstOrDefaultAsync(a => a.FusionId == fusionId).ConfigureAwait(false);
        if (accountRecord != null)
        {
            await DependsOnAccountRecord(accountRecord.Id).ConfigureAwait(false);
        }

        return accountRecord;
    }

    [ComputeMethod]
    public virtual async Task<AccountRecord?> TryGetAccountRecordUsername(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        using var _ = new MethodTimeLogger(_logger, new { username }.ToString());
        await using var database = await _commonServices.DatabaseHub.CreateDbContext().ConfigureAwait(false);
        var accountRecord = await database.Accounts.FirstOrDefaultAsync(a => a.Username == username).ConfigureAwait(false);

        if (accountRecord == null)
        {
            var usernameSearchable = DatabaseHelpers.GetSearchableName(username);
            accountRecord = await database.Accounts.FirstOrDefaultAsync(a => a.UsernameSearchable == usernameSearchable).ConfigureAwait(false);
        }

        if (accountRecord != null)
        {
            await DependsOnAccountRecord(accountRecord.Id).ConfigureAwait(false);
        }

        return accountRecord;
    }

    [ComputeMethod]
    public virtual async Task<AccountViewModel?> TryGetActiveAccount(Session session)
    {
        using var _ = new MethodTimeLogger(_logger, new { session }.ToString());
        var accountRecord = await TryGetActiveAccountRecord(session).ConfigureAwait(false);
        if (accountRecord == null)
        {
            return null;
        }

        await DependsOnAccountRecord(accountRecord.Id).ConfigureAwait(false);
        return await CreateAccountViewModel(accountRecord, true).ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<AccountViewModel?> TryGetAccountById(Session session, int accountId)
    {
        using var _ = new MethodTimeLogger(_logger, new { session, accountId }.ToString());
        await DependsOnAccountRecord(accountId).ConfigureAwait(false);

        var sessionAccount = await TryGetActiveAccount(session).ConfigureAwait(false);
        if (sessionAccount != null && sessionAccount.Id == accountId)
        {
            return sessionAccount;
        }

        var accountRecord = await TryGetAccountRecord(accountId).ConfigureAwait(false);
        if (accountRecord == null)
        {
            return null;
        }

        var isAdmin = sessionAccount != null && sessionAccount.IsAdmin();
        var isActive = sessionAccount != null && sessionAccount.Id == accountRecord.Id;

        return await CreateAccountViewModel(accountRecord, isActive || isAdmin).ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<AccountViewModel?> TryGetAccountByUsername(Session session, string username)
    {
        using var _ = new MethodTimeLogger(_logger, new { session, username }.ToString());
        var sessionAccount = await TryGetActiveAccount(session).ConfigureAwait(false);
        if (sessionAccount != null && sessionAccount.Username == username)
        {
            return sessionAccount;
        }

        var accountRecord = await TryGetAccountRecordUsername(username).ConfigureAwait(false);
        if (accountRecord == null)
        {
            return null;
        }

        var isAdmin = sessionAccount != null && sessionAccount.IsAdmin();
        var isActive = sessionAccount != null && sessionAccount.Id == accountRecord.Id;

        await DependsOnAccountRecord(accountRecord.Id).ConfigureAwait(false);

        return await CreateAccountViewModel(accountRecord, isActive || isAdmin).ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<AccountViewModel> CreateAccountViewModel(AccountRecord accountRecord, bool activeOrAdmin)
    {
        using var _ = new MethodTimeLogger(_logger, new { accountRecord.Id, activeOrAdmin }.ToString());
        await DependsOnAccountRecord(accountRecord.Id).ConfigureAwait(false);

        await _commonServices.BlizzardUpdateHandler.TryUpdate(accountRecord).ConfigureAwait(false);

        var characters = await _commonServices.CharacterServices.TryGetAllAccountCharacters(accountRecord.Id).ConfigureAwait(false);
        var followingViewModels = await _commonServices.FollowingServices.TryGetAccountFollowing(accountRecord.Id).ConfigureAwait(false);
        var followersViewModels = await _commonServices.FollowingServices.TryGetAccountFollowers(accountRecord.Id).ConfigureAwait(false);
        var postCount = GetPostCount(accountRecord.Id).ConfigureAwait(false);
        var memoryCount = GetMemoryCount(accountRecord.Id).ConfigureAwait(false);
        var commentCount = GetCommentCount(accountRecord.Id).ConfigureAwait(false);
        var reactionCount = GetReactionCount(accountRecord.Id).ConfigureAwait(false);

        var viewModel = accountRecord.CreateViewModel(_commonServices, activeOrAdmin, followingViewModels, followersViewModels);

        viewModel.TotalPostCount = await postCount;
        viewModel.TotalCommentCount = await commentCount;
        viewModel.TotalMemoriesCount = await memoryCount;
        viewModel.TotalReactionsCount = await reactionCount;

        var allCharacters = activeOrAdmin ? characters.Values.ToArray() : characters.Values.Where(x => x.AccountSync && x.CharacterStatus == CharacterStatus2.None).ToArray();

        viewModel.CharactersArray = allCharacters.Where(x => x.RealmVersion == BlizzardRealmVersion.Main).ToArray();
        viewModel.CharactersArrayClassicEra = allCharacters.Where(x => x.RealmVersion == BlizzardRealmVersion.Classic).ToArray();
        viewModel.CharactersArrayClassicProgression = allCharacters.Where(x => x.RealmVersion == BlizzardRealmVersion.ClassicProgression).ToArray();

        if (viewModel.IsCustomAvatar())
        {
            viewModel.Avatar = await _commonServices.MediaServices.TryGetBlobWithToken(viewModel.Avatar).ConfigureAwait(false);
        }

        return viewModel;
    }

    [ComputeMethod]
    public virtual async Task<int> GetPostCount(int accountId)
    {
        using var _ = new MethodTimeLogger(_logger, new { accountId }.ToString());
        await using var database = await _commonServices.DatabaseHub.CreateDbContext().ConfigureAwait(false);
        return await database.Posts.Where(x => x.AccountId == accountId).CountAsync().ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<int> GetMemoryCount(int accountId)
    {
        using var _ = new MethodTimeLogger(_logger, new { accountId }.ToString());
        await using var database = await _commonServices.DatabaseHub.CreateDbContext().ConfigureAwait(false);
        return await database.PostTags.Where(x => x.TagType == PostTagType.Account && x.TagId == accountId && x.TagKind == PostTagKind.PostRestored).CountAsync().ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<int> GetCommentCount(int accountId)
    {
        using var _ = new MethodTimeLogger(_logger, new { accountId }.ToString());
        await using var database = await _commonServices.DatabaseHub.CreateDbContext().ConfigureAwait(false);
        return await database.PostComments.Where(x => x.AccountId == accountId).CountAsync().ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<int> GetReactionCount(int accountId)
    {
        using var _ = new MethodTimeLogger(_logger, new { accountId }.ToString());
        await using var database = await _commonServices.DatabaseHub.CreateDbContext().ConfigureAwait(false);
        var postCount = await database.PostReactions.Where(x => x.AccountId == accountId && x.Reaction > PostReaction.None).CountAsync().ConfigureAwait(false);
        var commentCount = await database.PostCommentReactions.Where(x => x.AccountId == accountId && x.Reaction > PostReaction.None).CountAsync().ConfigureAwait(false);

        return postCount + commentCount;
    }

    [ComputeMethod]
    public virtual async Task<bool> CheckIsValidUsername(Session session, string username)
    {
        return await CheckIsValidUsername(username).ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<bool> CheckIsValidUsername(string username)
    {
        using var _ = new MethodTimeLogger(_logger, new { username }.ToString());
        if (!DatabaseHelpers.IsValidAccountName(username))
        {
            return false;
        }

        await using var database = await _commonServices.DatabaseHub.CreateDbContext().ConfigureAwait(false);
        var usernameExists = await database.Accounts.AnyAsync(x => x.Username == username).ConfigureAwait(false);
        if (usernameExists)
        {
            return false;
        }

        return true;
    }

    [CommandHandler]
    public virtual async Task<bool> TryChangeUsername(Account_TryChangeUsername command, CancellationToken cancellationToken = default)
    {
        using var _ = new MethodTimeLogger(_logger);
        return await AccountServices_TryChangeUsername.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<bool> TryChangeIsPrivate(Account_TryChangeIsPrivate command, CancellationToken cancellationToken = default)
    {
        using var _ = new MethodTimeLogger(_logger);
        return await AccountServices_TryChangeIsPrivate.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<bool> TryChangeBattleTagVisibility(Account_TryChangeBattleTagVisibility command, CancellationToken cancellationToken = default)
    {
        using var _ = new MethodTimeLogger(_logger);
        return await AccountServices_TryChangeBattleTagVisibility.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<string?> TryChangeAvatar(Account_TryChangeAvatar command, CancellationToken cancellationToken = default)
    {
        using var _ = new MethodTimeLogger(_logger);
        return await AccountServices_TryChangeAvatar.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<string?> TryChangeAvatarUpload(Account_TryChangeAvatarUpload command, CancellationToken cancellationToken = default)
    {
        using var _ = new MethodTimeLogger(_logger);
        return await AccountServices_TryChangeAvatarUpload.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<string?> TryChangeSocialLink(Account_TryChangeSocialLink command, CancellationToken cancellationToken = default)
    {
        using var _ = new MethodTimeLogger(_logger);
        return await AccountServices_TryChangeSocialLink.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<bool> TryDisconnectAccount(Account_TryDisconnectAccount command, CancellationToken cancellationToken = default)
    {
        using var _ = new MethodTimeLogger(_logger);
        return await AccountServices_TryDisconnectAccount.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<PostViewModel[]> TrySearchPostsByTime(Session session, long timeStamp, int diffInSeconds, ServerSideLocale locale)
    {
        using var _ = new MethodTimeLogger(_logger, new { session, timeStamp, diffInSeconds }.ToString());
        var accountRecord = await TryGetActiveAccountRecord(session).ConfigureAwait(false);
        if (accountRecord == null)
        {
            return [];
        }

        var (min, max) = ZExtensions.ClampTimeMinMaxAsInstant(timeStamp, diffInSeconds);
        var allPosts = await TrySearchGetAllPosts(accountRecord.Id, locale).ConfigureAwait(false);

        return allPosts.Where(x =>
        {
            var postTime = Instant.FromUnixTimeMilliseconds(x.PostTime);
            return postTime > min && postTime < max;
        }).ToArray();
    }

    [ComputeMethod]
    protected virtual async Task<PostViewModel[]> TrySearchGetAllPosts(int accountId, ServerSideLocale locale)
    {
        await _commonServices.PostServices.DependsOnPostsBy(accountId).ConfigureAwait(false);

        await using var database = await _commonServices.DatabaseHub.CreateDbContext().ConfigureAwait(false);

        var query = from r in database.Posts
                    where r.AccountId == accountId && r.DeletedTimeStamp == 0
                    select r.Id;

        var results = await query.ToArrayAsync().ConfigureAwait(false);
        var allPostViewModel = new List<PostViewModel>();
        foreach (var postId in results)
        {
            var postViewModel = await _commonServices.PostServices.TryGetPostViewModel(accountId, postId, locale).ConfigureAwait(false);
            if (postViewModel != null)
            {
                allPostViewModel.Add(postViewModel);
            }
        }

        return allPostViewModel.ToArray();
    }

    [ComputeMethod]
    public virtual async Task<PostTagInfo[]> TryGetAchievementsByTime(Session session, long timeStamp, int diffInSeconds, ServerSideLocale locale)
    {
        using var _ = new MethodTimeLogger(_logger, new { session, timeStamp, diffInSeconds, locale }.ToString());
        var accountRecord = await TryGetActiveAccountRecord(session).ConfigureAwait(false);
        if (accountRecord == null)
        {
            return [];
        }

        await DependsOnAccountAchievements(accountRecord.Id).ConfigureAwait(false);

        await using var database = await _commonServices.DatabaseHub.CreateDbContext().ConfigureAwait(false);

        var (min, max) = ZExtensions.ClampTimeMinMaxAsInstant(timeStamp, diffInSeconds);
        var query = from a in database.CharacterAchievements
                    where a.AccountId == accountRecord.Id && a.AchievementTimeStamp > min && a.AchievementTimeStamp < max
                    select a.AchievementId;

        var results = await query.ToArrayAsync().ConfigureAwait(false);

        return await GetAchievementsTags(results, locale).ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<(int Id, long TimeStamp)[]> TrySearchGetAllAchievements(int accountId)
    {
        await DependsOnAccountAchievements(accountId).ConfigureAwait(false);

        await using var database = await _commonServices.DatabaseHub.CreateDbContext().ConfigureAwait(false);

        var query = from a in database.CharacterAchievements
                    where a.AccountId == accountId
                    select a;

        var allAchievements = await query.ToArrayAsync().ConfigureAwait(false);
        var results = new HashSet<(int Id, long AchievementsTimeStamp)>();

        foreach (var achievement in allAchievements)
        {
            results.Add((achievement.AchievementId, achievement.AchievementTimeStamp.ToUnixTimeMilliseconds()));
        }

        return results.ToArray();
    }

    [ComputeMethod]
    public virtual async Task<AccountHistoryPageResult?> TryGetAccountHistory(Session session, int currentPage)
    {
        using var _ = new MethodTimeLogger(_logger, new { session, currentPage }.ToString());
        var activeAccount = await TryGetActiveAccount(session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return null;
        }

        if (currentPage == 0)
        {
            currentPage = 1;
        }

        return await TryGetAccountHistory(activeAccount.Id, currentPage).ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<CheckMemoryResult> TryCheckMemory(Session session, ServerSideLocale locale)
    {
        using var _ = new MethodTimeLogger(_logger, new { session }.ToString());

        var accountRecord = await TryGetActiveAccountRecord(session).ConfigureAwait(false);
        var allPosts = Array.Empty<PostViewModel>();
        var allAchievementRecords = Array.Empty<(int Id, long TimeStamp)>();
        if (accountRecord != null)
        {
            allPosts = await TrySearchGetAllPosts(accountRecord.Id, locale).ConfigureAwait(false);
            allAchievementRecords = await TrySearchGetAllAchievements(accountRecord.Id).ConfigureAwait(false);
        }

        var allAchievementInfo = new CheckMemoryAchievementInfo[allAchievementRecords.Length];
        for (var i = 0; i < allAchievementRecords.Length; i++)
        {
            var achievementRecord = allAchievementRecords[i];
            var postTag = await _commonServices.TagServices.GetTagInfo(PostTagType.Achievement, achievementRecord.Id, null, locale).ConfigureAwait(false);

            allAchievementInfo[i] = new CheckMemoryAchievementInfo
            {
                Achievement = postTag,
                TimeStamp = achievementRecord.TimeStamp
            };
        }

        var result = new CheckMemoryResult
        {
            Posts = allPosts,
            Achievements = allAchievementInfo
        };

        return result;
    }

    private async Task<PostTagInfo[]> GetAchievementsTags(int[] achievementsResults, ServerSideLocale locale)
    {
        var hashSet = new HashSet<int>();
        var postTagSet = new HashSet<PostTagInfo>();

        foreach (var tagId in achievementsResults)
        {
            if (hashSet.Add(tagId))
            {
                var postTag = await _commonServices.TagServices.GetTagInfo(PostTagType.Achievement, tagId, null, locale).ConfigureAwait(false);
                postTagSet.Add(postTag);
            }
        }

        return postTagSet.ToArray();
    }

    [ComputeMethod]
    public virtual async Task<AccountHistoryPageResult> TryGetAccountHistory(int activeAccountId, int currentPage)
    {
        using var _ = new MethodTimeLogger(_logger, new { activeAccountId, currentPage }.ToString());
        Exceptions.ThrowIf(activeAccountId == 0);
        Exceptions.ThrowIf(currentPage == 0);

        await using var database = await _commonServices.DatabaseHub.CreateDbContext().ConfigureAwait(false);

        var historyQuery = from record in database.AccountHistory
                           where record.AccountId == activeAccountId
                           from thisAccount in database.Accounts.Where(r => record.AccountId == r.Id)
                           from otherAccount in database.Accounts.Where(r => record.OtherAccountId == r.Id).DefaultIfEmpty()
                           orderby record.CreatedTime descending
                           select new AccountHistoryViewModel
                           {
                               Id = record.Id,
                               Type = record.Type,
                               AccountId = record.AccountId,
                               OtherAccountId = record.OtherAccountId.GetValueOrDefault(),
                               OtherAccountUsername = otherAccount == null ? null : otherAccount.GetUsernameSafe(),
                               TargetId = record.TargetId,
                               TargetPostId = record.TargetPostId.GetValueOrDefault(),
                               TargetCommentId = record.TargetCommentId.GetValueOrDefault(),
                               CreatedTime = record.CreatedTime.ToUnixTimeMilliseconds()
                           };

        var totalHistoryItemsCounts = await historyQuery.CountAsync().ConfigureAwait(false);

        var totalPages = (int)Math.Ceiling(totalHistoryItemsCounts / (float)ZExtensions.HistoryItemsPerPage);
        AccountHistoryViewModel[] recentHistoryViewModels;
        if (totalPages == 0)
        {
            recentHistoryViewModels = [];
        }
        else
        {
            currentPage = Math.Clamp(currentPage, 1, totalPages);
            recentHistoryViewModels = await historyQuery.Skip((currentPage - 1) * ZExtensions.HistoryItemsPerPage).Take(ZExtensions.HistoryItemsPerPage).ToArrayAsync().ConfigureAwait(false);
        }

        return new AccountHistoryPageResult
        {
            CurrentPage = currentPage,
            TotalPages = totalPages,
            ViewModels = recentHistoryViewModels
        };
    }

    [ComputeMethod]
    public virtual async Task<AccountRecord?> TryGetActiveAccountRecord(Session? session)
    {
        using var _ = new MethodTimeLogger(_logger, new { session }.ToString());
        if (session == null)
        {
            return null;
        }

        if (session.IsDefault())
        {
            return null;
        }

        var user = await _commonServices.Auth.GetUser(session).ConfigureAwait(false);
        if (user == null || user.IsGuest())
        {
            return null;
        }

        var accountRecord = await TryGetAccountRecordFusionId(user.Id).ConfigureAwait(false);
        if (accountRecord == null)
        {
            return null;
        }

        await DependsOnAccountRecord(accountRecord.Id).ConfigureAwait(false);

        return accountRecord;
    }
}