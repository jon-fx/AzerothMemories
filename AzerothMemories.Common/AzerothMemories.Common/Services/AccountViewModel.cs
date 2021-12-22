using System.Text.Json.Serialization;

namespace AzerothMemories.Services
{
    public sealed class AccountViewModel
    {
        [JsonInclude] public long Id;

        [JsonInclude] public string Ref;

        [JsonInclude] public string Username;
    }
}