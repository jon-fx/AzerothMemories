using Azure;
using System.Xml.Linq;

namespace AzerothMemories.WebServer.Services;

internal static class SiteMapHelper
{
    public static async Task<MediaResult> BuildSiteMap(string baseUrl, string name, string subName, List<(string Url, DateTime LastModified)> maps)
    {
        XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var sitemap = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement(xmlns + name));

        if (sitemap.Root == null)
        {
        }
        else
        {
            foreach (var (url, lastModified) in maps)
            {
                sitemap.Root.Add(new XElement(xmlns + subName, new XElement(xmlns + "loc", baseUrl + url), new XElement(xmlns + "lastmod", lastModified.ToString("yyyy-MM-ddTHH:mm:sszzz"))));
            }
        }

        using var memoryStream = new MemoryStream();
        await sitemap.SaveAsync(memoryStream, SaveOptions.None, CancellationToken.None).ConfigureAwait(false);

        var binaryData = memoryStream.ToArray();

        return new MediaResult(SystemClock.Instance.GetCurrentInstant(), new ETag(), "text/xml", binaryData);
    }
}