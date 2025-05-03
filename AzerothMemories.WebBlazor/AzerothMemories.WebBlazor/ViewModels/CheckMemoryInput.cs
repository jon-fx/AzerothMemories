using Microsoft.AspNetCore.Components.Forms;

namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class CheckMemoryInput
{
    [JsonInclude, DataMember, MemoryPackInclude] public CheckMemoryFlags Flags { get; set; }
    [JsonInclude, DataMember, MemoryPackInclude] public long ScreenShotUnixTime { get; set; }
    [JsonIgnore, MemoryPackIgnore] public IBrowserFile? BrowserFile { get; init; }
}