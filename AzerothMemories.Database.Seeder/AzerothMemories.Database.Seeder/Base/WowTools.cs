﻿namespace AzerothMemories.Database.Seeder.Base;

internal sealed class WowTools
{
    private const string BuildConst = "10.0.2.47213";

    public WowToolsInternal Main { get; } = new(BuildConst, true);
}