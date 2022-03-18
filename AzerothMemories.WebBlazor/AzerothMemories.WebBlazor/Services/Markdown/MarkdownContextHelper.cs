using System.Text;

namespace AzerothMemories.WebBlazor.Services.Markdown;

public sealed class MarkdownContextHelper
{
    public readonly Dictionary<int, string> UsersThatCanBeTagged;

    public readonly HashSet<int> AccountsTaggedInComment = new();
    public readonly StringBuilder AccountsTaggedInCommentMap = new();

    public readonly HashSet<string> HashTagsTaggedInComment = new();

    public MarkdownContextHelper(Dictionary<int, string> usersThatCanBeTagged)
    {
        UsersThatCanBeTagged = usersThatCanBeTagged;
    }
}