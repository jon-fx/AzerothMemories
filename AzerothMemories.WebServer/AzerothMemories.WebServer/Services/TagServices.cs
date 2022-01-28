using System.Text;
using System.Web;

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
    public virtual async Task<int> TryGetRealmId(string realmSlug)
    {
        await using var database = CreateDbContext();

        var query = from r in database.BlizzardData
                    where r.TagType == PostTagType.Realm && r.Media == realmSlug
                    select r.TagId;

        return (int)await query.FirstOrDefaultAsync();
    }

    [ComputeMethod]
    public virtual async Task<PostTagInfo> GetTagInfo(PostTagType tagType, long tagId, string hashTagText, string locale)
    {
        if (tagType == PostTagType.Account || tagType == PostTagType.Character || tagType == PostTagType.Guild)
        {
            return await TryGetUserTagInfo(tagType, tagId);
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

        var record = await database.BlizzardData.FirstOrDefaultAsync(r => r.TagType == tagType && r.TagId == tagId);
        if (record == null)
        {
            return new PostTagInfo(tagType, tagId, PostTagInfo.GetTagString(tagType, tagId), null);
        }

        return CreatePostTagInfo(record, locale);
    }

    [ComputeMethod]
    public virtual async Task<PostTagInfo> TryGetUserTagInfo(PostTagType tagType, long tagId)
    {
        await using var database = CreateDbContext();

        if (tagType == PostTagType.Account)
        {
            var data = await (from r in database.Accounts
                              where r.Id == tagId
                              select new { r.Username, r.Avatar }).FirstOrDefaultAsync();

            if (data != null)
            {
                return new PostTagInfo(PostTagType.Account, tagId, data.Username, data.Avatar);
            }
        }

        if (tagType == PostTagType.Character)
        {
            var data = await (from r in database.Characters
                              where r.Id == tagId
                              select new { r.Name, r.AvatarLink }).FirstOrDefaultAsync();

            if (data != null)
            {
                return new PostTagInfo(PostTagType.Character, tagId, data.Name, data.AvatarLink);
            }
        }

        if (tagType == PostTagType.Guild)
        {
            var data = await (from r in database.Guilds
                              where r.Id == tagId
                              select new { r.Name }).FirstOrDefaultAsync();

            if (data != null)
            {
                return new PostTagInfo(PostTagType.Guild, tagId, data.Name, null);
            }
        }

        return new PostTagInfo(tagType, tagId, PostTagInfo.GetTagString(tagType, tagId), null);
    }

    [ComputeMethod]
    public virtual async Task<PostTagInfo[]> Search(Session session, string searchString, string locale)
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

        return await Search(searchString, locale);
    }

    [ComputeMethod]
    protected virtual async Task<PostTagInfo[]> Search(string searchString, string locale)
    {
        await using var database = CreateDbContext();

        var query = GetSearchQuery(database, locale, searchString);
        var records = await query.ToArrayAsync();
        var postTags = new List<PostTagInfo>();
        foreach (var record in records)
        {
            var postTag = CreatePostTagInfo(record, locale);
            postTags.Add(postTag);
        }

        return postTags.ToArray();
    }

    private IQueryable<BlizzardDataRecord> GetSearchQuery(AppDbContext database, string locale, string searchString)
    {
        return database.BlizzardData.Where(r => r.Name_EnGb.ToLower().StartsWith(searchString)).OrderBy(r => r.TagType).ThenBy(r => r.Name_EnGb.Length).ThenBy(r => r.TagId).Take(50);
    }

    private PostTagInfo CreatePostTagInfo(BlizzardDataRecord record, string locale)
    {
        if (record.TagType == PostTagType.Realm)
        {
            record.Media = null;
        }

        var name = record.Name_EnGb;
        if (string.IsNullOrWhiteSpace(name))
        {
            name = PostTagInfo.GetTagString(record.TagType, record.TagId);
        }

        return new PostTagInfo(record.TagType, record.TagId, name, record.Media);
    }

    public async Task<PostTagRecord> TryCreateTagRecord(string systemTag, AccountViewModel accountViewModel, PostTagKind tagKind)
    {
        if (!ZExtensions.ParseTagInfoFrom(systemTag, out var postTagInfo))
        {
            return null;
        }

        var result = await TryCreateTagRecord(postTagInfo.Type, postTagInfo.Id, tagKind);
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

    public async Task<PostTagRecord> TryCreateTagRecord(PostTagType tagType, long tagId, PostTagKind tagKind)
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
                if (await IsValidTagIdWithBlizzardDataSanityChecks(tagType, tagId))
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
                if (await IsValidTagIdWithBlizzardDataSanityChecks(tagType, tagId))
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
    protected virtual async Task<bool> IsValidTagIdWithBlizzardDataSanityChecks(PostTagType tagType, long tagId)
    {
        await using var database = CreateDbContext();

        var tagString = await (from data in database.BlizzardData
                               where data.TagType == tagType && data.TagId == tagId
                               select data.Key).FirstOrDefaultAsync();

        return tagString != null;
    }

    public bool GetCommentText(string commentText, Dictionary<long, string> userThatCanBeTagged, out string newCommentText, out HashSet<long> userTags, out HashSet<string> hashTags)
    {
        userTags = new HashSet<long>();
        hashTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        newCommentText = string.Empty;

        if (string.IsNullOrWhiteSpace(commentText))
        {
            return true;
        }

        if (commentText.Length > ZExtensions.MaxCommentLength)
        {
            return false;
        }

        var encodedCommentText = HttpUtility.HtmlAttributeEncode(commentText);
        if (string.IsNullOrWhiteSpace(encodedCommentText))
        {
            return false;
        }

        if (commentText.Length > ZExtensions.MaxCommentLength)
        {
            return false;
        }

        var commentTextBuilder = new StringBuilder(encodedCommentText.Replace("\n", "<br>").Replace("\r", "<br>"));
        var commentUserTagsResult = ParseTagsFrom2(commentTextBuilder, '@');
        if (!commentUserTagsResult.Success)
        {
            return false;
        }

        for (var i = commentUserTagsResult.Tags.Count - 1; i >= 0; i--)
        {
            var (offset, str) = commentUserTagsResult.Tags[i];
            if (str.Length > 64)
            {
                return false;
            }

            var tagInfo = userThatCanBeTagged.FirstOrDefault(x => string.Equals(x.Value, str, StringComparison.OrdinalIgnoreCase));
            if (tagInfo.Key > 0 && tagInfo.Value != null)
            {
                userTags.Add(tagInfo.Key);
                commentTextBuilder.Remove(offset, str.Length + 1);
                commentTextBuilder.Insert(offset, $"<a href='account/{tagInfo.Key}'>@{tagInfo.Value}</a>");

                if (commentTextBuilder.Length > ZExtensions.MaxCommentLength)
                {
                    return false;
                }
            }
        }

        var commentHashTagsResult = ParseTagsFrom2(commentTextBuilder, '#');
        if (!commentHashTagsResult.Success)
        {
            return false;
        }

        for (var i = commentHashTagsResult.Tags.Count - 1; i >= 0; i--)
        {
            var (offset, str) = commentHashTagsResult.Tags[i];
            if (str.Length > 64)
            {
                return false;
            }

            commentTextBuilder.Remove(offset, str.Length + 1);
            commentTextBuilder.Insert(offset, $"<a href='postsearch?tag=128-{str}'>#{str}</a>");

            if (commentTextBuilder.Length > ZExtensions.MaxCommentLength)
            {
                return false;
            }

            hashTags.Add($"HashTag-{str}");
        }

        newCommentText = commentTextBuilder.ToString();

        return true;
    }

    private static (bool Success, List<(int Index, string Text)> Tags) ParseTagsFrom2(StringBuilder commentText, char tagPrefix)
    {
        var results = new List<(int, string)>();
        if (commentText.Length == 0)
        {
            return (false, results);
        }

        char? blockStart = null;
        var blockStartIndex = 0;
        var blockText = string.Empty;

        bool TryAddToList()
        {
            if (blockStart == null)
            {
                return true;
            }

            if (blockText.Contains(new string(tagPrefix, 2)))
            {
                return false;
            }

            if (blockText.Length > 75)
            {
                return false;
            }

            if (blockText.Length > 1)
            {
                if (blockStart == tagPrefix)
                {
                    results.Add((blockStartIndex, blockText));
                }
                else
                {
                    return false;
                }
            }

            blockStart = null;
            blockStartIndex = 0;
            blockText = string.Empty;

            return true;
        }

        for (var i = 0; i < commentText.Length; i++)
        {
            if (commentText[i] == tagPrefix)
            {
                if (!TryAddToList())
                {
                    return (false, results);
                }

                blockStartIndex = i;
                blockStart = commentText[i];
                blockText = string.Empty;
            }
            else if (commentText[i] == '<')
            {
                if (!TryAddToList())
                {
                    return (false, results);
                }
            }
            else if (char.IsWhiteSpace(commentText[i]))
            {
                if (!TryAddToList())
                {
                    return (false, results);
                }
            }
            else if (blockStart != null && char.IsLetterOrDigit(commentText[i]))
            {
                blockText += commentText[i];
            }
        }

        if (!TryAddToList())
        {
            return (false, results);
        }

        return (true, results);
    }
}