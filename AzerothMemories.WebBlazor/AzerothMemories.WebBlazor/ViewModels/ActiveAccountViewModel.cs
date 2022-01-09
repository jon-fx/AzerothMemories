namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class ActiveAccountViewModel : AccountViewModel
{
    public ActiveAccountViewModel()
    {
    }

    public bool CanChangeUsername => true;

    [JsonIgnore]
    public Dictionary<long, string> UserTags =>
        new()
        {
            { Id, Username },
            { 200, "Bob" },
            { 300, "Bill" },
            { 400, "Ben" },
            { 500, "Tests" },
        };
}