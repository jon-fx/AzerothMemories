using Markdig;

namespace AzerothMemories.WebBlazor.Services.Markdown;

internal static class MarkDigExtensions
{
    public static MarkdownContextHelper TryGetContextHelper(this MarkdownParserContext parserContext)
    {
        if (parserContext == null)
        {
            return null;
        }

        if (!parserContext.Properties.TryGetValue("ContextHelper", out var helperObj))
        {
            return null;
        }

        if (helperObj is MarkdownContextHelper helper)
        {
            return helper;
        }

        return null;
    }
}