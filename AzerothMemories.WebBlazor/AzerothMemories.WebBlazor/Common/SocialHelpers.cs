namespace AzerothMemories.WebBlazor.Common;

public sealed class SocialHelpers
{
    public static readonly SocialHelpers[] All;
    public static readonly string BlizzardIconLink = "img/battlenet-icon.webp";

    static SocialHelpers()
    {
        All = new SocialHelpers[(int)SocialLinks.Count];

        _ = new SocialHelpers(SocialLinks.Discord, "Discord Profile", "img/discord-icon.webp")
        {
            ValidatorFunc = BasicValidator
        };

        _ = new SocialHelpers(SocialLinks.Twitter, "Twitter Profile", "img/twitter-icon.webp")
        {
            LinkPrefix = "https://twitter.com/",
            ValidatorFunc = BasicValidator
        };
        _ = new SocialHelpers(SocialLinks.Twitch, "Twitch Channel", "img/twitch-icon.webp")
        {
            LinkPrefix = "https://www.twitch.tv/",
            ValidatorFunc = BasicValidator
        };
        _ = new SocialHelpers(SocialLinks.YouTube, "YouTube Channel", "img/youtube-icon.webp")
        {
            LinkPrefix = "https://www.youtube.com/channel/",
            ValidatorFunc = BasicValidator
        };

        Exceptions.ThrowIf(All.Any(x => x == null));
    }

    private static bool BasicValidator(string arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
        {
            return false;
        }

        if (arg.Length is < 3 or > 100)
        {
            return false;
        }

        foreach (var c in arg)
        {
            if (char.IsLetterOrDigit(c))
            {
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    public readonly string Name;
    public readonly SocialLinks SocialLink;
    public readonly string SocialIconLink;

    private SocialHelpers(SocialLinks socialLink, string name, string icon)
    {
        Name = name;
        SocialLink = socialLink;
        SocialIconLink = icon;

        All[LinkId] = this;
    }

    public int LinkId => (int)SocialLink;

    public string LinkPrefix { get; private init; }

    public Func<string, bool> ValidatorFunc { get; private init; }
}