namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class PostViewModelBlobInfo
{
    [JsonInclude, JsonPropertyName("title")] public string Title { get; set; }
    [JsonInclude, JsonPropertyName("description")] public string Description { get; set; }
    [JsonInclude, JsonPropertyName("src")] public string Source { get; set; }
}