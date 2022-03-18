using Markdig.Helpers;
using Markdig.Parsers;

namespace AzerothMemories.WebBlazor.Services.Markdown;

internal sealed class AccountLinkInlineParser : InlineParser
{
    public AccountLinkInlineParser()
    {
        OpeningCharacters = new[] { '@' };
    }

    public override bool Match(InlineProcessor processor, ref StringSlice slice)
    {
        var contextHelper = processor.Context.TryGetContextHelper();
        if (contextHelper == null)
        {
            return false;
        }

        var current = slice.CurrentChar;
        if (current != '@' || !slice.PeekChar().IsAlpha())
        {
            return false;
        }

        current = slice.NextChar();
        var start = slice.Start;
        var end = start;

        while (current.IsAlphaNumeric() || current == '-')
        {
            end = slice.Start;
            current = slice.NextChar();
        }

        var accountSlice = new StringSlice(slice.Text, start, end);
        var accountString = accountSlice.ToString();
        var inlineStart = processor.GetSourcePosition(slice.Start, out var line, out var column);
        var link = new AccountLink
        {
            Span =
            {
                Start = inlineStart,
                End = inlineStart + (end - start) + 1
            },
            Line = line,
            Column = column,
            AccountSlice = accountSlice
        };

        var tagInfo = contextHelper.UsersThatCanBeTagged.FirstOrDefault(x => string.Equals(x.Value, accountString, StringComparison.OrdinalIgnoreCase));
        if (tagInfo.Key == 0 || tagInfo.Value == null)
        {
            return false;
        }

        link.AccountId = tagInfo.Key;
        link.AccountUsername = tagInfo.Value;
        processor.Inline = link;

        contextHelper.AccountsTaggedInComment.Add(tagInfo.Key);
        contextHelper.AccountsTaggedInCommentMap.Append($"{tagInfo.Key}-{tagInfo.Value}|");

        return true;
    }
}