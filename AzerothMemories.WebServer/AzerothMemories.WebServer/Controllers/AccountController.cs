namespace AzerothMemories.WebServer.Controllers
{
    [ApiController, JsonifyErrors]
    [Route("api/[controller]/[action]")]
    public class AccountController : ControllerBase, IAccountServices
    {
        private readonly IAccountServices _accountServices;
        private readonly CommonServices _commonServices;

        public AccountController(IAccountServices accountServices, CommonServices commonServices)
        {
            _accountServices = accountServices;
            _commonServices = commonServices;
        }

        [HttpGet("{accountId}"), Publish]
        public Task<AccountViewModel> TryGetAccount(Session session, [FromRoute] long accountId, CancellationToken cancellationToken = default)
        {
            return _accountServices.TryGetAccount(session, accountId, cancellationToken);
        }

        [HttpPost("{newUsername}")]
        public Task<string> TryChangeUsername(Session session, [FromRoute] string newUsername, CancellationToken cancellationToken = default)
        {
            return _accountServices.TryChangeUsername(session, newUsername, cancellationToken);
        }
    }
}