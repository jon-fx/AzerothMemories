namespace AzerothMemories.WebBlazor.Common;

[Flags]
public enum AccountFlags
{
    None,
    AlphaUser = 1 << 0,
    BetaUser = 1 << 1,
    SecondAvatarIndex = 1 << 8,
}