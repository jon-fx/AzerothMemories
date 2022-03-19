namespace AzerothMemories.WebServer.Services.Handlers;

internal interface IMoaCommandHandler<TCommand, TResult> where TCommand : ISessionCommand<TResult>
{
    Task<TResult> TryHandle(TCommand command);
}