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

        if (record.TagType == PostTagType.Realm)
        {
            record.Media = null;
        }

        return new PostTagInfo(record.TagType, record.TagId, record.Name.En_Gb, record.Media);
    }
}