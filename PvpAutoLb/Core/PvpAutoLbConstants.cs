namespace PvpAutoLb.Core;

internal static class PvpAutoLbConstants
{
    // FFXIV's "no target" sentinel for ActionManager.
    public const ulong NoTargetEntityId = 0xE000_0000UL;

    public const int TickThrottleMs = 33;
    // Short window after an accepted submission. Long enough to avoid double-
    // queueing the same fire, short enough that a fizzled queue (target died /
    // moved out of range when flushed) retargets quickly on the next tick.
    public const int FireThrottleMs = 250;
    public const int SaveThrottleMs = 250;

    // If a target's predicted time-to-death is shorter than this, skip the LB —
    // the target will be dead before our cast lands (animation lock + cast).
    public const int DoomedTtdMs = 1200;

    // Wait this long after a hard-target swap before restoring the user's
    // previous target — long enough for the LB animation to release.
    public const int TargetRestoreDelayMs = 700;

    // Used when an LB's EffectRange is 0 but its shape implies AoE.
    public const float UnknownAoeFallbackYalms = 5f;

    public const float DefaultThresholdPercent = 30f;
    public const uint DefaultThresholdAbsolute = 7000;
    public const float DefaultAutoSelectRangeYalms = 30f;

    // ActionCategory row id for "Limit Break" in the Action sheet.
    public const uint LimitBreakCategoryId = 15;
}
