using Markdig;
using Markdig.Extensions.AutoLinks;

namespace AzerothMemories.WebBlazor.Services.Markdown;

public sealed class MarkdownServices
{
    private readonly MarkdownPipeline _postMarkdownPipeline;
    private readonly MarkdownPipeline _commentMarkdownPipeline;

    public MarkdownServices()
    {
        MarkdownPipelineBuilder GetCommonSetup()
        {
            return new MarkdownPipelineBuilder()
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
                .DisableHtml();
        }

        _postMarkdownPipeline = GetCommonSetup()
                               .Build();

        _commentMarkdownPipeline = GetCommonSetup()
                                  .DisableHeadings()
                                  .Build();
    }

    public MarkdownParserResult GetCommentText(string commentText, Dictionary<int, string> usersThatCanBeTagged, bool isMainPost)
    {
        if (isMainPost)
        {
            return GetCommentText(_postMarkdownPipeline, commentText, usersThatCanBeTagged);
        }

        return GetCommentText(_commentMarkdownPipeline, commentText, usersThatCanBeTagged);
    }

    private MarkdownParserResult GetCommentText(MarkdownPipeline markdownPipeline, string commentText, Dictionary<int, string> usersThatCanBeTagged)
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

        var tempCommentText = Markdig.Markdown.ToHtml(commentText, markdownPipeline, context);
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