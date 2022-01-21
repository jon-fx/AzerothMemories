using System.Web;

namespace AzerothMemories.WebServer.Services;

[RegisterComputeService]
[RegisterAlias(typeof(IAccountServices))]
public class AccountServices : IAccountServices
{
    private readonly CommonServices _commonServices;

    public AccountServices(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    public void OnAccountUpdate(AccountRecord accountRecord)
    {
        InvalidateAccountRecord(accountRecord);
    }

    public void InvalidateAccountRecord(AccountRecord accountRecord)
    {
        using var computed = Computed.Invalidate();

        _ = TryGetAccountRecord(accountRecord.Id);
        _ = TryGetAccountRecordFusionId(accountRecord.FusionId);
        _ = TryGetAccountRecordUsername(accountRecord.Username);
        _ = CreateAccountViewModel(accountRecord, true);
        _ = CreateAccountViewModel(accountRecord, false);
        _ = _commonServices.TagServices.TryGetUserTagInfo(PostTagType.Account, accountRecord.Id);
    }

    [CommandHandler(IsFilter = true, Priority = 1)]
    protected virtual async Task OnSignIn(SignInCommand command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        await context.InvokeRemainingHandlers(cancellationToken);

        if (Computed.IsInvalidating())
        {
            return;
        }

        if (!command.User.Claims.TryGetValue("BattleNet-Id", out var blizzardIdClaim))
        {
            throw new NotImplementedException();
        }

        if (!long.TryParse(blizzardIdClaim, out var blizzardId))
        {
            throw new NotImplementedException();
        }

        if (!command.User.Claims.TryGetValue("BattleNet-Tag", out var battleTag))
        {
            throw new NotImplementedException();
        }

        if (!command.User.Claims.TryGetValue("BattleNet-Region", out var battleNetRegion))
        {
            throw new NotImplementedException();
        }

        if (!command.User.Claims.TryGetValue("BattleNet-Token", out var battleNetToken))
        {
            throw new NotImplementedException();
        }

        if (!command.User.Claims.TryGetValue("BattleNet-TokenExpires", out var battleNetTokenExpiresStr) || !long.TryParse(battleNetTokenExpiresStr, out var battleNetTokenExpires))
        {
            throw new NotImplementedException();
        }

        var sessionInfo = context.Operation().Items.Get<SessionInfo>();
        if (sessionInfo == null)
        {
            throw new NotImplementedException();
        }

        var userId = sessionInfo.UserId;
        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var accountRecord = await GetOrCreateAccount(database, userId);

        var updateQuery = database.GetUpdateQuery(accountRecord, out var changed);
        var blizzardRegion = BlizzardRegionExt.FromName(battleNetRegion);

        if (CheckAndChange.Check(ref accountRecord.BlizzardId, blizzardId, ref changed))
        {
            updateQuery = updateQuery.Set(x => x.BlizzardId, accountRecord.BlizzardId);
        }

        if (CheckAndChange.Check(ref accountRecord.BlizzardRegionId, blizzardRegion, ref changed))
        {
            updateQuery = updateQuery.Set(x => x.BlizzardRegionId, accountRecord.BlizzardRegionId);
        }

        if (CheckAndChange.Check(ref accountRecord.BattleNetToken, battleNetToken, ref changed))
        {
            updateQuery = updateQuery.Set(x => x.BattleNetToken, accountRecord.BattleNetToken);
        }

        if (CheckAndChange.Check(ref accountRecord.BattleNetTokenExpiresAt, Instant.FromUnixTimeMilliseconds(battleNetTokenExpires), ref changed))
        {
            updateQuery = updateQuery.Set(x => x.BattleNetTokenExpiresAt, accountRecord.BattleNetTokenExpiresAt);
        }

        string newUsername = null;
        var previousBattleTag = accountRecord.BattleTag ?? string.Empty;
        if (string.IsNullOrWhiteSpace(accountRecord.Username) || accountRecord.Username == previousBattleTag.Replace("#", string.Empty))
        {
            newUsername = battleTag.Replace("#", string.Empty);
        }

        if (CheckAndChange.Check(ref accountRecord.BattleTag, battleTag, ref changed))
        {
            updateQuery = updateQuery.Set(x => x.BattleTag, accountRecord.BattleTag);
        }

        if (!string.IsNullOrWhiteSpace(newUsername))
        {
            var result = await database.Accounts.CountAsync(x => x.Username == newUsername, cancellationToken);
            newUsername = result > 0 ? $"User{accountRecord.Id}" : newUsername;

            if (CheckAndChange.Check(ref accountRecord.Username, newUsername, ref changed))
            {
                updateQuery = updateQuery.Set(p => p.Username, accountRecord.Username);
            }

            if (CheckAndChange.Check(ref accountRecord.UsernameSearchable, DatabaseHelpers.GetSearchableName(accountRecord.Username), ref changed))
            {
                updateQuery = updateQuery.Set(p => p.UsernameSearchable, accountRecord.UsernameSearchable);
            }
        }

        if (changed)
        {
            await updateQuery.UpdateAsync(cancellationToken);
        }

        await _commonServices.BlizzardUpdateHandler.TryUpdate(database, accountRecord, BlizzardUpdatePriority.Account);

        InvalidateAccountRecord(accountRecord);
    }

    private async Task<AccountRecord> GetOrCreateAccount(DatabaseConnection database, string userId)
    {
        var accountRecord = await TryGetAccountRecordFusionId(userId);
        if (accountRecord == null)
        {
            accountRecord = new AccountRecord
            {
                FusionId = userId,
                //MoaRef = moaRef.Full,
                //BlizzardId = moaRef.Id,
                CreatedDateTime = SystemClock.Instance.GetCurrentInstant()
                //LastLoginDateTime = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
            };

            accountRecord.Id = await database.InsertWithInt64IdentityAsync(accountRecord);

            if (accountRecord.Id == 0)
            {
                throw new NotImplementedException();
            }

            await TestingHistory(database, new AccountHistoryRecord
            {
                AccountId = accountRecord.Id,
                CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                Type = AccountHistoryType.AccountCreated
            });

            InvalidateAccountRecord(accountRecord);
        }

        return accountRecord;
    }

    [ComputeMethod]
    public virtual async Task<AccountRecord> TryGetAccountRecord(long id)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var user = await database.Accounts.Where(a => a.Id == id).FirstOrDefaultAsync();
        return user;
    }

    [ComputeMethod]
    public virtual async Task<AccountRecord> TryGetAccountRecordFusionId(string fusionId)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var user = await database.Accounts.Where(a => a.FusionId == fusionId).FirstOrDefaultAsync();
        return user;
    }

    [ComputeMethod]
    public virtual async Task<AccountRecord> TryGetAccountRecordUsername(string username)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var user = await database.Accounts.Where(a => a.Username == username).FirstOrDefaultAsync();
        return user;
    }

    [ComputeMethod]
    public virtual async Task<AccountViewModel> TryGetActiveAccount(Session session)
    {
        var accountRecord = await TryGetCurrentSessionAccountRecord(session);
        if (accountRecord == null)
        {
            return null;
        }

        return await CreateAccountViewModel(accountRecord, true);
    }

    [ComputeMethod]
    public virtual async Task<AccountViewModel> TryGetAccountById(Session session, long accountId)
    {
        var sessionAccount = await TryGetActiveAccount(session);
        if (sessionAccount != null && sessionAccount.Id == accountId)
        {
            return sessionAccount;
        }

        var accountRecord = await TryGetAccountRecord(accountId);
        if (accountRecord == null)
        {
            return null;
        }

        return await CreateAccountViewModel(accountRecord, sessionAccount != null && sessionAccount.AccountType >= AccountType.Admin);
    }

    [ComputeMethod]
    public virtual async Task<AccountViewModel> TryGetAccountByUsername(Session session, string username)
    {
        var sessionAccount = await TryGetActiveAccount(session);
        if (sessionAccount != null && sessionAccount.Username == username)
        {
            return sessionAccount;
        }

        var accountRecord = await TryGetAccountRecordUsername(username);
        if (accountRecord == null)
        {
            return null;
        }

        return await CreateAccountViewModel(accountRecord, sessionAccount != null && sessionAccount.AccountType >= AccountType.Admin);
    }

    public async Task<bool> TryEnqueueUpdate(Session session)
    {
        var accountRecord = await TryGetCurrentSessionAccountRecord(session);
        if (accountRecord == null)
        {
            return false;
        }

        await _commonServices.BlizzardUpdateHandler.TryUpdate(accountRecord, BlizzardUpdatePriority.Account);

        return true;
    }

    [ComputeMethod]
    protected virtual async Task<AccountViewModel> CreateAccountViewModel(AccountRecord accountRecord, bool activeOrAdmin)
    {
        var characters = await _commonServices.CharacterServices.TryGetAllAccountCharacters(accountRecord.Id);
        var followingViewModels = await _commonServices.FollowingServices.TryGetAccountFollowing(accountRecord.Id);
        var followersViewModels = await _commonServices.FollowingServices.TryGetAccountFollowers(accountRecord.Id);
        var postCount = await GetPostCount(accountRecord.Id);
        var memoryCount = await GetMemoryCount(accountRecord.Id);
        var commentCount = await GetCommentCount(accountRecord.Id);
        var reactionCount = await GetReactionCount(accountRecord.Id);

        var viewModel = accountRecord.CreateViewModel(activeOrAdmin, followingViewModels, followersViewModels);

        viewModel.TotalPostCount = postCount;
        viewModel.TotalCommentCount = commentCount;
        viewModel.TotalMemoriesCount = memoryCount;
        viewModel.TotalReactionsCount = reactionCount;

        viewModel.CharactersArray = activeOrAdmin ? characters.Values.ToArray() : characters.Values.Where(x => x.AccountSync && x.CharacterStatus == CharacterStatus2.None).ToArray();

        return viewModel;
    }

    [ComputeMethod]
    public virtual async Task<int> GetPostCount(long accountId)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        return await database.Posts.Where(x => x.AccountId == accountId).CountAsync();
    }

    [ComputeMethod]
    public virtual async Task<int> GetMemoryCount(long accountId)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        return await database.PostTags.Where(x => x.TagType == PostTagType.Account && x.TagId == accountId && x.TagKind == PostTagKind.PostRestored).CountAsync();
    }

    [ComputeMethod]
    public virtual async Task<int> GetCommentCount(long accountId)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        return await database.PostComments.Where(x => x.AccountId == accountId).CountAsync();
    }

    [ComputeMethod]
    public virtual async Task<int> GetReactionCount(long accountId)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var postCount = await database.PostReactions.Where(x => x.AccountId == accountId && x.Reaction > PostReaction.None).CountAsync();
        var commentCount = await database.PostCommentReactions.Where(x => x.AccountId == accountId && x.Reaction > PostReaction.None).CountAsync();

        return postCount + commentCount;
    }

    [ComputeMethod]
    public virtual async Task<bool> CheckIsValidUsername(Session session, string username)
    {
        return await TryReserveUsername(username);
    }

    [ComputeMethod]
    protected virtual async Task<bool> TryReserveUsername(string username)
    {
        if (!DatabaseHelpers.IsValidAccountName(username))
        {
            return false;
        }

        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var usernameExists = await database.Accounts.AnyAsync(x => x.Username == username);
        if (usernameExists)
        {
            return false;
        }

        return true;
    }

    public async Task<bool> TryChangeUsername(Session session, string newUsername)
    {
        if (!DatabaseHelpers.IsValidAccountName(newUsername))
        {
            return false;
        }

        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var usernameExists = await database.Accounts.AnyAsync(x => x.Username == newUsername);
        if (usernameExists)
        {
            return false;
        }

        var accountRecord = await TryGetCurrentSessionAccountRecord(session);
        if (accountRecord == null)
        {
            return false;
        }

        var previousUsername = accountRecord.Username;
        var updateResult = await database.Accounts.Where(x => x.Id == accountRecord.Id && x.Username == accountRecord.Username).AsUpdatable()
            .Set(x => x.Username, newUsername)
            .Set(x => x.UsernameSearchable, DatabaseHelpers.GetSearchableName(newUsername))
            .UpdateAsync();

        if (updateResult == 0)
        {
            return false;
        }

        await TestingHistory(database, new AccountHistoryRecord
        {
            AccountId = accountRecord.Id,
            CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.UsernameChanged
        });

        InvalidateAccountRecord(accountRecord);

        using var computed = Computed.Invalidate();
        _ = TryGetAccountRecordUsername(previousUsername);

        return true;
    }

    public async Task TestingHistory(DatabaseConnection database, AccountHistoryRecord historyRecord)
    {
        if (historyRecord.AccountId == 0)
        {
            throw new NotImplementedException();
        }

        var query = from r in database.AccountHistory
                    where r.AccountId == historyRecord.AccountId &&
                          r.OtherAccountId == historyRecord.OtherAccountId &&
                          r.Type == historyRecord.Type &&
                          r.TargetId == historyRecord.TargetId &&
                          r.TargetPostId == historyRecord.TargetPostId &&
                          r.TargetCommentId == historyRecord.TargetCommentId
                    select r.Id;

        historyRecord.Id = await query.FirstOrDefaultAsync();

        if (historyRecord.Id > 0)
        {
            await database.UpdateAsync(historyRecord);
        }
        else
        {
            historyRecord.Id = await database.InsertWithInt64IdentityAsync(historyRecord);
        }

        using var computed = Computed.Invalidate();
        _ = TryGetAccountHistory(historyRecord.AccountId, 1);
    }

    public async Task<bool> TryChangeIsPrivate(Session session, bool newValue)
    {
        var accountRecord = await TryGetCurrentSessionAccountRecord(session);
        if (accountRecord == null)
        {
            return false;
        }

        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var updateResult = await database.Accounts.Where(x => x.Id == accountRecord.Id && x.IsPrivate == !newValue).AsUpdatable()
            .Set(x => x.IsPrivate, newValue)
            .UpdateAsync();

        if (updateResult == 0)
        {
            return !newValue;
        }

        InvalidateAccountRecord(accountRecord);

        return newValue;
    }

    public async Task<bool> TryChangeBattleTagVisibility(Session session, bool newValue)
    {
        var accountRecord = await TryGetCurrentSessionAccountRecord(session);
        if (accountRecord == null)
        {
            return false;
        }

        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var updateResult = await database.Accounts.Where(x => x.Id == accountRecord.Id && x.BattleTagIsPublic == !newValue).AsUpdatable()
            .Set(x => x.BattleTagIsPublic, newValue)
            .UpdateAsync();

        if (updateResult == 0)
        {
            return !newValue;
        }

        InvalidateAccountRecord(accountRecord);

        return newValue;
    }

    public async Task<string> TryChangeAvatar(Session session, string newAvatar)
    {
        var accountRecord = await TryGetCurrentSessionAccountRecord(session);
        if (accountRecord == null)
        {
            return null;
        }

        newAvatar = HttpUtility.UrlDecode(newAvatar);

        if (accountRecord.Avatar == newAvatar)
        {
            return accountRecord.Avatar;
        }

        if (string.IsNullOrWhiteSpace(newAvatar))
        {
        }
        else if (newAvatar.Length > 200)
        {
            return accountRecord.Avatar;
        }

        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var updateResult = await database.GetUpdateQuery(accountRecord, out _).Set(x => x.Avatar, newAvatar).UpdateAsync();
        if (updateResult == 0)
        {
            return accountRecord.Avatar;
        }

        InvalidateAccountRecord(accountRecord);

        return accountRecord.Avatar;
    }

    public async Task<string> TryChangeSocialLink(Session session, int linkId, StringBody stringBody)
    {
        var accountRecord = await TryGetCurrentSessionAccountRecord(session);
        if (accountRecord == null)
        {
            return null;
        }

        var newValue = stringBody.Value;
        var helper = SocialHelpers.All[linkId];
        var previous = ServerSocialHelpers.GetterFunc[helper.LinkId](accountRecord);
        if (!string.IsNullOrWhiteSpace(newValue) && !helper.ValidatorFunc(newValue))
        {
            return previous;
        }

        if (previous == newValue)
        {
            return previous;
        }

        ServerSocialHelpers.SetterFunc[helper.LinkId](accountRecord, newValue);

        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var updateResult = await database.GetUpdateQuery(accountRecord, out _).Set(ServerSocialHelpers.QuerySetter[helper.LinkId], newValue).UpdateAsync();
        if (updateResult == 0)
        {
            return previous;
        }

        InvalidateAccountRecord(accountRecord);

        return newValue;
    }

    [ComputeMethod]
    public virtual async Task<PostTagInfo[]> TryGetAchievementsByTime(Session session, long timeStamp, int diffInSeconds, string locale)
    {
        var accountRecord = await TryGetCurrentSessionAccountRecord(session);
        if (accountRecord == null)
        {
            return Array.Empty<PostTagInfo>();
        }

        diffInSeconds = Math.Clamp(diffInSeconds, 0, 300);

        var min = Instant.FromUnixTimeMilliseconds(timeStamp).Minus(Duration.FromSeconds(diffInSeconds)).ToUnixTimeMilliseconds();
        var max = Instant.FromUnixTimeMilliseconds(timeStamp).Plus(Duration.FromSeconds(diffInSeconds)).ToUnixTimeMilliseconds();

        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var query = from a in database.CharacterAchievements
                    where a.AccountId == accountRecord.Id && a.AchievementTimeStamp > min && a.AchievementTimeStamp < max
                    select a.AchievementId;

        var results = await query.ToArrayAsync();
        var hashSet = new HashSet<long>();
        var postTagSet = new HashSet<PostTagInfo>();

        foreach (var tagId in results)
        {
            if (hashSet.Add(tagId))
            {
                var postTag = await _commonServices.TagServices.GetTagInfo(PostTagType.Achievement, tagId, null, locale);
                postTagSet.Add(postTag);
            }
        }

        return postTagSet.ToArray();
    }

    [ComputeMethod]
    public virtual async Task<AccountHistoryPageResult> TryGetAccountHistory(Session session, int currentPage)
    {
        var activeAccount = await TryGetActiveAccount(session);
        if (activeAccount == null)
        {
            return null;
        }

        if (currentPage == 0)
        {
            currentPage = 1;
        }

        return await TryGetAccountHistory(activeAccount.Id, currentPage);
    }

    [ComputeMethod]
    public virtual async Task<AccountHistoryPageResult> TryGetAccountHistory(long activeAccountId, int currentPage)
    {
        Exceptions.ThrowIf(activeAccountId == 0);
        Exceptions.ThrowIf(currentPage == 0);

        await using var database = _commonServices.DatabaseProvider.GetDatabase();

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
                               OtherAccountUsername = otherAccount == null ? null : otherAccount.Username,
                               TargetId = record.TargetId,
                               TargetPostId = record.TargetPostId.GetValueOrDefault(),
                               TargetCommentId = record.TargetCommentId.GetValueOrDefault(),
                               CreatedTime = record.CreatedTime.ToUnixTimeMilliseconds()
                           };

        var totalHistoryItemsCounts = await historyQuery.CountAsync();

        var totalPages = (int)Math.Ceiling(totalHistoryItemsCounts / (float)CommonConfig.HistoryItemsPerPage);
        AccountHistoryViewModel[] recentHistoryViewModels;
        if (totalPages == 0)
        {
            recentHistoryViewModels = Array.Empty<AccountHistoryViewModel>();
        }
        else
        {
            currentPage = Math.Clamp(currentPage, 1, totalPages);
            recentHistoryViewModels = await historyQuery.Skip((currentPage - 1) * CommonConfig.HistoryItemsPerPage).Take(CommonConfig.HistoryItemsPerPage).ToArrayAsync();
        }

        return new AccountHistoryPageResult
        {
            CurrentPage = currentPage,
            TotalPages = totalPages,
            ViewModels = recentHistoryViewModels,
        };
    }

    [ComputeMethod]
    protected virtual async Task<AccountRecord> TryGetCurrentSessionAccountRecord(Session session)
    {
        if (session == null)
        {
            return null;
        }

        var user = await _commonServices.Auth.GetUser(session);
        if (user == null || user.IsAuthenticated == false)
        {
            return null;
        }

        //user.MustBeAuthenticated();

        var accountRecord = await TryGetAccountRecordFusionId(user.Id.Value);
        if (accountRecord == null)
        {
            return null;
        }

        return accountRecord;
    }
}