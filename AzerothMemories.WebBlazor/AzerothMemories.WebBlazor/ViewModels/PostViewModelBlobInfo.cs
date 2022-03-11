namespace AzerothMemories.WebBlazor.ViewModels;

public sealed record PostViewModelBlobInfo
{
    [JsonInclude, JsonPropertyName("title")] public string Title { get; init; }
    [JsonInclude, JsonPropertyName("description")] public string Description { get; init; }
    [JsonInclude, JsonPropertyName("src")] public string Source { get; init; }

    public static PostViewModelBlobInfo[] CreateBlobInfo(string username, string comment, string[] blobNames)
    {
        var results = new List<PostViewModelBlobInfo>();
        var title = $"{username}'s memory";
        var description = "false";
        if (string.IsNullOrWhiteSpace(comment))
        {
        }
        else if (comment.Length > 50)
        {
            description = $"{comment[..50]}...";
        }
        else
        {
            description = comment;
        }

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