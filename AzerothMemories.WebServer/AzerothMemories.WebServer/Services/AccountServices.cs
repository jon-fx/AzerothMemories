﻿using AzerothMemories.WebServer.Services.Handlers;

namespace AzerothMemories.WebServer.Services;

[RegisterComputeService]
[RegisterAlias(typeof(IAccountServices))]
public class AccountServices : DbServiceBase<AppDbContext>, IAccountServices, IDatabaseContextProvider
{
    private readonly CommonServices _commonServices;
    private readonly IDbSessionInfoRepo<AppDbContext, DbSessionInfo<string>, string> _sessionRepo;

    public AccountServices(IServiceProvider services, CommonServices commonServices) : base(services)
    {
        _commonServices = commonServices;
        _sessionRepo = services.GetRequiredService<IDbSessionInfoRepo<AppDbContext, DbSessionInfo<string>, string>>();
    }

    [CommandHandler(IsFilter = true, Priority = 1)]
    protected virtual async Task OnSignInCommand(SignInCommand command, CancellationToken cancellationToken)
    {
        await AccountServices_OnSignInCommand.TryHandle(_commonServices, _sessionRepo, this, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler(IsFilter = true, Priority = 1)]
    protected virtual async Task OnSignOutCommand(SignOutCommand command, CancellationToken cancellationToken)
    {
        await AccountServices_OnSignOutCommand.TryHandle(_commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler(IsFilter = true, Priority = 1)]
    protected virtual async Task OnSetupSessionCommand(SetupSessionCommand command, CancellationToken cancellationToken)
    {
        await AccountServices_OnSetupSessionCommand.TryHandle(_commonServices, this, command, cancellationToken).ConfigureAwait(false);
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
    public virtual async Task<AccountRecord> GetOrCreateAccount(string userId)
    {
        var accountRecord = await TryGetAccountRecordFusionId(userId).ConfigureAwait(false);
        if (accountRecord == null)
        {
            accountRecord = new AccountRecord
            {
                FusionId = userId,
                AccountFlags = AccountFlags.AlphaUser,
                CreatedDateTime = SystemClock.Instance.GetCurrentInstant(),
            };

            await using var database = CreateDbContext(true);
            await database.Accounts.AddAsync(accountRecord).ConfigureAwait(false);
            await database.SaveChangesAsync().ConfigureAwait(false);

            //await DependsOnAccountRecord(accountRecord.Id);

            await AddNewHistoryItem(new Account_AddNewHistoryItem
            {
                AccountId = accountRecord.Id,
                Type = AccountHistoryType.AccountCreated
            }).ConfigureAwait(false);

            //InvalidateAccountRecord(accountRecord);
        }

        Exceptions.ThrowIf(accountRecord.Id == 0);

        return accountRecord;
    }

    [ComputeMethod]
    public virtual async Task<AccountRecord> TryGetAccountRecord(int id)
    {
        await DependsOnAccountRecord(id).ConfigureAwait(false);

        await using var database = CreateDbContext();
        var accountRecord = await database.Accounts.FirstOrDefaultAsync(a => a.Id == id).ConfigureAwait(false);

        return accountRecord;
    }

    [ComputeMethod]
    public virtual async Task<AccountRecord> TryGetAccountRecordFusionId(string fusionId)
    {
        await using var database = CreateDbContext();
        var accountRecord = await database.Accounts.FirstOrDefaultAsync(a => a.FusionId == fusionId).ConfigureAwait(false);
        if (accountRecord != null)
        {
            await DependsOnAccountRecord(accountRecord.Id).ConfigureAwait(false);
        }

        return accountRecord;
    }

    [ComputeMethod]
    public virtual async Task<AccountRecord> TryGetAccountRecordUsername(string username)
    {
        await using var database = CreateDbContext();
        var accountRecord = await database.Accounts.FirstOrDefaultAsync(a => a.Username == username).ConfigureAwait(false);
        if (accountRecord != null)
        {
            await DependsOnAccountRecord(accountRecord.Id).ConfigureAwait(false);
        }

        return accountRecord;
    }

    [ComputeMethod]
    public virtual async Task<AccountViewModel> TryGetActiveAccount(Session session)
    {
        var accountRecord = await TryGetActiveAccountRecord(session).ConfigureAwait(false);
        if (accountRecord == null)
        {
            return null;
        }

        await DependsOnAccountRecord(accountRecord.Id).ConfigureAwait(false);
        return await CreateAccountViewModel(accountRecord, true).ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<AccountViewModel> TryGetAccountById(Session session, int accountId)
    {
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
    public virtual async Task<AccountViewModel> TryGetAccountByUsername(Session session, string username)
    {
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

    public async Task<bool> TryEnqueueUpdate(Session session)
    {
        var accountRecord = await TryGetActiveAccountRecord(session).ConfigureAwait(false);
        if (accountRecord == null)
        {
            return false;
        }

        await _commonServices.BlizzardUpdateHandler.TryUpdate(accountRecord, BlizzardUpdatePriority.Account).ConfigureAwait(false);

        return true;
    }

    [ComputeMethod]
    public virtual async Task<AccountViewModel> CreateAccountViewModel(AccountRecord accountRecord, bool activeOrAdmin)
    {
        await DependsOnAccountRecord(accountRecord.Id).ConfigureAwait(false);

        await _commonServices.BlizzardUpdateHandler.TryUpdate(accountRecord, BlizzardUpdatePriority.Account).ConfigureAwait(false);

        var characters = await _commonServices.CharacterServices.TryGetAllAccountCharacters(accountRecord.Id).ConfigureAwait(false);
        var followingViewModels = await _commonServices.FollowingServices.TryGetAccountFollowing(accountRecord.Id).ConfigureAwait(false);
        var followersViewModels = await _commonServices.FollowingServices.TryGetAccountFollowers(accountRecord.Id).ConfigureAwait(false);
        var postCount = await GetPostCount(accountRecord.Id).ConfigureAwait(false);
        var memoryCount = await GetMemoryCount(accountRecord.Id).ConfigureAwait(false);
        var commentCount = await GetCommentCount(accountRecord.Id).ConfigureAwait(false);
        var reactionCount = await GetReactionCount(accountRecord.Id).ConfigureAwait(false);

        var viewModel = accountRecord.CreateViewModel(_commonServices, activeOrAdmin, followingViewModels, followersViewModels);

        viewModel.TotalPostCount = postCount;
        viewModel.TotalCommentCount = commentCount;
        viewModel.TotalMemoriesCount = memoryCount;
        viewModel.TotalReactionsCount = reactionCount;

        viewModel.CharactersArray = activeOrAdmin ? characters.Values.ToArray() : characters.Values.Where(x => x.AccountSync && x.CharacterStatus == CharacterStatus2.None).ToArray();

        return viewModel;
    }

    [ComputeMethod]
    public virtual async Task<int> GetPostCount(int accountId)
    {
        await using var database = CreateDbContext();
        return await database.Posts.Where(x => x.AccountId == accountId).CountAsync().ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<int> GetMemoryCount(int accountId)
    {
        await using var database = CreateDbContext();
        return await database.PostTags.Where(x => x.TagType == PostTagType.Account && x.TagId == accountId && x.TagKind == PostTagKind.PostRestored).CountAsync().ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<int> GetCommentCount(int accountId)
    {
        await using var database = CreateDbContext();
        return await database.PostComments.Where(x => x.AccountId == accountId).CountAsync().ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<int> GetReactionCount(int accountId)
    {
        await using var database = CreateDbContext();
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
        if (!DatabaseHelpers.IsValidAccountName(username))
        {
            return false;
        }

        await using var database = CreateDbContext();
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
        return await AccountServices_TryChangeUsername.TryHandle(_commonServices, this, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<bool> TryChangeIsPrivate(Account_TryChangeIsPrivate command, CancellationToken cancellationToken = default)
    {
        return await AccountServices_TryChangeIsPrivate.TryHandle(_commonServices, this, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<bool> TryChangeBattleTagVisibility(Account_TryChangeBattleTagVisibility command, CancellationToken cancellationToken = default)
    {
        return await AccountServices_TryChangeBattleTagVisibility.TryHandle(_commonServices, this, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<string> TryChangeAvatar(Account_TryChangeAvatar command, CancellationToken cancellationToken = default)
    {
        return await AccountServices_TryChangeAvatar.TryHandle(_commonServices, this, command, cancellationToken).ConfigureAwait(false);
    }

    public Task<string> TryChangeAvatarUpload(Session session, byte[] buffer)
    {
        try
        {
            using var memoryStream = new MemoryStream(buffer);
            using var binaryReader = new BinaryReader(memoryStream);

            var byteCount = binaryReader.ReadInt32();
            var imageBuffer = binaryReader.ReadBytes(byteCount);

            var command = new Account_TryChangeAvatarUpload
            {
                Session = session,
                ImageData = imageBuffer,
            };

            return TryChangeAvatarUpload(command);
        }
        catch (Exception)
        {
            return Task.FromResult(string.Empty);
        }
    }

    [CommandHandler]
    public virtual async Task<string> TryChangeAvatarUpload(Account_TryChangeAvatarUpload command, CancellationToken cancellationToken = default)
    {
        return await AccountServices_TryChangeAvatarUpload.TryHandle(_commonServices, this, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<string> TryChangeSocialLink(Account_TryChangeSocialLink command, CancellationToken cancellationToken = default)
    {
        return await AccountServices_TryChangeSocialLink.TryHandle(_commonServices, this, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<bool> AddNewHistoryItem(Account_AddNewHistoryItem command, CancellationToken cancellationToken = default)
    {
        return await AccountServices_AddNewHistoryItem.TryHandle(_commonServices, this, command, cancellationToken).ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<PostTagInfo[]> TryGetAchievementsByTime(Session session, long timeStamp, int diffInSeconds, ServerSideLocale locale)
    {
        var accountRecord = await TryGetActiveAccountRecord(session).ConfigureAwait(false);
        if (accountRecord == null)
        {
            return Array.Empty<PostTagInfo>();
        }

        await DependsOnAccountAchievements(accountRecord.Id).ConfigureAwait(false);

        diffInSeconds = Math.Clamp(diffInSeconds, 0, 300);

        var min = Instant.FromUnixTimeMilliseconds(timeStamp).Minus(Duration.FromSeconds(diffInSeconds));
        var max = Instant.FromUnixTimeMilliseconds(timeStamp).Plus(Duration.FromSeconds(diffInSeconds));

        await using var database = CreateDbContext();
        var query = from a in database.CharacterAchievements
                    where a.AccountId == accountRecord.Id && a.AchievementTimeStamp > min && a.AchievementTimeStamp < max
                    select a.AchievementId;

        var results = await query.ToArrayAsync().ConfigureAwait(false);
        var hashSet = new HashSet<int>();
        var postTagSet = new HashSet<PostTagInfo>();

        foreach (var tagId in results)
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
    public virtual async Task<AccountHistoryPageResult> TryGetAccountHistory(Session session, int currentPage)
    {
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
    public virtual async Task<AccountHistoryPageResult> TryGetAccountHistory(int activeAccountId, int currentPage)
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

        var totalHistoryItemsCounts = await historyQuery.CountAsync().ConfigureAwait(false);

        var totalPages = (int)Math.Ceiling(totalHistoryItemsCounts / (float)CommonConfig.HistoryItemsPerPage);
        AccountHistoryViewModel[] recentHistoryViewModels;
        if (totalPages == 0)
        {
            recentHistoryViewModels = Array.Empty<AccountHistoryViewModel>();
        }
        else
        {
            currentPage = Math.Clamp(currentPage, 1, totalPages);
            recentHistoryViewModels = await historyQuery.Skip((currentPage - 1) * CommonConfig.HistoryItemsPerPage).Take(CommonConfig.HistoryItemsPerPage).ToArrayAsync().ConfigureAwait(false);
        }

        return new AccountHistoryPageResult
        {
            CurrentPage = currentPage,
            TotalPages = totalPages,
            ViewModels = recentHistoryViewModels,
        };
    }

    [ComputeMethod]
    public virtual async Task<AccountRecord> TryGetActiveAccountRecord(Session session)
    {
        if (session == null)
        {
            return null;
        }

        var user = await _commonServices.Auth.GetUser(session).ConfigureAwait(false);
        if (user == null || user.IsAuthenticated == false)
        {
            return null;
        }

        var accountRecord = await TryGetAccountRecordFusionId(user.Id.Value).ConfigureAwait(false);
        if (accountRecord == null)
        {
            return null;
        }

        await DependsOnAccountRecord(accountRecord.Id).ConfigureAwait(false);

        return accountRecord;
    }

    public Task<AppDbContext> CreateCommandDbContextNow(CancellationToken cancellationToken)
    {
        return CreateCommandDbContext(true, cancellationToken);
    }
}