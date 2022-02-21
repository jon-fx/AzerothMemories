using Microsoft.JSInterop;

namespace AzerothMemories.WebBlazor.Services;

public sealed class CookieHelper
{
    private string _token;

    public async Task Initialize(IJSRuntime jsRuntime)
    {
        if (string.IsNullOrWhiteSpace(_token))
        {
            _token = await jsRuntime.InvokeAsync<string>("GetAntiForgeryToken");
        }
    }

    public string GetAntiForgeryToken()
    {
        return _token;
    }
}