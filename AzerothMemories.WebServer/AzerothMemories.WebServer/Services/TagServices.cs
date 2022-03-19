namespace AzerothMemories.WebServer.Services;

[RegisterComputeService]
[RegisterAlias(typeof(ITagServices))]
public class TagServices : DbServiceBase<AppDbContext>, ITagServices
{
    private readonly CommonServices _commonServices;

    public TagServices(IServiceProvider services, CommonServices commonServices) : base(services)
    {
        _commonServices = commonServices;
    }

    [ComputeMethod]
    public virtual async Task<bool> IsValidRealmSlug(string realmSlug)
    {
        var allRealmSlugs = await GetAllRealmSlugs().ConfigureAwait(false);
        return allRealmSlugs.Contains(realmSlug);
    }

    [ComputeMethod]
    protected virtual async Task<HashSet<string>> GetAllRealmSlugs()
    {
        await using var database = CreateDbContext();

        var query = from r in database.BlizzardData
                    where r.TagType == PostTagType.Realm
                    select r.Media;

        var resultsSet = new HashSet<string>();
        var queryResults = await query.ToArrayAsync().ConfigureAwait(false);
        foreach (var queryResult in queryResults)
        {
            resultsSet.Add(queryResult);
        }

        return resultsSet;
    }

    [ComputeMethod]
    public virtual async Task<PostTagInfo> GetTagInfo(PostTagType tagType, int tagId, string hashTagText, ServerSideLocale locale)
    {
        if (tagType == PostTagType.Account || tagType == PostTagType.Character || tagType == PostTagType.Guild)
        {
            return await TryGetUserTagInfo(tagType, tagId).ConfigureAwait(false);
        }

        if (tagType == PostTagType.HashTag)
        {
            if (string.IsNullOrWhiteSpace(hashTagText))
            {
                throw new NotImplementedException();
            }

            return new PostTagInfo(tagType, tagId, hashTagText, null);
        }

        await using var database = CreateDbContext();

        var tagString = PostTagInfo.GetTagString(tagType, tagId);
        //var record = await database.BlizzardData.FirstOrDefaultAsync(r => r.TagType == tagType && r.TagId == tagId);
        var record = await database.BlizzardData.FirstOrDefaultAsync(r => r.Key == tagString).ConfigureAwait(false);
        if (record == null)
        {
            return new PostTagInfo(tagType, tagId, tagString, null);
        }

        return CreatePostTagInfo(record, locale);
    }

    [ComputeMethod]
    public virtual async Task<PostTagInfo> TryGetUserTagInfo(PostTagType tagType, int tagId)
    {
        await using var database = CreateDbContext();

        if (tagType == PostTagType.Account)
        {
            var data = await (from r in database.Accounts
                              where r.Id == tagId
                              select new { r.Username, r.Avatar }).FirstOrDefaultAsync().ConfigureAwait(false);

            if (data != null)
            {
                await _commonServices.AccountServices.DependsOnAccountRecord(tagId).ConfigureAwait(false);

                return new PostTagInfo(PostTagType.Account, tagId, data.Username, data.Avatar);
            }
        }

        if (tagType == PostTagType.Character)
        {
            var data = await (from r in database.Characters
                              where r.Id == tagId
                              select new { r.Name, r.AvatarLink, r.Gender, r.Race }).FirstOrDefaultAsync().ConfigureAwait(false);

            if (data != null)
            {
                await _commonServices.CharacterServices.DependsOnCharacterRecord(tagId).ConfigureAwait(false);

                return new PostTagInfo(PostTagType.Character, tagId, data.Name, CharacterViewModel.GetAvatarStringWithFallBack(data.AvatarLink, data.Race, data.Gender));
            }
        }

        if (tagType == PostTagType.Guild)
        {
            var data = await (from r in database.Guilds
                              where r.Id == tagId
                              select new { r.Name }).FirstOrDefaultAsync().ConfigureAwait(false);

            if (data != null)
            {
                await _commonServices.GuildServices.DependsOnGuildRecord(tagId).ConfigureAwait(false);

                return new PostTagInfo(PostTagType.Guild, tagId, data.Name, null);
            }
        }

        return new PostTagInfo(tagType, tagId, PostTagInfo.GetTagString(tagType, tagId), null);
    }

    [ComputeMethod]
    public virtual async Task<PostTagInfo[]> Search(Session session, string searchString, ServerSideLocale locale)
    {
        if (string.IsNullOrWhiteSpace(searchString) || searchString.Length < 3)
        {
            return Array.Empty<PostTagInfo>();
        }

        searchString = DatabaseHelpers.GetSearchableName(searchString);
        if (searchString.Length < 3)
        {
            return Array.Empty<PostTagInfo>();
        }

        return await Search(searchString, locale).ConfigureAwait(false);
    }

    [ComputeMethod]
    protected virtual async Task<PostTagInfo[]> Search(string searchString, ServerSideLocale locale)
    {
        await using var database = CreateDbContext();

        var query = ServerLocaleHelpers.GetSearchQuery(database, locale, searchString);
        var records = await query.ToArrayAsync().ConfigureAwait(false);
        var postTags = new List<PostTagInfo>();
        foreach (var record in records)
        {
            var postTag = CreatePostTagInfo(record, locale);
            postTags.Add(postTag);
        }

        return postTags.OrderBy(x => x.Name.Length).ThenBy(x => x.Type).ThenBy(x => x.Id).Take(50).ToArray();
    }

    private PostTagInfo CreatePostTagInfo(BlizzardDataRecord record, ServerSideLocale locale)
    {
        if (record.TagType == PostTagType.Realm)
        {
            record.Media = null;
        }
        else if (!string.IsNullOrEmpty(record.Media))
        {
            record.Media = $"{ZExtensions.BlobStaticMediaStoragePath}{record.Media}";
        }

        var name = ServerLocaleHelpers.GetName(locale, record.Name);
        if (string.IsNullOrWhiteSpace(name))
        {
            name = PostTagInfo.GetTagString(record.TagType, record.TagId);
        }

        return new PostTagInfo(record.TagType, record.TagId, name, record.Media, record.MinTagTime.ToUnixTimeMilliseconds());
    }

    public async Task<PostTagRecord> TryCreateTagRecord(string systemTag, PostRecord postRecord, AccountViewModel accountViewModel, PostTagKind tagKind)
    {
        if (!ZExtensions.ParseTagInfoFrom(systemTag, out var postTagInfo))
        {
            return null;
        }

        var result = await TryCreateTagRecord(postRecord, postTagInfo.Type, postTagInfo.Id, tagKind).ConfigureAwait(false);
        if (result != null)
        {
            if (postTagInfo.Type == PostTagType.Account && accountViewModel.Id != postTagInfo.Id)
            {
                return null;
            }

            if (postTagInfo.Type == PostTagType.Character && accountViewModel.CharactersArray.FirstOrDefault(x => x.Id == postTagInfo.Id) == null)
            {
                return null;
            }
        }

        return result;
    }

    public async Task<PostTagRecord> TryCreateTagRecord(PostRecord postRecord, PostTagType tagType, int tagId, PostTagKind tagKind)
    {
        switch (tagType)
        {
            case PostTagType.None:
            {
                return null;
            }
            case PostTagType.Type:
            case PostTagType.Main:
            case PostTagType.Region:
            case PostTagType.Realm:
            {
                var results = await IsValidTagIdWithBlizzardDataSanityChecks(tagType, tagId).ConfigureAwait(false);
                if (results.Exists && postRecord.PostTime >= results.MinTagTime)
                {
                    break;
                }

                return null;
            }
            case PostTagType.Account:
            case PostTagType.Character:
            case PostTagType.Guild:
            {
                break;
            }
            case PostTagType.Achievement:
            case PostTagType.Item:
            case PostTagType.Mount:
            case PostTagType.Pet:
            case PostTagType.Zone:
            case PostTagType.Npc:
            case PostTagType.Spell:
            case PostTagType.Object:
            case PostTagType.Quest:
            case PostTagType.ItemSet:
            case PostTagType.Toy:
            case PostTagType.Title:
            case PostTagType.CharacterRace:
            case PostTagType.CharacterClass:
            case PostTagType.CharacterClassSpecialization:
            {
                var results = await IsValidTagIdWithBlizzardDataSanityChecks(tagType, tagId).ConfigureAwait(false);
                if (results.Exists && postRecord.PostTime >= results.MinTagTime)
                {
                    break;
                }

                return null;
            }
            case PostTagType.HashTag:
            {
                return null;
            }
            default:
            {
                return null;
            }
        }

        return new PostTagRecord
        {
            CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            TagId = tagId,
            TagType = tagType,
            TagString = PostTagInfo.GetTagString(tagType, tagId),
            TagKind = tagKind
        };
    }

    public Task<PostTagRecord> GetHashTagRecord(string hashTag, PostTagKind tagKind)
    {
        Exceptions.ThrowIf(!hashTag.StartsWith("HashTag"));

        var result = new PostTagRecord
        {
            CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            TagKind = tagKind,
            TagType = PostTagType.HashTag,
            TagString = hashTag,
        };

        return Task.FromResult(result);
    }

    [ComputeMethod]
    protected virtual async Task<(bool Exists, Instant MinTagTime)> IsValidTagIdWithBlizzardDataSanityChecks(PostTagType tagType, int tagId)
    {
        await using var database = CreateDbContext();

        var tagString = PostTagInfo.GetTagString(tagType, tagId);
        var query = from record in database.BlizzardData
                    where record.Key == tagString
                    select new { record.Id, record.MinTagTime };

        var exists = await query.FirstOrDefaultAsync().ConfigureAwait(false);
        if (exists == null)
        {
            return (false, Instant.FromUnixTimeMilliseconds(0));
        }

        return (true, exists.MinTagTime);
    }
}