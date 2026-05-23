using System;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;

namespace PvpAutoLb.Core;

// Owns the hard-target swap state machine: snapshot the user's pre-swap
// target on first yank, then restore it after the LB animation lands.
internal sealed class TargetSwapper
{
    private ulong? userOriginalTargetId;
    private ulong? lastOurSwapTargetId;
    private DateTime lastSwapAtUtc;

    public void Swap(IBattleChara target)
    {
        var current = Svc.Targets.Target;
        if (current?.EntityId == target.EntityId) return;

        if (userOriginalTargetId == null && current is IBattleChara prev)
            userOriginalTargetId = prev.EntityId;

        Svc.Targets.Target = target;
        lastOurSwapTargetId = target.EntityId;
        lastSwapAtUtc = DateTime.UtcNow;
    }

    public void TryRestore()
    {
        if (userOriginalTargetId == null) return;
        if ((DateTime.UtcNow - lastSwapAtUtc).TotalMilliseconds < PvpAutoLbConstants.TargetRestoreDelayMs) return;

        // If the user has manually moved off our pick, respect their choice
        // and stop tracking — don't snap them back.
        if (Svc.Targets.Target?.EntityId != lastOurSwapTargetId)
        {
            ClearRestoreState();
            return;
        }

        var original = Svc.Objects.SearchById(userOriginalTargetId.Value);
        if (original is IBattleChara b && !b.IsDead)
            Svc.Targets.Target = b;
        else
            Svc.Targets.Target = null;

        ClearRestoreState();
    }

    private void ClearRestoreState()
    {
        userOriginalTargetId = null;
        lastOurSwapTargetId = null;
    }
}
