namespace AzerothMemories.WebBlazor.Shared;

internal static class MainTheme
{
    public static MudTheme Theme { get; } = new()
    {
        Palette = new PaletteDark(),
        LayoutProperties = new LayoutProperties
        {
            AppbarHeight = "70px"
        }
    };
}