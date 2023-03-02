using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace AzerothMemories.WebBlazor.Common;

public static class Exceptions
{
    [DebuggerStepThrough]
    public static void ThrowIf([DoesNotReturnIf(true)] bool condition, [CallerArgumentExpression(nameof(condition))] string message = null)
    {
        if (condition)
        {
            throw new NotImplementedException();
        }
    }
}