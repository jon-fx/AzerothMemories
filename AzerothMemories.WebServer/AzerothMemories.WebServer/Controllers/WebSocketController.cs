namespace AzerothMemories.WebServer.Controllers;

public sealed class WebSocketController : ControllerBase
{
    private readonly WebSocketServer _webSocketServer;

    public WebSocketController(WebSocketServer webSocketServer)
    {
        _webSocketServer = webSocketServer;
    }

    [Route("/fusion/ws")]
    public Task Get()
    {
        return _webSocketServer.HandleRequest(HttpContext);
    }
}