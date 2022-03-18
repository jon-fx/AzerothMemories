using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace AzerothMemories.WebBlazor.Services.Markdown;

internal sealed class AccountLinkRenderer : HtmlObjectRenderer<AccountLink>
{
    protected override void Write(HtmlRenderer renderer, AccountLink obj)
    {
        if (renderer.EnableHtmlForInline)
        {
            renderer.Write($"<a href='account/{obj.AccountId}'>").Write('@').Write(obj.AccountUsername).Write("</a>");
        }
        else
        {
            renderer.Write('@').Write(obj.AccountUsername);
        }
    }
}