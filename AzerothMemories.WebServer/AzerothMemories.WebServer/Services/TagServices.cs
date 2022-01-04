using System.Globalization;
using System.Text;

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
    public virtual async Task<PostTagInfo> GetTagInfo(PostTagType tagType, int tagId)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        var record = await database.BlizzardData.Where(r => r.TagType == tagType && r.TagId == tagId).FirstOrDefaultAsync();
        if (record == null)
        {
            return new PostTagInfo(tagType, tagId, $"{tagType}-{tagId}", null);
        }

        return CreatePostTagInfo(record);
    }

    [ComputeMethod]
    public virtual async Task<PostTagInfo[]> Search(Session session, string searchString, string locale = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchString) || searchString.Length < 3)
        {
            return Array.Empty<PostTagInfo>();
        }

        var cultureInfo = locale == null ? CultureInfo.CurrentCulture : new CultureInfo(locale);
        var tempBytes = Encoding.GetEncoding("ISO-8859-8").GetBytes(searchString);
        searchString = Encoding.UTF8.GetString(tempBytes);
        searchString = searchString.Trim().ToLower();
        if (searchString.Length < 3)
        {
            return Array.Empty<PostTagInfo>();
        }

        return await Search(searchString, cultureInfo);
    }

    [ComputeMethod]
    protected virtual async Task<PostTagInfo[]> Search(string searchString, CultureInfo cultureInfo)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        var query = GetSearchQuery(database, cultureInfo, searchString);
        var records = await query.ToArrayAsync();
        var postTags = new List<PostTagInfo>();
        foreach (var record in records)
        {
            var postTag = CreatePostTagInfo(record);
            postTags.Add(postTag);
        }

        return postTags.ToArray();
    }

    private IQueryable<BlizzardDataRecord> GetSearchQuery(DatabaseConnection database, CultureInfo cultureInfo, string searchString)
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
}