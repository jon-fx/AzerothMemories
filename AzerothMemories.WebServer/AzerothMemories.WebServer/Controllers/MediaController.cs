using Microsoft.Net.Http.Headers;

namespace AzerothMemories.WebServer.Controllers
{
    [ApiController, JsonifyErrors]
    //[AutoValidateAntiforgeryToken]
    [Route("api/[controller]/[action]")]
    public sealed class MediaController : ControllerBase
    {
        private readonly CommonServices _commonServices;
        private readonly ISessionResolver _sessionResolver;

        public MediaController(CommonServices commonServices, ISessionResolver sessionResolver)
        {
            _commonServices = commonServices;
            _sessionResolver = sessionResolver;
        }

        [HttpGet]
        [Route("~/media/{container}/{fileName}")]
        public async Task<IActionResult> Get(Session session, [FromRoute] string container, [FromRoute] string fileName, [FromQuery] MediaSize size = MediaSize.Xxl)
        {
            MediaResult results = null;
            if (container.ToLowerInvariant() == ZExtensions.BlobMedia)
            {
                results = await _commonServices.MediaServices.TryGetMedia(session, fileName).ConfigureAwait(false);
            }
            else if (container.ToLowerInvariant() == ZExtensions.BlobAvatars)
            {
                results = await _commonServices.MediaServices.TryGetAvatar(session, fileName).ConfigureAwait(false);
            }
            else if (container.ToLowerInvariant() == ZExtensions.BlobImages)
            {
                size = (MediaSize)Math.Clamp((byte)size, (byte)0, (byte)MediaSize.Xxl);
                results = await _commonServices.MediaServices.TryGetUserMedia(session, fileName, size).ConfigureAwait(false);
            }

            if (results != null)
            {
                return File(results.MediaBytes, results.MediaType, results.LastModified.ToDateTimeOffset(), EntityTagHeaderValue.Any);
            }

            return NotFound();
        }
    }
}