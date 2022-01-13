namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class ActiveAccountViewModel : AccountViewModel
{
    public ActiveAccountViewModel()
    {
    }

    public bool CanChangeUsername => true;

    public Dictionary<long, string> GetUserTagList()
    {
        if (FollowersViewModels == null)
        {
            return new Dictionary<long, string>();
        }

        var tagSet = new Dictionary<long, string>();
        foreach (var kvp in FollowersViewModels)
        {
            tagSet.TryAdd(kvp.Value.FollowerId, kvp.Value.FollowerUsername);
        }

        return tagSet;
    }
}