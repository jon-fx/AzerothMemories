using Markdig.Helpers;
using Markdig.Parsers;

namespace AzerothMemories.WebBlazor.Services.Markdown;

internal sealed class HashTagLinkInlineParser : InlineParser
{
    public HashTagLinkInlineParser()
    {
        OpeningCharacters = new[] { '#' };
    }

    public override bool Match(InlineProcessor processor, ref StringSlice slice)
    {
        var contextHelper = processor.Context.TryGetContextHelper();
        if (contextHelper == null)
        {
            return false;
        }

        var current = slice.CurrentChar;
        if (current != '#' || !slice.PeekChar().IsAlpha())
        {
            return false;
        }

        current = slice.NextChar();
        var start = slice.Start;
        var end = start;

        while (current.IsAlphaNumeric())
        {
            end = slice.Start;
            current = slice.NextChar();
        }

        var hashSlice = new StringSlice(slice.Text, start, end);
        var hashSliceString = hashSlice.ToString();
        if (hashSliceString.Length < 2 || hashSliceString.Length > 50)
        {
            return false;
        }

        var inlineStart = processor.GetSourcePosition(slice.Start, out var line, out var column);
        var link = new HashTagLink
        {
            Span =
            {
                Start = inlineStart,
                End = inlineStart + (end - start) + 1
            },
            Line = line,
            Column = column,
            HashSlice = hashSlice
        };

        processor.Inline = link;
        contextHelper.HashTagsTaggedInComment.Add(hashSliceString);

        return true;
    }
}