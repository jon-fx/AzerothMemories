using Humanizer;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AzerothMemories.WebServer.Common;

internal sealed class MethodTimeLogger : IDisposable
{
    private readonly ILogger _logger;
    private readonly string? _callerMemberName;
    private readonly Stopwatch? _stopwatch;

    public MethodTimeLogger(ILogger logger, [CallerMemberName] string? callerMemberName = null)
    {
        _logger = logger;
        _callerMemberName = callerMemberName;

        if (string.IsNullOrWhiteSpace(_callerMemberName))
        {
        }
        else
        {
            _stopwatch = Stopwatch.StartNew();
        }
    }

    public void Dispose()
    {
        if (_stopwatch == null || _stopwatch.Elapsed < TimeSpan.FromMilliseconds(1))
        {
            return;
        }

        _stopwatch.Stop();
        _logger.LogInformation("MethodTimeLogger - Method: {Name} completed in {Time}", _callerMemberName, _stopwatch.Elapsed.Humanize());
    }
}