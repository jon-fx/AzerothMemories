using Microsoft.Net.Http.Headers;

namespace AzerothMemories.WebServer.Controllers;

[ApiController]
[JsonifyErrors]
public sealed class SiteMapController : ControllerBase
{
    private readonly CommonServices _commonServices;

    public SiteMapController(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    [HttpGet]
    [Route("~/sitemap.xml")]
    public async Task<IActionResult> TryGetSiteMapIndex()
    {
        var results = await _commonServices.MediaServices.TryGetSiteMapIndex().ConfigureAwait(false);
        if (results.MediaBytes != null)
        {
            return File(results.MediaBytes, results.MediaType, results.LastModified.ToDateTimeOffset(), EntityTagHeaderValue.Any);
        }

        return NotFound();
    }

    [HttpGet]
    [Route("~/sitemaps/sitemap-main.xml")]
    [Route("~/sitemaps/sitemap-{nameType}-{fileIndex:int}.xml")]
    public async Task<IActionResult> TryGetSiteMapNamed(SiteMapType nameType = SiteMapType.Main, int? fileIndex = null)
    {
        if (nameType == SiteMapType.Main)
        {
            var results = await _commonServices.MediaServices.TryGetSiteMapMain().ConfigureAwait(false);
            if (results.MediaBytes != null)
            {
                return File(results.MediaBytes, results.MediaType, results.LastModified.ToDateTimeOffset(), EntityTagHeaderValue.Any);
            }

            return NotFound();
        }

        fileIndex ??= 0;

        var counters = await _commonServices.MediaServices.GetSiteMapCounters().ConfigureAwait(false);
        var maxCounter = counters[(int)nameType];
        if (fileIndex >= maxCounter)
        {
            fileIndex = Math.Clamp(fileIndex.Value, 0, maxCounter - 1);
        }

        var namedResults = await _commonServices.MediaServices.TryGetSiteMapNamed(nameType, fileIndex.Value).ConfigureAwait(false);
        if (namedResults?.MediaBytes != null)
        {
            return File(namedResults.MediaBytes, namedResults.MediaType, namedResults.LastModified.ToDateTimeOffset(), EntityTagHeaderValue.Any);
        }

        return NotFound();
    }
}