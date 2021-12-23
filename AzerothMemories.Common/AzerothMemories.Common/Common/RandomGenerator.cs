namespace AzerothMemories.Common;

public static class RandomGenerator
{
    private static int _seed = Environment.TickCount;
    private static readonly ThreadLocal<Random> _threadLocalRandom = new(() => new Random(Interlocked.Increment(ref _seed)));

    public static Random Instance => _threadLocalRandom.Value ?? throw new NotImplementedException();
}