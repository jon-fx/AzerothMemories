﻿using System.Text;
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
    public virtual async Task<PostTagInfo> GetTagInfo(PostTagType tagType, long tagId, string hashTagText, ServerSideLocale locale)
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
    public virtual async Task<PostTagInfo> TryGetUserTagInfo(PostTagType tagType, long tagId)
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
            record.Media = $"{ZExtensions.BlobMediaStoragePath}{record.Media}";
        }

        var name = ServerLocaleHelpers.GetName(locale, record.Name);
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

        var result = await TryCreateTagRecord(postTagInfo.Type, postTagInfo.Id, tagKind).ConfigureAwait(false);
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
                if (await IsValidTagIdWithBlizzardDataSanityChecks(tagType, tagId).ConfigureAwait(false))
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
                if (await IsValidTagIdWithBlizzardDataSanityChecks(tagType, tagId).ConfigureAwait(false))
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

        var tagString = PostTagInfo.GetTagString(tagType, tagId);
        var exists = await database.BlizzardData.AnyAsync(r => r.Key == tagString).ConfigureAwait(false);

        return exists;
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
            else if (blockStart != null && (char.IsLetterOrDigit(commentText[i]) || commentText[i] == '-'))
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