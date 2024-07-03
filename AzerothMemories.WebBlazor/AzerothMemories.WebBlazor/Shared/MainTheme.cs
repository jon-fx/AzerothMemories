namespace AzerothMemories.WebBlazor.Shared;

internal static class MainTheme
{
    public static MudTheme Theme { get; } = new()
    {
        PaletteDark = new PaletteDark(),
        PaletteLight = new PaletteLight(),
        LayoutProperties = new LayoutProperties
        {
            AppbarHeight = "70px"
        }
    };
}