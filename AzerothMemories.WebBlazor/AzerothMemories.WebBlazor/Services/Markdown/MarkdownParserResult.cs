namespace AzerothMemories.WebBlazor.Services.Markdown;

public sealed class MarkdownParserResult
{
    public readonly MarkdownParserResultCode ResultCode;
    public readonly MarkdownContextHelper ContextHelper;

    public readonly string CommentText;
    public readonly string CommentTextMarkdown;

    public MarkdownParserResult(MarkdownParserResultCode resultCode, MarkdownContextHelper contextHelper, string commentText, string commentTextMarkdown)
    {
        ResultCode = resultCode;
        ContextHelper = contextHelper;

        CommentText = commentText;
        CommentTextMarkdown = commentTextMarkdown;
    }

    public HashSet<int> AccountsTaggedInComment => ContextHelper.AccountsTaggedInComment;

    public HashSet<string> HashTagsTaggedInComment => ContextHelper.HashTagsTaggedInComment;

    public string AccountsTaggedInCommentMap => ContextHelper.AccountsTaggedInCommentMap.ToString().TrimEnd('|');
}