using System;

namespace PvpAutoLb.Core;

[Flags]
public enum DutyMask
{
    None = 0,
    CrystallineConflict = 1 << 0,
    Frontline = 1 << 1,
    RivalWings = 1 << 2,
    CustomMatch = 1 << 3,
    Other = 1 << 4,
    All = CrystallineConflict | Frontline | RivalWings | CustomMatch | Other,
}
