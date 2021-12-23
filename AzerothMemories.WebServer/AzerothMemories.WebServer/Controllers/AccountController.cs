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
        public Task<AccountViewModel> TryGetAccount([FromRoute] long accountId)
        {
            return _accountServices.TryGetAccount(accountId);
        }

        [HttpPost("{newUsername}")]
        public Task TryChangeUsername([FromRoute] string newUsername)
        {
            return _accountServices.TryChangeUsername(newUsername);
        }
    }
}