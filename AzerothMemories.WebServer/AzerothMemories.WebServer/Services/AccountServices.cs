using SixLabors.ImageSharp;
using System.Security.Cryptography;
using System.Text;

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

        if (CheckAndChange.Check(ref accountRecord.BattleNetTokenExpiresAt, DateTimeOffset.FromUnixTimeMilliseconds(battleNetTokenExpires), ref changed))
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
                CreatedDateTime = DateTimeOffset.UtcNow,
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
    public virtual async Task<AccountViewModel> TryGetAccount(Session session, long accountId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    [ComputeMethod]
    public virtual async Task<AccountViewModel> TryGetAccount(Session session, string username, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
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

    [ComputeMethod]
    public virtual async Task<PostTagInfo[]> TryGetAchievementsByTime(Session session, long timeStamp, int diffInSeconds, CancellationToken cancellationToken = default)
    {
        var accountRecord = await GetCurrentSessionAccountRecord(session, cancellationToken);
        if (accountRecord == null)
        {
            return Array.Empty<PostTagInfo>();
        }

        diffInSeconds = Math.Clamp(diffInSeconds, 0, 300);

        var min = (DateTimeOffset.FromUnixTimeMilliseconds(timeStamp) - TimeSpan.FromSeconds(diffInSeconds)).ToUnixTimeMilliseconds();
        var max = (DateTimeOffset.FromUnixTimeMilliseconds(timeStamp) + TimeSpan.FromSeconds(diffInSeconds)).ToUnixTimeMilliseconds();

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
                var postTag = await _commonServices.TagServices.GetTagInfo(PostTagType.Achievement, tagId);
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

    public async Task<(AddMemoryResult Result, long PostId)> TryPostMemory(Session session, AddMemoryTransferData transferData, CancellationToken cancellationToken = default)
    {
        const int maxLength = 2048;
        if (transferData.Comment.Length >= maxLength)
        {
            return (AddMemoryResult.CommentTooLong, 0);
        }

        var dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(transferData.TimeStamp);
        if (dateTimeOffset < DateTimeOffset.FromUnixTimeMilliseconds(946684800) || dateTimeOffset > DateTimeOffset.UtcNow)
        {
            return (AddMemoryResult.InvalidTime, 0);
        }

        var accountViewModel = await TryGetAccount(session, cancellationToken);
        if (accountViewModel == null)
        {
            return (AddMemoryResult.SessionNotFound, 0);
        }

        if (!_commonServices.TagServices.GetCommentText(transferData.Comment, accountViewModel, out var commentText, out var accountsTaggedInComment, out var hashTagsTaggedInComment))
        {
            return (AddMemoryResult.ParseCommentFailed, 0);
        }

        var tagIds = new HashSet<long>();
        var postRecord = new PostRecord
        {
            AccountId = accountViewModel.Id,
            PostAvatar = transferData.AvatarTag,
            PostComment = commentText,
            PostTime = dateTimeOffset,
            PostEditedTime = dateTimeOffset,
            PostCreatedTime = dateTimeOffset,
            PostVisibility = transferData.IsPrivate ? (byte)1 : (byte)0,
        };

        var buildSystemTagsResult = await BuildSystemTagsString(postRecord, accountViewModel, transferData.SystemTags, tagIds);
        if (buildSystemTagsResult != AddMemoryResult.Success)
        {
            return (buildSystemTagsResult, 0);
        }

        var addCommentTagResult = await AddCommentTags(postRecord, accountsTaggedInComment, hashTagsTaggedInComment, tagIds);
        if (addCommentTagResult != AddMemoryResult.Success)
        {
            return (addCommentTagResult, 0);
        }

        var uploadAndSortResult = await UploadAndSortImages(postRecord, transferData.UploadResults);
        if (uploadAndSortResult != AddMemoryResult.Success)
        {
            return (uploadAndSortResult, 0);
        }

        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        postRecord.Id = await database.InsertWithInt64IdentityAsync(postRecord);

        var tagRecords = new List<PostTagRecord>();
        foreach (var tagId in tagIds)
        {
            tagRecords.Add(new PostTagRecord { PostId = postRecord.Id, TagId = tagId, CreatedTime = DateTimeOffset.UtcNow });
        }

        await database.PostTags.BulkCopyAsync(tagRecords);

        return (AddMemoryResult.Failed, 0);
    }

    private async Task<AddMemoryResult> BuildSystemTagsString(PostRecord postRecord, ActiveAccountViewModel accountViewModel, HashSet<string> systemTags, HashSet<long> tagIds)
    {
        if (!string.IsNullOrWhiteSpace(postRecord.PostAvatar) && !systemTags.Contains(postRecord.PostAvatar))
        {
            return AddMemoryResult.InvalidTags;
        }

        var systemTagBuilder = new StringBuilder();
        foreach (var systemTag in systemTags)
        {
            var tagId = await _commonServices.TagServices.IsValidTags(systemTag, accountViewModel);
            if (tagId > 0 && tagIds.Add(tagId))
            {
                systemTagBuilder.Append(systemTag);
                systemTagBuilder.Append("~0|");
            }
            else
            {
                return AddMemoryResult.InvalidTags;
            }
        }

        postRecord.SystemTags = systemTagBuilder.ToString().TrimEnd('|');

        return AddMemoryResult.Success;
    }

    private async Task<AddMemoryResult> AddCommentTags(PostRecord postRecord, HashSet<long> accountsTaggedInComment, HashSet<string> hashTagsTaggedInComment, HashSet<long> tagIds)
    {
        foreach (var accountId in accountsTaggedInComment)
        {
            var tagId = await _commonServices.TagServices.GetTagRecordId(PostTagType.Account, accountId);
            if (tagId > 0)
            {
                tagIds.Add(tagId);
            }
            else
            {
                return AddMemoryResult.InvalidTags;
            }
        }

        foreach (var hashTag in hashTagsTaggedInComment)
        {
            var tagId = await _commonServices.TagServices.GetHashTagRecordId(hashTag);
            if (tagId > 0)
            {
                tagIds.Add(tagId);
            }
            else
            {
                return AddMemoryResult.InvalidTags;
            }
        }

        return AddMemoryResult.Success;
    }

    private async Task<AddMemoryResult> UploadAndSortImages(PostRecord postRecord, List<AddMemoryUploadResult> uploadResults)
    {
        var data = new List<(byte[] Buffer, string Hash, string Name, long TimeStamp, string ImageBlobName)>();
        foreach (var uploadResult in uploadResults)
        {
            if (uploadResult.FileContent.Length > 1024 * 1024 * 10)
            {
                return AddMemoryResult.Failed;
            }

            try
            {
                using var image = Image.Load(uploadResult.FileContent);
                //image.Mutate(x => x.Resize(new ResizeOptions
                //{
                //    Mode = ResizeMode.Max,
                //    Size = new Size(1920, 1080)
                //}));

                await using var memoryStream = new MemoryStream();
                await image.SaveAsJpegAsync(memoryStream);

                var buffer = memoryStream.ToArray();
                var hashData = MD5.HashData(buffer);
                var hashString = GetHashString(hashData);

                data.Add((buffer, hashString, uploadResult.FileName, uploadResult.FileTimeStamp, "TODO"));
            }
            catch (Exception)
            {
                return AddMemoryResult.Failed;
            }
        }

        var imageNameBuilder = new StringBuilder();
        foreach (var uploadResult in data)
        {
            imageNameBuilder.Append(uploadResult.ImageBlobName);
            imageNameBuilder.Append('|');
        }

        postRecord.BlobNames = imageNameBuilder.ToString().TrimEnd('|');

        return AddMemoryResult.Success;
    }

    private string GetHashString(byte[] hashData)
    {
        var output = new StringBuilder(hashData.Length);
        foreach (var b in hashData)
        {
            output.Append(b.ToString("X2"));
        }

        return output.ToString();
    }
}