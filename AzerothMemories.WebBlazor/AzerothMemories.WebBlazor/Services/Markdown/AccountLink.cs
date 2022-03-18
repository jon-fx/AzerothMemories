using Markdig.Helpers;
using Markdig.Syntax.Inlines;

namespace AzerothMemories.WebBlazor.Services.Markdown;

internal sealed class AccountLink : LeafInline
{
    public int AccountId { get; set; }

    public string AccountUsername { get; set; }

    public StringSlice AccountSlice { get; set; }
}