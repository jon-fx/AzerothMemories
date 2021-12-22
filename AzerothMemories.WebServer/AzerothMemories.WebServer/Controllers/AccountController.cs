namespace AzerothMemories.WebServer.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class AccountController : ControllerBase, IAccountServices
    {
        private readonly IAccountServices _accountServices;
        private readonly CommonServices _commonServices;

        public AccountController(IAccountServices accountServices, CommonServices commonServices)
        {
            _accountServices = accountServices;
            _commonServices = commonServices;
        }

        [HttpGet, Publish]
        public Task<AccountViewModel> GetAccount(Session session, long accountId)
        {
            return _accountServices.GetAccount(session, accountId);
        }
    }
}