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

            using var computed = Computed.Invalidate();
            _ = TryGetAccountRecord(accountRecord.Id);
            _ = TryGetAccountRecordFusionId(userId);
            _ = TryGetAccountRecordUsername(accountRecord.Username);
        }

        await _commonServices.BlizzardUpdateHandler.TryUpdateAccount(database, accountRecord);
    }

    private async Task<AccountRecord> GetOrCreateAccount(IDataContext database, string userId)
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

            using var computed = Computed.Invalidate();

            _ = TryGetAccountRecord(accountRecord.Id);
            _ = TryGetAccountRecordFusionId(accountRecord.FusionId);
            _ = TryGetAccountRecordUsername(accountRecord.Username);
            _ = _commonServices.TagServices.TryGetUserTagInfo(PostTagType.Account, accountRecord.Id);
        }

        return accountRecord;
    }

    [ComputeMethod]
    public virtual async Task<AccountRecord> TryGetAccountRecord(long id)
    {
        if (Computed.IsInvalidating())
        {
            //return null;
        }

        await using var dbContext = _commonServices.DatabaseProvider.GetDatabase();
        var user = await dbContext.Accounts.Where(a => a.Id == id).FirstOrDefaultAsync();

        return user;
    }

    [ComputeMethod]
    public virtual async Task<AccountRecord> TryGetAccountRecordFusionId(string fusionId)
    {
        if (Computed.IsInvalidating())
        {
            //return null;
        }

        await using var dbContext = _commonServices.DatabaseProvider.GetDatabase();
        var user = await dbContext.Accounts.Where(a => a.FusionId == fusionId).FirstOrDefaultAsync();
        return user;
    }

    [ComputeMethod]
    public virtual async Task<AccountRecord> TryGetAccountRecordUsername(string username)
    {
        if (Computed.IsInvalidating())
        {
            //return null;
        }

        await using var dbContext = _commonServices.DatabaseProvider.GetDatabase();
        var user = await dbContext.Accounts.Where(a => a.Username == username).FirstOrDefaultAsync();
        return user;
    }

    [ComputeMethod]
    public virtual async Task<ActiveAccountViewModel> TryGetAccount(Session session, CancellationToken cancellationToken = default)
    {
        var accountRecord = await GetCurrentSessionAccountRecord(session, cancellationToken);
        if (accountRecord == null)
        {
            return null;
        }

        var characters = await _commonServices.CharacterServices.TryGetAllAccountCharacters(accountRecord.Id);
        var viewModel = accountRecord.CreateActiveAccountViewModel(characters);

        return viewModel;
    }

    [ComputeMethod]
    public virtual async Task<AccountViewModel> TryGetAccountById(Session session, long accountId, CancellationToken cancellationToken = default)
    {
        var sessionAccount = await TryGetAccount(session, cancellationToken);
        if (sessionAccount != null && sessionAccount.Id == accountId)
        {
            return sessionAccount;
        }

        var accountRecord = await TryGetAccountRecord(accountId);
        if (accountRecord == null)
        {
            return null;
        }

        var characters = await _commonServices.CharacterServices.TryGetAllAccountCharacters(accountRecord.Id);
        return accountRecord.CreateAccountViewModel(characters);
    }

    [ComputeMethod]
    public virtual async Task<AccountViewModel> TryGetAccountByUsername(Session session, string username, CancellationToken cancellationToken = default)
    {
        var sessionAccount = await TryGetAccount(session, cancellationToken);
        if (sessionAccount != null && sessionAccount.Username == username)
        {
            return sessionAccount;
        }

        var accountRecord = await TryGetAccountRecordUsername(username);
        if (accountRecord == null)
        {
            return null;
        }

        var characters = await _commonServices.CharacterServices.TryGetAllAccountCharacters(accountRecord.Id);
        return accountRecord.CreateAccountViewModel(characters);
    }

    [ComputeMethod]
    public virtual async Task<bool> TryReserveUsername(Session session, string username, CancellationToken cancellationToken = default)
    {
        return await TryReserveUsername(username);
    }

    [ComputeMethod]
    public virtual async Task<bool> TryReserveUsername(string username)
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

    public async Task<bool> TryChangeUsername(Session session, string newUsername, CancellationToken cancellationToken = default)
    {
        if (!DatabaseHelpers.IsValidAccountName(newUsername))
        {
            return false;
        }

        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var usernameExists = await database.Accounts.AnyAsync(x => x.Username == newUsername, cancellationToken);
        if (usernameExists)
        {
            return false;
        }

        var accountRecord = await GetCurrentSessionAccountRecord(session, cancellationToken);
        if (accountRecord == null)
        {
            return false;
        }

        var updateResult = await database.Accounts.Where(x => x.Id == accountRecord.Id && x.Username == accountRecord.Username).AsUpdatable()
            .Set(x => x.Username, newUsername)
            .Set(x => x.UsernameSearchable, DatabaseHelpers.GetSearchableName(newUsername))
            .UpdateAsync(cancellationToken);

        if (updateResult == 0)
        {
            return false;
        }

        using var computed = Computed.Invalidate();

        _ = TryGetAccountRecord(accountRecord.Id);
        _ = TryGetAccountRecordFusionId(accountRecord.FusionId);
        _ = TryGetAccountRecordUsername(accountRecord.Username);
        _ = TryReserveUsername(accountRecord.Username);
        _ = _commonServices.TagServices.TryGetUserTagInfo(PostTagType.Account, accountRecord.Id);

        return true;
    }

    public async Task<bool> TryChangeIsPrivate(Session session, bool newValue, CancellationToken cancellationToken = default)
    {
        var accountRecord = await GetCurrentSessionAccountRecord(session, cancellationToken);
        if (accountRecord == null)
        {
            return false;
        }

        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var updateResult = await database.Accounts.Where(x => x.Id == accountRecord.Id && x.IsPrivate == !newValue).AsUpdatable()
            .Set(x => x.IsPrivate, newValue)
            .UpdateAsync(cancellationToken);

        if (updateResult == 0)
        {
            return !newValue;
        }

        using var computed = Computed.Invalidate();

        _ = TryGetAccountRecord(accountRecord.Id);
        _ = TryGetAccountRecordFusionId(accountRecord.FusionId);
        _ = TryGetAccountRecordUsername(accountRecord.Username);

        return newValue;
    }

    public async Task<bool> TryChangeBattleTagVisibility(Session session, bool newValue, CancellationToken cancellationToken = default)
    {
        var accountRecord = await GetCurrentSessionAccountRecord(session, cancellationToken);
        if (accountRecord == null)
        {
            return false;
        }

        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var updateResult = await database.Accounts.Where(x => x.Id == accountRecord.Id && x.BattleTagIsPublic == !newValue).AsUpdatable()
            .Set(x => x.BattleTagIsPublic, newValue)
            .UpdateAsync(cancellationToken);

        if (updateResult == 0)
        {
            return !newValue;
        }

        using var computed = Computed.Invalidate();

        _ = TryGetAccountRecord(accountRecord.Id);
        _ = TryGetAccountRecordFusionId(accountRecord.FusionId);
        _ = TryGetAccountRecordUsername(accountRecord.Username);

        return newValue;
    }

    public async Task<string> TryChangeAvatar(Session session, string newAvatar)
    {
        var accountRecord = await GetCurrentSessionAccountRecord(session);
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

        using var computed = Computed.Invalidate();

        _ = TryGetAccountRecord(accountRecord.Id);
        _ = TryGetAccountRecordFusionId(accountRecord.FusionId);
        _ = TryGetAccountRecordUsername(accountRecord.Username);
        _ = _commonServices.TagServices.TryGetUserTagInfo(PostTagType.Account, accountRecord.Id);

        return accountRecord.Avatar;
    }

    public async Task<string> TryChangeSocialLink(Session session, int linkId, string newValue)
    {
        var accountRecord = await GetCurrentSessionAccountRecord(session);
        if (accountRecord == null)
        {
            return null;
        }

        newValue = HttpUtility.UrlDecode(newValue);

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

        using var computed = Computed.Invalidate();

        _ = TryGetAccountRecord(accountRecord.Id);
        _ = TryGetAccountRecordFusionId(accountRecord.FusionId);
        _ = TryGetAccountRecordUsername(accountRecord.Username);

        return newValue;
    }

    [ComputeMethod]
    public virtual async Task<PostTagInfo[]> TryGetAchievementsByTime(Session session, long timeStamp, int diffInSeconds, string locale = null, CancellationToken cancellationToken = default)
    {
        var accountRecord = await GetCurrentSessionAccountRecord(session, cancellationToken);
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

        var results = await query.ToArrayAsync(cancellationToken);
        var hashSet = new HashSet<long>();
        var postTagSet = new HashSet<PostTagInfo>();

        foreach (var tagId in results)
        {
            if (hashSet.Add(tagId))
            {
                var postTag = await _commonServices.TagServices.GetTagInfo(PostTagType.Achievement, tagId, locale);
                postTagSet.Add(postTag);
            }
        }

        return postTagSet.ToArray();
    }

    [ComputeMethod]
    public virtual async Task<AccountRecord> GetCurrentSessionAccountRecord(Session session, CancellationToken cancellationToken = default)
    {
        if (session == null)
        {
            return null;
        }

        var user = await _commonServices.Auth.GetUser(session, cancellationToken);
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