using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace AzerothMemories.WebBlazor.Services.Markdown;

internal sealed class HashTagLinkRenderer : HtmlObjectRenderer<HashTagLink>
{
    protected override void Write(HtmlRenderer renderer, HashTagLink obj)
    {
        if (renderer.EnableHtmlForInline)
        {
            renderer.Write("<a href='postsearch?tag=128-").Write(obj.HashSlice).Write("'>").Write('#').Write(obj.HashSlice).Write("</a>");
        }
        else
        {
            renderer.Write('#').Write(obj.HashSlice);
        }
    }
}