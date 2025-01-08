using System;

namespace Watermelon
{
    [Flags]
    public enum HelperTaskType
    {
        Gathering = GatherWood | GatherStone | GatherBerry | GatherCoconut | GatherGrass | GatherPumpkin,
        GatherWood = 1 << 0,
        GatherStone = 1 << 1,
        GatherBerry = 1 << 2,
        GatherCoconut = 1 << 3,
        GatherGrass = 1 << 4,
        GatherPumpkin = 1 << 5,

        Storing = 1 << 10,
        ConverterStoring = 1 << 11,

        Building = 1 << 15,
        Fishing = 1 << 16,
    }
}