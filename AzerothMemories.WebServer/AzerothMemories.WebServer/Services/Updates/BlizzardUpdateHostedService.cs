namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class BlizzardUpdateHostedService : IHostedService, IDisposable
{
    private readonly BlizzardUpdateHandler _blizzardUpdateHandler;

    private Timer _mainTimer;
    private int _mainTimerTickCounter;
    private int _mainTimerUpdateThreadId;

    public BlizzardUpdateHostedService(BlizzardUpdateHandler blizzardUpdateHandler)
    {
        _blizzardUpdateHandler = blizzardUpdateHandler;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _mainTimer = new Timer(OnMainTimerTick, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _mainTimer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _mainTimer?.Dispose();
        _mainTimer = null;
    }

    private async void OnMainTimerTick(object state)
    {
        if (Interlocked.CompareExchange(ref _mainTimerUpdateThreadId, Environment.CurrentManagedThreadId, 0) == 0)
        {
            var timerTickCounter = Interlocked.Increment(ref _mainTimerTickCounter);
            if (timerTickCounter == 1)
            {
                await _blizzardUpdateHandler.OnStarting().ConfigureAwait(false);
            }

            await _blizzardUpdateHandler.OnUpdating().ConfigureAwait(false);

            Interlocked.Exchange(ref _mainTimerUpdateThreadId, 0);
        }
    }
}