using Markdig.Helpers;
using Markdig.Syntax.Inlines;

namespace AzerothMemories.WebBlazor.Services.Markdown;

internal sealed class HashTagLink : LeafInline
{
    public StringSlice HashSlice { get; set; }
}