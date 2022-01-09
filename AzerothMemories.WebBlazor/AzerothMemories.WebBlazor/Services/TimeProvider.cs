using Humanizer;
using System.Globalization;

namespace AzerothMemories.WebBlazor.Services;

public class TimeProvider
{
    //private readonly IJSRuntime _jsRuntime;

    //private DateTimeZone _dateTimeZone;
    //private int _currentUpdateThreadId;

    //public TimeProvider(IJSRuntime jsRuntime)
    //{
    //    _jsRuntime = jsRuntime;
    //}

    //public async Task<bool> EnsureInitialized(CancellationToken cancellationToken)
    //{
    //    if (_dateTimeZone == null && Interlocked.CompareExchange(ref _currentUpdateThreadId, Environment.CurrentManagedThreadId, 0) == 0)
    //    {
    //        var timeZone = await _jsRuntime.InvokeAsync<string>("BlazorGetTimeZone", cancellationToken);
    //        _dateTimeZone = DateTimeZoneProviders.Tzdb[timeZone];

    //        Interlocked.Exchange(ref _currentUpdateThreadId, 0);

    //        return true;
    //    }

    //    return false;
    //}

    public ZonedDateTime GetTimeAsLocal(Instant instant)
    {
        var timeZone = DateTimeZoneProviders.Bcl[TimeZoneInfo.Local.Id];
        return instant.InZone(timeZone);
    }

    //public static string GetTimeAsLocalString(long postTime)
    //{
    //    return GetTimeAsLocalString(Instant.FromUnixTimeMilliseconds(postTime));
    //}

    public string GetTimeAsLocalStringAgo(long unixTimeStamp, bool shortDate)
    {
        return GetTimeAsLocalStringAgo(Instant.FromUnixTimeMilliseconds(unixTimeStamp), shortDate);
    }

    public string GetTimeAsLocalStringAgo(Instant instant, bool shortDate)
    {
        var culture = CultureInfo.CurrentCulture;
        var timeZone = DateTimeZoneProviders.Bcl[TimeZoneInfo.Local.Id];
        var zoned = instant.InZone(timeZone);

        var dateFormat = zoned.LocalDateTime.ToString(shortDate ? culture.DateTimeFormat.ShortDatePattern : culture.DateTimeFormat.LongDatePattern, culture);
        var timeFormat = zoned.LocalDateTime.ToString(culture.DateTimeFormat.LongTimePattern, culture);

        var nowZoned = SystemClock.Instance.GetCurrentInstant().InZone(timeZone);
        var humanized = zoned.LocalDateTime.ToDateTimeUnspecified().Humanize(dateToCompareAgainst: nowZoned.LocalDateTime.ToDateTimeUnspecified());

        var timeString = $"{dateFormat} {timeFormat} ({humanized}) ({timeZone.Id})";
        return timeString;
    }

    //public static string GetTimeAsLocalString(Instant instant)
    //{
    //    var culture = CultureInfo.CurrentCulture;
    //    var timeZone = DateTimeZoneProviders.Tzdb[TimeZoneInfo.Local.Id];
    //    var zonedDateTime = instant.InZone(timeZone);
    //    var dateFormat = zonedDateTime.LocalDateTime.ToString(culture.DateTimeFormat.ShortDatePattern, culture);
    //    var timeFormat = zonedDateTime.LocalDateTime.ToString(culture.DateTimeFormat.ShortTimePattern, culture);

    //    var timeSince = zonedDateTime - SystemClock.Instance.GetCurrentInstant().InZone(timeZone);
    //    return $"{timeFormat} {dateFormat} ({timeSince.ToTimeSpan().Humanize(minUnit: TimeUnit.Minute, maxUnit: TimeUnit.Year)}) ({timeZone.Id})";
    //}

    //public DateTimeOffset GetTimeAsLocalDateTime(Instant postTimeStamp)
    //{
    //    return GetTimeAsLocalDateTime(postTimeStamp.ToUnixTimeMilliseconds());
    //}

    //public DateTimeOffset GetTimeAsLocalDateTime(long timeStamp)
    //{
    //    if (timeStamp < 0)
    //    {
    //        return default;
    //    }

    //    var dateTimeZone = _dateTimeZone ?? DateTimeZone.Utc;
    //    var instant = Instant.FromUnixTimeMilliseconds(timeStamp).InZone(dateTimeZone);

    //    return instant.ToDateTimeOffset();
    //}

    //public long GetInstantFrom(LocalDateTime screenShotLocalTime)
    //{
    //    var dateTimeZone = _dateTimeZone ?? DateTimeZone.Utc;

    //    var screenShotZoned = screenShotLocalTime.InZoneStrictly(dateTimeZone);
    //    var screenShotInstant = screenShotZoned.ToInstant();
    //    return screenShotInstant.ToUnixTimeMilliseconds();
    //}

    //public long GetTimeFromLastModified(DateTimeOffset dateTimeOffset)
    //{
    //    var dateTimeZone = _dateTimeZone ?? DateTimeZone.Utc;
    //    var screenShotZoned = dateTimeOffset.ToOffsetDateTime().InZone(dateTimeZone);
    //    var screenShotInstant = screenShotZoned.ToInstant();

    //    return screenShotInstant.ToUnixTimeMilliseconds();
    //}

    public string GetJoinedDate(long timeStamp)
    {
        var culture = CultureInfo.CurrentCulture;
        var timeZone = DateTimeZoneProviders.Bcl[TimeZoneInfo.Local.Id];
        var instant = Instant.FromUnixTimeMilliseconds(timeStamp);
        var zoned = instant.InZone(timeZone);
        var dateFormat = zoned.LocalDateTime.ToString(culture.DateTimeFormat.YearMonthPattern, culture);

        return dateFormat;
    }

    public bool TryGetTimeFromFileName(string fileName, out long screenShotUnixTime)
    {
        screenShotUnixTime = -1;

        var prefix = "WoWScrnShot_";
        if (fileName.StartsWith(prefix))
        {
            var fileNameSpan = fileName.AsSpan()[prefix.Length..fileName.Length];
            if (!byte.TryParse(fileNameSpan[..2], out var month) || month > 12)
            {
                return false;
            }

            if (!byte.TryParse(fileNameSpan.Slice(2, 2), out var day) || day > 31)
            {
                return false;
            }

            if (!byte.TryParse(fileNameSpan.Slice(4, 2), out var year) || year > 100)
            {
                return false;
            }

            if (fileNameSpan[6] != '_')
            {
                return false;
            }

            if (!byte.TryParse(fileNameSpan.Slice(7, 2), out var hour) || hour > 24)
            {
                return false;
            }

            if (!byte.TryParse(fileNameSpan.Slice(9, 2), out var minute) || minute > 60)
            {
                return false;
            }

            //if (!byte.TryParse(fileNameSpan.Slice(11, 2), out _))
            //{
            //    return false;
            //}

            var screenShotLocalTime = new LocalDateTime(2000 + year, month, day, hour, minute, 0);

            var timeZone = DateTimeZoneProviders.Bcl[TimeZoneInfo.Local.Id];
            var screenShotZoned = screenShotLocalTime.InZoneStrictly(timeZone);
            var screenShotInstant = screenShotZoned.ToInstant();
            screenShotUnixTime = screenShotInstant.ToUnixTimeMilliseconds();

            return true;
        }

        return false;
    }
}