namespace AzerothMemories.WebBlazor.ViewModels;

[Flags]
public enum CheckMemoryFlags
{
    None,
    SupportedFileExtension = 1 << 0,
    ValidTimeFromFileName = 1 << 1
}