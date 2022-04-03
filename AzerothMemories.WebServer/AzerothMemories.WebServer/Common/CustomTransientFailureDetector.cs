using Npgsql;
using Stl.Fusion.Operations.Reprocessing;

namespace AzerothMemories.WebServer.Common;

internal sealed class CustomTransientFailureDetector : ITransientFailureDetector
{
    public bool IsTransient(Exception error)
    {
        if (error is DbUpdateConcurrencyException)
        {
            return true;
        }

        if (error is PostgresException postgresException && postgresException.IsTransient)
        {
            return true;
        }

        return false;
    }
}