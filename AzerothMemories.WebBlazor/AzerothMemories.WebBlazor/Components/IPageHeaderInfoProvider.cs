namespace AzerothMemories.WebBlazor.Components;

public interface IPageHeaderInfoProvider
{
    string GetPageTitle();

    string GetPageDescription();

    string? GetPageImage();

    string? GetPageImageAlt();

    string? GetCanonicalLink();
}