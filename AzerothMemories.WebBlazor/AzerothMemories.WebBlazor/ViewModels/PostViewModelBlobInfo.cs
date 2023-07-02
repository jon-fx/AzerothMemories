namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial record PostViewModelBlobInfo
{
    [JsonInclude, JsonPropertyName("title"), DataMember, MemoryPackInclude] public string Title { get; init; }
    [JsonInclude, JsonPropertyName("description"), DataMember, MemoryPackInclude] public string Description { get; init; }
    [JsonInclude, JsonPropertyName("src"), DataMember, MemoryPackInclude] public string Source { get; init; }

    public static PostViewModelBlobInfo[] CreateBlobInfo(string username, string comment, string[] blobNames)
    {
        var results = new List<PostViewModelBlobInfo>();
        var title = $"{username}'s memory";
        var description = string.Empty;//comment;
        //if (string.IsNullOrWhiteSpace(comment))
        //{
        //}
        //else if (comment.Length > 50)
        //{
        //    description = $"{comment[..50]}...";
        //}
        //else
        //{
        //    description = comment;
        //}

        foreach (var imageBlobName in blobNames)
        {
            if (string.IsNullOrWhiteSpace(imageBlobName))
            {
                continue;
            }

            var imageSource = $"{ZExtensions.BlobUserUploadsStoragePath}{imageBlobName}";

            results.Add(new PostViewModelBlobInfo { Title = title, Description = description, Source = imageSource });
        }

        return results.ToArray();
    }
}