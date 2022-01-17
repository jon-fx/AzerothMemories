namespace AzerothMemories.WebBlazor.Services;

public sealed class TimeProvider
{
    public DateTimeZone GetCurrentTimeZone()
    {
        return DateTimeZoneProviders.Tzdb.GetSystemDefault();
    }

    public ZonedDateTime GetTimeAsLocal(Instant instant)
    {
        var timeZone = GetCurrentTimeZone();
        return instant.InZone(timeZone);
    }

    public string GetTimeAsLocalString(Instant instant)
    {
        var culture = CultureInfo.CurrentCulture;
        var timeZone = GetCurrentTimeZone();
        var zoned = instant.InZone(timeZone);

        var dateFormat = zoned.LocalDateTime.ToString(culture.DateTimeFormat.ShortDatePattern, culture);
        var timeFormat = zoned.LocalDateTime.ToString(culture.DateTimeFormat.LongTimePattern, culture);

        var timeString = $"{dateFormat} {timeFormat} ({timeZone.Id})";
        return timeString;
    }

    public string GetTimeAsLocalStringAgo(long unixTimeStamp, bool shortDate)
    {
        return GetTimeAsLocalStringAgo(Instant.FromUnixTimeMilliseconds(unixTimeStamp), shortDate);
    }

    public string GetTimeAsLocalStringAgo(Instant instant, bool shortDate)
    {
        var culture = CultureInfo.CurrentCulture;
        var timeZone = GetCurrentTimeZone();
        var zoned = instant.InZone(timeZone);

        var dateFormat = zoned.LocalDateTime.ToString(shortDate ? culture.DateTimeFormat.ShortDatePattern : culture.DateTimeFormat.LongDatePattern, culture);
        var timeFormat = zoned.LocalDateTime.ToString(culture.DateTimeFormat.LongTimePattern, culture);

        var nowZoned = SystemClock.Instance.GetCurrentInstant().InZone(timeZone);
        var humanized = zoned.LocalDateTime.ToDateTimeUnspecified().Humanize(dateToCompareAgainst: nowZoned.LocalDateTime.ToDateTimeUnspecified());

        var timeString = $"{dateFormat} {timeFormat} ({humanized}) ({timeZone.Id})";
        return timeString;
    }

    public string GetJoinedDate(long timeStamp)
    {
        var culture = CultureInfo.CurrentCulture;
        var timeZone = GetCurrentTimeZone();
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

            var timeZone = GetCurrentTimeZone();
            var screenShotZoned = screenShotLocalTime.InZoneStrictly(timeZone);
            var screenShotInstant = screenShotZoned.ToInstant();
            screenShotUnixTime = screenShotInstant.ToUnixTimeMilliseconds();

            return true;
        }

        return false;
    }
}