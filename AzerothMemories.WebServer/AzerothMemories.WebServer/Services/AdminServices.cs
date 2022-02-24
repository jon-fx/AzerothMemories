namespace AzerothMemories.WebServer.Services
{
    [RegisterComputeService]
    [RegisterAlias(typeof(IAdminServices))]
    public class AdminServices : DbServiceBase<AppDbContext>, IAdminServices
    {
        private readonly CommonServices _commonServices;

        public AdminServices(IServiceProvider services, CommonServices commonServices) : base(services)
        {
            _commonServices = commonServices;
        }
    }
}