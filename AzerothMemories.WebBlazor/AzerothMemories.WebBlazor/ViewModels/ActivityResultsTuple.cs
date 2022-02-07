namespace AzerothMemories.WebBlazor.ViewModels;

public record ActivityResultsTuple(PostTagInfo PostTagInfo, int Counter)
{
    public ActivityResultsTuple() : this(null, 0)
    {
    }
}