using Dapper;
using System.Data;

namespace AzerothMemories.WebServer.Database;

public class DapperDateTimeTypeHandler : SqlMapper.TypeHandler<DateTime>
{
    public override DateTime Parse(object value)
    {
        if (value is DateTime dateTime)
        {
            return dateTime;
        }
        else if (value is NodaTime.Instant i)
        {
            return i.ToDateTimeUtc();
        }

        if (value is LocalDateTime local)
        {
            return local.ToDateTimeUnspecified();
        }

        throw new ArgumentException($"Invalid value of type '{value?.GetType().FullName}' given. DateTime or NodaTime.Instant values are supported.", nameof(value));
    }

    public void SetValue(IDbDataParameter parameter, object value) => parameter.Value = value;

    public override void SetValue(IDbDataParameter parameter, DateTime value) => parameter.Value = value;
}