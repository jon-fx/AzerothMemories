using Stl.CommandR;

namespace AzerothMemories.WebServer.Common
{
    public sealed class CommonServices
    {
        private readonly IServiceProvider _serviceProvider;

        //private IClusterClient _clusterClient;
        //private QueuedUpdateHandler _queuedUpdateHandler;
        //private PubSubProvider _pubSubProvider;

        public CommonServices(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            Config = _serviceProvider.GetRequiredService<CommonConfig>();
            Commander = _serviceProvider.GetRequiredService<ICommander>();
            DatabaseProvider = _serviceProvider.GetRequiredService<DatabaseProvider>();
            //WarcraftClientProvider = _serviceProvider.GetRequiredService<WarcraftClientProvider>();
        }

        public ICommander Commander { get; }

        public CommonConfig Config { get; }

        //public IClusterClient ClusterClient => _clusterClient ??= _serviceProvider.GetRequiredService<IClusterClient>();

        public DatabaseProvider DatabaseProvider { get; }

        //internal QueuedUpdateHandler QueuedUpdateHandler => _queuedUpdateHandler ??= _serviceProvider.GetRequiredService<QueuedUpdateHandler>();

        //internal WarcraftClientProvider WarcraftClientProvider { get; }

        //public PubSubProvider PubSubProvider => _pubSubProvider ??= _serviceProvider.GetRequiredService<PubSubProvider>();
    }
}