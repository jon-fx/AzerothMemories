using Humanizer;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AzerothMemories.WebServer.Common;

internal sealed class MethodTimeLogger : IDisposable
{
    private readonly ILogger _logger;
    private readonly string? _extraInfo;
    private readonly string? _callerMemberName;
    private readonly Stopwatch? _stopwatch;

    public MethodTimeLogger(ILogger logger, string? extraInfo = null, [CallerMemberName] string? callerMemberName = null)
    {
        _logger = logger;
        _extraInfo = extraInfo;
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

        var extraInfo = _extraInfo;
        if (string.IsNullOrWhiteSpace(extraInfo))
        {
            extraInfo = string.Empty;
        }
        else
        {
            extraInfo = $" - {extraInfo}";
        }

        _stopwatch.Stop();
        _logger.LogInformation("MethodTimeLogger - Method: {Name} completed in {Time}{ExtraInfo}", _callerMemberName, _stopwatch.Elapsed.Humanize(), extraInfo);
    }
}