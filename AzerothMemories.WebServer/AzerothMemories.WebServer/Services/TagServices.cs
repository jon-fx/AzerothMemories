using System.Globalization;
using System.Text;
using System.Web;

namespace AzerothMemories.WebServer.Services;

[RegisterComputeService]
[RegisterAlias(typeof(ITagServices))]
public class TagServices : ITagServices
{
    private readonly CommonServices _commonServices;

    public TagServices(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    [ComputeMethod]
    public virtual async Task<PostTagInfo> GetTagInfo(PostTagType tagType, long tagId, string locale)
    {
        if (tagType == PostTagType.Account || tagType == PostTagType.Character || tagType == PostTagType.Guild)
        {
            return await TryGetUserTagInfo(tagType, tagId);
        }

        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        var record = await database.BlizzardData.Where(r => r.TagType == tagType && r.TagId == tagId).FirstOrDefaultAsync();
        if (record == null)
        {
            return new PostTagInfo(tagType, tagId, PostTagInfo.GetTagString(tagType, tagId), null);
        }

        return CreatePostTagInfo(record);
    }

    [ComputeMethod]
    public virtual async Task<PostTagInfo> TryGetUserTagInfo(PostTagType tagType, long tagId)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

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
            //var data = await (from r in database.G
            //                  where r.Id == tagId
            //                  select new { r.Username, r.Avatar }).FirstOrDefaultAsync();

            //if (data != null)
            //{
            //    return new PostTagInfo(PostTagType.Account, tagId, data.Username, data.Avatar);
            //}

            throw new NotImplementedException();
        }

        return new PostTagInfo(tagType, tagId, PostTagInfo.GetTagString(tagType, tagId), null);
    }

    [ComputeMethod]
    public virtual async Task<PostTagInfo[]> Search(Session session, string searchString, string locale = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchString) || searchString.Length < 3)
        {
            return Array.Empty<PostTagInfo>();
        }

        locale ??= CultureInfo.CurrentCulture.Name;
        var tempBytes = Encoding.GetEncoding("ISO-8859-8").GetBytes(searchString);
        searchString = Encoding.UTF8.GetString(tempBytes);
        searchString = searchString.Trim().ToLower();
        if (searchString.Length < 3)
        {
            return Array.Empty<PostTagInfo>();
        }

        return await Search(searchString, locale);
    }

    [ComputeMethod]
    protected virtual async Task<PostTagInfo[]> Search(string searchString, string locale)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        var query = GetSearchQuery(database, locale, searchString);
        var records = await query.ToArrayAsync();
        var postTags = new List<PostTagInfo>();
        foreach (var record in records)
        {
            var postTag = CreatePostTagInfo(record);
            postTags.Add(postTag);
        }

        return postTags.ToArray();
    }

    private IQueryable<BlizzardDataRecord> GetSearchQuery(DatabaseConnection database, string locale, string searchString)
    {
        return database.BlizzardData.Where(r => Sql.Lower(r.Name.En_Gb).StartsWith(searchString)).OrderBy(r => r.TagType).ThenBy(r => Sql.Length(r.Name.En_Gb)).Take(50);
    }

    private PostTagInfo CreatePostTagInfo(BlizzardDataRecord record)
    {
        if (record.TagType == PostTagType.Realm)
        {
            record.Media = null;
        }

        return new PostTagInfo(record.TagType, record.TagId, record.Name.En_Gb, record.Media);
    }

    public async Task<PostTagRecord> TryCreateTagRecord(string systemTag, ActiveAccountViewModel accountViewModel)
    {
        if (!ZExtensions.ParseTagInfoFrom(systemTag, out var postTagInfo))
        {
            return null;
        }

        var result = await TryCreateTagRecord(postTagInfo.Type, postTagInfo.Id);
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

    public async Task<PostTagRecord> TryCreateTagRecord(PostTagType tagType, long tagId)
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
            TagString = PostTagInfo.GetTagString(tagType, tagId)
        };
    }

    public Task<PostTagRecord> GetHashTagRecord(string hashTag)
    {
        var result = new PostTagRecord
        {
            CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            TagKind = PostTagKind.Comment,
            TagType = PostTagType.HashTag,
            TagString = hashTag,
        };

        return Task.FromResult(result);
    }

    [ComputeMethod]
    protected virtual async Task<bool> IsValidTagIdWithBlizzardDataSanityChecks(PostTagType tagType, long tagId)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        var tagString = await (from data in database.BlizzardData
                               where data.TagType == tagType && data.TagId == tagId
                               select data.Key).FirstOrDefaultAsync();

        return tagString != null;
    }

    public bool GetCommentText(string commentText, ActiveAccountViewModel accountViewModel, out string newCommentText, out HashSet<long> userTags, out HashSet<string> hashTags)
    {
        const int maxLength = 2048;

        userTags = new HashSet<long>();
        hashTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        newCommentText = string.Empty;

        if (string.IsNullOrWhiteSpace(commentText))
        {
            return true;
        }

        if (commentText.Length > maxLength)
        {
            return false;
        }

        var encodedCommentText = HttpUtility.HtmlEncode(commentText);
        if (string.IsNullOrWhiteSpace(encodedCommentText))
        {
            return false;
        }

        if (commentText.Length > maxLength)
        {
            return false;
        }

        var commentTextBuilder = new StringBuilder(encodedCommentText.Replace("\n", "<br>").Replace("\r", "<br>"));
        var commentUserTags = ParseTagsFrom(commentTextBuilder, '@');
        for (var i = commentUserTags.Count - 1; i >= 0; i--)
        {
            var (offset, str) = commentUserTags[i];
            if (str.Length > 64)
            {
                return false;
            }

            var tagInfo = accountViewModel.UserTags.FirstOrDefault(x => string.Equals(x.Value, str, StringComparison.OrdinalIgnoreCase));
            if (tagInfo.Key > 0 && tagInfo.Value != null)
            {
                userTags.Add(tagInfo.Key);
                commentTextBuilder.Remove(offset, str.Length + 1);
                commentTextBuilder.Insert(offset, $"<a href='account/{tagInfo.Key}'>@{tagInfo.Value}</a>");

                if (commentTextBuilder.Length > maxLength)
                {
                    return false;
                }
            }
        }

        var commentHashTags = ParseTagsFrom(commentTextBuilder, '#');
        for (var i = commentHashTags.Count - 1; i >= 0; i--)
        {
            var (offset, str) = commentHashTags[i];
            if (str.Length > 64)
            {
                return false;
            }

            commentTextBuilder.Remove(offset, str.Length + 1);
            commentTextBuilder.Insert(offset, $"<a href='postsearch?tag=128-{str}'>#{str}</a>");

            if (commentTextBuilder.Length > maxLength)
            {
                return false;
            }

            hashTags.Add($"HashTag-{str}");
        }

        newCommentText = commentTextBuilder.ToString();

        return true;
    }

    private static List<(int, string)> ParseTagsFrom(StringBuilder commentText, char tagPrefix)
    {
        var results = new List<(int, string)>();
        if (commentText.Length == 0)
        {
            return results;
        }

        char? blockStart = null;
        var blockStartIndex = 0;
        var blockText = string.Empty;

        void TryAddToList()
        {
            if (blockStart == null)
            {
                return;
            }

            if (blockText.Contains(new string(tagPrefix, 2)))
            {
                throw new NotImplementedException();
            }

            //if (blockText.Length > 75)
            //{
            //    throw new NotImplementedException();
            //}

            if (blockText.Length > 1)
            {
                if (blockStart == tagPrefix)
                {
                    results.Add((blockStartIndex, blockText));
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            blockStart = null;
            blockStartIndex = 0;
            blockText = string.Empty;
        }

        for (var i = 0; i < commentText.Length; i++)
        {
            if (commentText[i] == tagPrefix)
            {
                TryAddToList();

                blockStartIndex = i;
                blockStart = commentText[i];
                blockText = string.Empty;
            }
            else if (commentText[i] == '<')
            {
                TryAddToList();
            }
            else if (char.IsWhiteSpace(commentText[i]))
            {
                TryAddToList();
            }
            else if (blockStart != null && char.IsLetterOrDigit(commentText[i]))
            {
                blockText += commentText[i];
            }
        }

        TryAddToList();

        return results;
    }
}