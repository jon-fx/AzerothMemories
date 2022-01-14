namespace AzerothMemories.WebBlazor.ViewModels;

[Flags]
public enum MainSearchType
{
    None,
    Account = 1 << 0,
    Character = 1 << 1,
    Guild = 1 << 2
}