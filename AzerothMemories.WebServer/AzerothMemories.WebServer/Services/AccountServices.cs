namespace AzerothMemories.WebServer.Services;

[RegisterComputeService]
[RegisterAlias(typeof(IAccountServices))]
public class AccountServices : DbServiceBase<AppDbContext>, IAccountServices
{
    private readonly CommonServices _commonServices;

    public AccountServices(IServiceProvider services, CommonServices commonServices) : base(services)
    {
        _commonServices = commonServices;
    }

    [CommandHandler(IsFilter = true, Priority = 1)]
    protected virtual async Task OnSignIn(SignInCommand command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        await context.InvokeRemainingHandlers(cancellationToken);

        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Account_InvalidateAccountRecord>();
            if (invRecord != null)
            {
                _ = DependsOnAccountRecord(invRecord.Id);
                _ = TryGetAccountRecordUsername(invRecord.Username);
                _ = TryGetAccountRecordFusionId(invRecord.FusionId);
            }

            return;
        }

        if (command.AuthenticatedIdentity.Schema.StartsWith("BattleNet-"))
        {
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
            var blizzardRegion = BlizzardRegionExt.FromName(battleNetRegion);

            var accountRecord = await GetOrCreateAccount(userId);
            await using var database = await CreateCommandDbContext(cancellationToken).ConfigureAwait(false);
            database.Attach(accountRecord);

            accountRecord.BlizzardId = blizzardId;
            accountRecord.BlizzardRegionId = blizzardRegion;
            accountRecord.BattleTag = battleTag;
            accountRecord.BattleNetToken = battleNetToken;
            accountRecord.BattleNetTokenExpiresAt = Instant.FromUnixTimeMilliseconds(battleNetTokenExpires);

            if (string.IsNullOrWhiteSpace(accountRecord.Username))
            {
                var newUsername = $"User-{accountRecord.Id}";

                accountRecord.Username = newUsername;
                accountRecord.UsernameSearchable = DatabaseHelpers.GetSearchableName(newUsername);
            }

            await database.SaveChangesAsync(cancellationToken);

            context.Operation().Items.Set(new Account_InvalidateAccountRecord(accountRecord.Id, accountRecord.Username, accountRecord.FusionId));
        }
        else if (command.AuthenticatedIdentity.Schema.StartsWith("ToDo-"))
        {
            throw new NotImplementedException();
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    [ComputeMethod]
    public virtual Task<long> DependsOnAccountRecord(long accountId)
    {
        return Task.FromResult(accountId);
    }

    [ComputeMethod]
    public virtual Task<long> DependsOnAccountAchievements(long accountId)
    {
        return Task.FromResult(accountId);
    }

    [ComputeMethod]
    protected virtual async Task<AccountRecord> GetOrCreateAccount(string userId)
    {
        var accountRecord = await TryGetAccountRecordFusionId(userId);
        if (accountRecord == null)
        {
            accountRecord = new AccountRecord
            {
                FusionId = userId,
                CreatedDateTime = SystemClock.Instance.GetCurrentInstant()
            };

            await using var database = CreateDbContext(true);
            await database.Accounts.AddAsync(accountRecord);
            await database.SaveChangesAsync();

            //await DependsOnAccountRecord(accountRecord.Id);

            await AddNewHistoryItem(new Account_AddNewHistoryItem
            {
                AccountId = accountRecord.Id,
                Type = AccountHistoryType.AccountCreated
            });

            //InvalidateAccountRecord(accountRecord);
        }

        Exceptions.ThrowIf(accountRecord.Id == 0);

        return accountRecord;
    }

    [ComputeMethod]
    public virtual async Task<AccountRecord> TryGetAccountRecord(long id)
    {
        await DependsOnAccountRecord(id);

        await using var database = CreateDbContext();
        var accountRecord = await database.Accounts.FirstOrDefaultAsync(a => a.Id == id);

        return accountRecord;
    }

    [ComputeMethod]
    public virtual async Task<AccountRecord> TryGetAccountRecordFusionId(string fusionId)
    {
        await using var database = CreateDbContext();
        var accountRecord = await database.Accounts.FirstOrDefaultAsync(a => a.FusionId == fusionId);
        if (accountRecord != null)
        {
            await DependsOnAccountRecord(accountRecord.Id);
        }
        return accountRecord;
    }

    [ComputeMethod]
    public virtual async Task<AccountRecord> TryGetAccountRecordUsername(string username)
    {
        await using var database = CreateDbContext();
        var accountRecord = await database.Accounts.FirstOrDefaultAsync(a => a.Username == username);
        if (accountRecord != null)
        {
            await DependsOnAccountRecord(accountRecord.Id);
        }
        return accountRecord;
    }

    [ComputeMethod]
    public virtual async Task<AccountViewModel> TryGetActiveAccount(Session session)
    {
        var accountRecord = await TryGetActiveAccountRecord(session);
        if (accountRecord == null)
        {
            return null;
        }

        await DependsOnAccountRecord(accountRecord.Id);
        return await CreateAccountViewModel(accountRecord, true);
    }

    [ComputeMethod]
    public virtual async Task<AccountViewModel> TryGetAccountById(Session session, long accountId)
    {
        await DependsOnAccountRecord(accountId);

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

        await DependsOnAccountRecord(accountRecord.Id);
        return await CreateAccountViewModel(accountRecord, sessionAccount != null && sessionAccount.AccountType >= AccountType.Admin);
    }

    public async Task<bool> TryEnqueueUpdate(Session session)
    {
        var accountRecord = await TryGetActiveAccountRecord(session);
        if (accountRecord == null)
        {
            return false;
        }

        await _commonServices.BlizzardUpdateHandler.TryUpdate(accountRecord, BlizzardUpdatePriority.Account);

        return true;
    }

    [ComputeMethod]
    public virtual async Task<AccountViewModel> CreateAccountViewModel(AccountRecord accountRecord, bool activeOrAdmin)
    {
        await DependsOnAccountRecord(accountRecord.Id);

        await _commonServices.BlizzardUpdateHandler.TryUpdate(accountRecord, BlizzardUpdatePriority.Account);

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
        await using var database = CreateDbContext();
        return await database.Posts.Where(x => x.AccountId == accountId).CountAsync();
    }

    [ComputeMethod]
    public virtual async Task<int> GetMemoryCount(long accountId)
    {
        await using var database = CreateDbContext();
        return await database.PostTags.Where(x => x.TagType == PostTagType.Account && x.TagId == accountId && x.TagKind == PostTagKind.PostRestored).CountAsync();
    }

    [ComputeMethod]
    public virtual async Task<int> GetCommentCount(long accountId)
    {
        await using var database = CreateDbContext();
        return await database.PostComments.Where(x => x.AccountId == accountId).CountAsync();
    }

    [ComputeMethod]
    public virtual async Task<int> GetReactionCount(long accountId)
    {
        await using var database = CreateDbContext();
        var postCount = await database.PostReactions.Where(x => x.AccountId == accountId && x.Reaction > PostReaction.None).CountAsync();
        var commentCount = await database.PostCommentReactions.Where(x => x.AccountId == accountId && x.Reaction > PostReaction.None).CountAsync();

        return postCount + commentCount;
    }

    [ComputeMethod]
    public virtual async Task<bool> CheckIsValidUsername(Session session, string username)
    {
        return await CheckIsValidUsername(username);
    }

    [ComputeMethod]
    protected virtual async Task<bool> CheckIsValidUsername(string username)
    {
        if (!DatabaseHelpers.IsValidAccountName(username))
        {
            return false;
        }

        await using var database = CreateDbContext();
        var usernameExists = await database.Accounts.AnyAsync(x => x.Username == username);
        if (usernameExists)
        {
            return false;
        }

        return true;
    }

    [CommandHandler]
    public virtual async Task<bool> TryChangeUsername(Account_TryChangeUsername command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Account_InvalidateAccountRecord>();
            if (invRecord != null)
            {
                _ = DependsOnAccountRecord(invRecord.Id);

                var username = invRecord.Username;
                if (!string.IsNullOrWhiteSpace(username))
                {
                    _ = CheckIsValidUsername(username);
                }
            }

            var invPreviousUsername = context.Operation().Items.Get<string>();
            if (!string.IsNullOrWhiteSpace(invPreviousUsername))
            {
                _ = CheckIsValidUsername(invPreviousUsername);
                _ = TryGetAccountRecordUsername(invPreviousUsername);
            }

            return default;
        }

        if (!DatabaseHelpers.IsValidAccountName(command.NewUsername))
        {
            return false;
        }

        var accountRecord = await TryGetActiveAccountRecord(command.Session);
        if (accountRecord == null)
        {
            return false;
        }

        await using var database = await CreateCommandDbContext(cancellationToken);
        var usernameExists = await database.Accounts.AnyAsync(x => x.Username == command.NewUsername, cancellationToken);
        if (usernameExists)
        {
            return false;
        }

        var previousUsername = accountRecord.Username;
        database.Attach(accountRecord);
        accountRecord.Username = command.NewUsername;
        accountRecord.UsernameSearchable = DatabaseHelpers.GetSearchableName(command.NewUsername);

        await database.SaveChangesAsync(cancellationToken);

        await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
        {
            AccountId = accountRecord.Id,
            Type = AccountHistoryType.UsernameChanged
            //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
        }, cancellationToken);

        context.Operation().Items.Set(new Account_InvalidateAccountRecord(accountRecord.Id, accountRecord.Username, accountRecord.FusionId));
        context.Operation().Items.Set(previousUsername);

        return true;
    }

    [CommandHandler]
    public virtual async Task<bool> TryChangeIsPrivate(Account_TryChangeIsPrivate command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Account_InvalidateAccountRecord>();
            if (invRecord != null)
            {
                _ = DependsOnAccountRecord(invRecord.Id);
            }

            return default;
        }

        var accountRecord = await TryGetActiveAccountRecord(command.Session);
        if (accountRecord == null)
        {
            return false;
        }

        await using var database = await CreateCommandDbContext(cancellationToken);
        database.Attach(accountRecord);
        accountRecord.IsPrivate = command.NewValue;
        await database.SaveChangesAsync(cancellationToken);

        context.Operation().Items.Set(new Account_InvalidateAccountRecord(accountRecord.Id, accountRecord.Username, accountRecord.FusionId));

        return command.NewValue;
    }

    [CommandHandler]
    public virtual async Task<bool> TryChangeBattleTagVisibility(Account_TryChangeBattleTagVisibility command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Account_InvalidateAccountRecord>();
            if (invRecord != null)
            {
                _ = DependsOnAccountRecord(invRecord.Id);
            }

            return default;
        }

        var accountRecord = await TryGetActiveAccountRecord(command.Session);
        if (accountRecord == null)
        {
            return false;
        }

        await using var database = await CreateCommandDbContext(cancellationToken);
        database.Attach(accountRecord);
        accountRecord.BattleTagIsPublic = command.NewValue;

        await database.SaveChangesAsync(cancellationToken);

        context.Operation().Items.Set(new Account_InvalidateAccountRecord(accountRecord.Id, accountRecord.Username, accountRecord.FusionId));

        return command.NewValue;
    }

    [CommandHandler]
    public virtual async Task<string> TryChangeAvatar(Account_TryChangeAvatar command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Account_InvalidateAccountRecord>();
            if (invRecord != null)
            {
                _ = DependsOnAccountRecord(invRecord.Id);
            }

            return default;
        }

        var accountViewModel = await TryGetActiveAccount(command.Session);
        if (accountViewModel == null)
        {
            return null;
        }

        var newAvatar = command.NewAvatar;
        if (accountViewModel.Avatar == newAvatar)
        {
            return accountViewModel.Avatar;
        }

        if (string.IsNullOrWhiteSpace(newAvatar))
        {
            newAvatar = null;
        }
        else if (newAvatar.StartsWith("https://render") && newAvatar.Contains(".worldofwarcraft.com"))
        {
            var character = accountViewModel.GetAllCharactersSafe().FirstOrDefault(x => x.AvatarLink == newAvatar);
            if (character == null)
            {
                return null;
            }
        }
        else
        {
            return null;
        }

        var accountRecord = await TryGetActiveAccountRecord(command.Session);
        await using var database = await CreateCommandDbContext(cancellationToken);
        database.Attach(accountRecord);
        accountRecord.Avatar = newAvatar;

        await database.SaveChangesAsync(cancellationToken);

        context.Operation().Items.Set(new Account_InvalidateAccountRecord(accountRecord.Id, accountRecord.Username, accountRecord.FusionId));

        return newAvatar;
    }

    [CommandHandler]
    public virtual async Task<string> TryChangeSocialLink(Account_TryChangeSocialLink command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Account_InvalidateAccountRecord>();
            if (invRecord != null)
            {
                _ = DependsOnAccountRecord(invRecord.Id);
            }

            return default;
        }

        var accountRecord = await TryGetActiveAccountRecord(command.Session);
        if (accountRecord == null)
        {
            return null;
        }

        var newValue = command.NewValue;
        var helper = SocialHelpers.All[command.LinkId];
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

        await using var database = await CreateCommandDbContext(cancellationToken);
        database.Attach(accountRecord);

        ServerSocialHelpers.QuerySetter[helper.LinkId](accountRecord, newValue);

        await database.SaveChangesAsync(cancellationToken);

        context.Operation().Items.Set(new Account_InvalidateAccountRecord(accountRecord.Id, accountRecord.Username, accountRecord.FusionId));

        return newValue;
    }

    [CommandHandler]
    public virtual async Task<bool> AddNewHistoryItem(Account_AddNewHistoryItem command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Account_InvalidateFollowing>();
            if (invRecord != null)
            {
                _ = TryGetAccountHistory(invRecord.AccountId, invRecord.Page);
            }

            return default;
        }

        if (command.AccountId == 0)
        {
            throw new NotImplementedException();
        }

        await using var database = await CreateCommandDbContext(cancellationToken);
        var query = from r in database.AccountHistory
                    where r.AccountId == command.AccountId &&
                          r.OtherAccountId == command.OtherAccountId &&
                          r.Type == command.Type &&
                          r.TargetId == command.TargetId &&
                          r.TargetPostId == command.TargetPostId &&
                          r.TargetCommentId == command.TargetCommentId
                    select r;

        var record = await query.FirstOrDefaultAsync(cancellationToken);
        if (record == null)
        {
            record = new AccountHistoryRecord();

            database.AccountHistory.Add(record);
        }

        record.Type = command.Type;
        record.AccountId = command.AccountId;
        record.OtherAccountId = command.OtherAccountId;
        record.TargetId = command.TargetId;
        record.TargetPostId = command.TargetPostId;
        record.TargetCommentId = command.TargetCommentId;
        record.CreatedTime = SystemClock.Instance.GetCurrentInstant();

        await database.SaveChangesAsync(cancellationToken);

        context.Operation().Items.Set(new Account_InvalidateFollowing(record.AccountId, 1));

        return true;
    }

    [ComputeMethod]
    public virtual async Task<PostTagInfo[]> TryGetAchievementsByTime(Session session, long timeStamp, int diffInSeconds, string locale)
    {
        var accountRecord = await TryGetActiveAccountRecord(session);
        if (accountRecord == null)
        {
            return Array.Empty<PostTagInfo>();
        }

        await DependsOnAccountAchievements(accountRecord.Id);

        diffInSeconds = Math.Clamp(diffInSeconds, 0, 300);

        var min = Instant.FromUnixTimeMilliseconds(timeStamp).Minus(Duration.FromSeconds(diffInSeconds));
        var max = Instant.FromUnixTimeMilliseconds(timeStamp).Plus(Duration.FromSeconds(diffInSeconds));

        await using var database = CreateDbContext();
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

        await using var database = CreateDbContext();

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
    protected virtual async Task<AccountRecord> TryGetActiveAccountRecord(Session session)
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

        var accountRecord = await TryGetAccountRecordFusionId(user.Id.Value);
        if (accountRecord == null)
        {
            return null;
        }

        await DependsOnAccountRecord(accountRecord.Id);

        return accountRecord;
    }
}