namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class ActiveAccountViewModel : AccountViewModel
{
    public ActiveAccountViewModel()
    {
        UserTags = new Dictionary<long, string>
        {
            {1, "Lightfx"},
            {2, "Bob"},
            {3, "Bill"},
            {4, "Ben"},
            {5, "Tests"},
        };
    }

    public bool CanChangeUsername => true;

    public Dictionary<long, string> UserTags { get; set; }
}