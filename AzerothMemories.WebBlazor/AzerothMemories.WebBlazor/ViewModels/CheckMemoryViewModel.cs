using Microsoft.AspNetCore.Components.Forms;

namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class CheckMemoryViewModel
{
    public required CheckMemoryFlags Flags { get; set; }
    public required long ScreenShotUnixTime { get; set; }
    public required CheckMemoryPostInfo[] CurrentPosts { get; set; }
    public required PostTagInfo[] Achievements { get; set; }
    public required int MatchingAchievements { get; set; }
    public required IBrowserFile BrowserFile { get; init; }
    public PostViewModelBlobInfo? BlobInfo { get; set; }
    public Color BlobIconColor { get; set; } = Color.Default;

    public string GetTimeString(ClientServices clientServices)
    {
        if (ScreenShotUnixTime > 0)
        {
            return clientServices.TimeProvider.GetTimeAsLocalString(ScreenShotUnixTime);
        }

        return "Unknown";
    }

    public string GetDisplayString(ClientServices clientServices)
    {
        var timeString = GetTimeString(clientServices);
        return $"{BrowserFile.Name} - {timeString}";
    }
}