namespace AzerothMemories.WebBlazor.Services;

public sealed class TokenDelegatingHandler : DelegatingHandler
{
    public static string HeaderName = "X-XSRF-TOKEN";

    private readonly CookieHelper _cookieHelper;

    public TokenDelegatingHandler(CookieHelper cookieHelper)
    {
        _cookieHelper = cookieHelper;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _cookieHelper.GetAntiForgeryToken();
        if (string.IsNullOrWhiteSpace(token))
        {
            
        }
        else
        {
            request.Headers.Add(HeaderName , token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}