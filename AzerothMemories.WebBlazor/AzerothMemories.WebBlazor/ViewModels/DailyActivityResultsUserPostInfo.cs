namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial record DailyActivityResultsUserPostInfo
{
    [JsonInclude, DataMember, MemoryPackInclude] public required int AccountId { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public required int PostId { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public required long PostTime { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public required long PostCreatedTime { get; init; }
    [JsonInclude, DataMember, MemoryPackInclude] public required PostViewModelBlobInfo[] BlobInfo { get; init; }

    public PostViewModelBlobInfo[] GetBlobPreviewInfo(string description)
    {
        var results = new PostViewModelBlobInfo[BlobInfo.Length];
        for (var i = 0; i < results.Length; i++)
        {
            var blobInfo = BlobInfo[i];
            var newBlobInfo = new PostViewModelBlobInfo
            {
                Title = blobInfo.Title + $": {i + 1}/{BlobInfo.Length}",
                Description = description,
                Source = blobInfo.Source
            };

            results[i] = newBlobInfo;
        }

        return results;
    }
}