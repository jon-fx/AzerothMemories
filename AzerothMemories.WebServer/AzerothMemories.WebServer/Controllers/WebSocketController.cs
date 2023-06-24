using Stl.Rpc.Server;

namespace AzerothMemories.WebServer.Controllers;

public sealed class WebSocketController : ControllerBase
{
    private readonly RpcWebSocketServer _webSocketServer;

    public WebSocketController(RpcWebSocketServer webSocketServer)
    {
        _webSocketServer = webSocketServer;
    }

    [Route("/rpc/ws"), ApiExplorerSettings(IgnoreApi = true)]
    public Task Get()
    {
        return _webSocketServer.Invoke(HttpContext);
    }
}