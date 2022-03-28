using Microsoft.Net.Http.Headers;

namespace AzerothMemories.WebServer.Controllers;

[ApiController]
[JsonifyErrors]
[UseDefaultSession]
//[AutoValidateAntiforgeryToken]
[Route("api/[controller]/[action]")]
public sealed class MediaController : ControllerBase
{
    private readonly CommonServices _commonServices;

    public MediaController(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    [HttpGet]
    [Route("~/media/{container}/{fileName}")]
    public async Task<IActionResult> Get(Session session, [FromRoute] string container, [FromRoute] string fileName, [FromQuery] MediaSize size = MediaSize.True)
    {
        MediaResult results = null;
        container = container.ToLowerInvariant();
        if (container == ZExtensions.BlobStaticMedia)
        {
            results = await _commonServices.MediaServices.TryGetStaticMedia(session, fileName).ConfigureAwait(false);
        }
        else if (container == ZExtensions.BlobUserAvatars)
        {
            results = await _commonServices.MediaServices.TryGetUserAvatar(session, fileName).ConfigureAwait(false);
        }
        else if (container == ZExtensions.BlobUserUploads)
        {
            size = (MediaSize)Math.Clamp((byte)size, (byte)0, (byte)MediaSize.True);
            results = await _commonServices.MediaServices.TryGetUserUpload(session, fileName, size).ConfigureAwait(false);
        }

        if (results?.MediaBytes != null)
        {
            return File(results.MediaBytes, results.MediaType, results.LastModified.ToDateTimeOffset(), EntityTagHeaderValue.Any);
        }

        return NotFound();
    }
}