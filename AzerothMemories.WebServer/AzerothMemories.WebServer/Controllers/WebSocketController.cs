using Stl.Rpc.Server;

namespace AzerothMemories.WebServer.Controllers;

public sealed class WebSocketController : ControllerBase
{
    private readonly RpcWebSocketServer _webSocketServer;

    public WebSocketController(RpcWebSocketServer webSocketServer)
    {
        _webSocketServer = webSocketServer;
    }

    [Route("/rpc/ws")]
    public Task Get()
    {
        return _webSocketServer.HandleRequest(HttpContext);
    }
}