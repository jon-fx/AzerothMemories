namespace AzerothMemories.WebBlazor.Common;

[Flags]
public enum AccountFlags
{
    None,

    //Account = 1 << 0,
    //Character = 1 << 1,
    SecondAvatarIndex = 1 << 8,
}