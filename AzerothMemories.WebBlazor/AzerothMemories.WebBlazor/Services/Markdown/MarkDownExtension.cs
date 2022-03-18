using Markdig;
using Markdig.Renderers;

namespace AzerothMemories.WebBlazor.Services.Markdown;

internal sealed class MarkDownExtension : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        pipeline.InlineParsers.AddIfNotAlready<AccountLinkInlineParser>();
        pipeline.InlineParsers.AddIfNotAlready<HashTagLinkInlineParser>();
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        var htmlRenderer = renderer as HtmlRenderer;
        var renderers = htmlRenderer?.ObjectRenderers;
        if (renderers != null)
        {
            renderers.AddIfNotAlready<AccountLinkRenderer>();
            renderers.AddIfNotAlready<HashTagLinkRenderer>();
        }
    }
}