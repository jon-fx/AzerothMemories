using System.ComponentModel;

namespace AzerothMemories.WebBlazor.Common;

public enum PostReaction
{
    [Description("")] None,
    [Description("👍")] Reaction1,
    [Description("😍")] Reaction2,
    [Description("🤣")] Reaction3,
    [Description("😢")] Reaction4,
    [Description("😲")] Reaction5,
    [Description("👿")] Reaction6,
    [Description("")] Reaction7,
    [Description("")] Reaction8,
    [Description("")] Reaction9
}