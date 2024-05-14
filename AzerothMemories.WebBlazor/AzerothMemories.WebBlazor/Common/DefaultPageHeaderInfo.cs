namespace AzerothMemories.WebBlazor.Common;

public sealed class DefaultPageHeaderInfo : IPageHeaderInfoProvider
{
    private readonly string _pageTitle;

    public DefaultPageHeaderInfo()
    {
        _pageTitle = "Memories of Azeroth";
    }

    public DefaultPageHeaderInfo(string pageTitle)
    {
        _pageTitle = pageTitle;
    }

    public string GetPageTitle()
    {
        if (string.IsNullOrWhiteSpace(_pageTitle))
        {
            return "Memories of Azeroth";
        }

#if DEBUG
        return $"DEBUG - {_pageTitle} - Memories of Azeroth";
#endif

        return $"{_pageTitle} - Memories of Azeroth";
    }

    public string GetPageDescription()
    {
        return "Memories of Azeroth is a site dedicated to organising, storing and sharing your World of Warcraft screenshots.";
    }

    public string GetPageImage()
    {
        return "header-banner.png";
    }

    public string GetPageImageAlt()
    {
        return "Memories of Azeroth header banner";
    }

    public string? GetCanonicalLink()
    {
        return null;
    }
}