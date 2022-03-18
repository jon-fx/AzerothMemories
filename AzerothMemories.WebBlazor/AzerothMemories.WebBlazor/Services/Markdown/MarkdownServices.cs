using Markdig;
using Markdig.Extensions.AutoLinks;

namespace AzerothMemories.WebBlazor.Services.Markdown;

public sealed class MarkdownServices
{
    private readonly MarkdownPipeline _markdownPipeline;

    public MarkdownServices()
    {
        _markdownPipeline = new MarkdownPipelineBuilder()
            //.UseAbbreviations()
            //.UseAutoIdentifiers()
            //.UseCitations()
            //.UseCustomContainers()
            //.UseDefinitionLists()
            .UseEmphasisExtras()
            //.UseFigures()
            //.UseFooters()
            //.UseFootnotes()
            //.UseGridTables()
            //.UseMathematics()
            //.UseMediaLinks()
            //.UsePipeTables()
            //.UseListExtras()
            //.UseTaskLists()
            //.UseDiagrams()
            .UseAutoLinks(new AutoLinkOptions { OpenInNewWindow = true, UseHttpsForWWWLinks = true })
            //.UseGenericAttributes()
            .UseBootstrap()
            .UseEmojiAndSmiley()
            .Use<MarkDownExtension>()
            .UseSoftlineBreakAsHardlineBreak()
            .DisableHtml()
            .Build();
    }

    public MarkdownParserResult GetCommentText(string commentText, Dictionary<int, string> usersThatCanBeTagged)
    {
        var contextHelper = new MarkdownContextHelper(usersThatCanBeTagged);

        if (string.IsNullOrWhiteSpace(commentText))
        {
            return new MarkdownParserResult(MarkdownParserResultCode.Success, contextHelper, commentText, string.Empty);
        }

        if (commentText.Length > ZExtensions.MaxCommentLength)
        {
            return new MarkdownParserResult(MarkdownParserResultCode.Failed_Length, contextHelper, commentText, string.Empty);
        }

        var context = new MarkdownParserContext();
        context.Properties.Add("ContextHelper", contextHelper);

        var tempCommentText = Markdig.Markdown.ToHtml(commentText, _markdownPipeline, context);
        if (string.IsNullOrWhiteSpace(tempCommentText))
        {
            return new MarkdownParserResult(MarkdownParserResultCode.Success, contextHelper, commentText, string.Empty);
        }

        if (tempCommentText.Length > ZExtensions.MaxCommentLength)
        {
            return new MarkdownParserResult(MarkdownParserResultCode.Failed_Length, contextHelper, commentText, string.Empty);
        }

        return new MarkdownParserResult(MarkdownParserResultCode.Success, contextHelper, commentText, tempCommentText);
    }
}