using System;

namespace KillEmAll.Enums
{
    [Flags]
    public enum RelativePosition
    {
        UNDEFINED = 1,
        ABOVE = 2,
        BELOW = 4,
        LEFT = 8,
        RIGHT = 16,
        SAME = 32
    }
}
