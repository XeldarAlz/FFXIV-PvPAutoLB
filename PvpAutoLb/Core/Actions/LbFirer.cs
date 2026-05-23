using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;

namespace PvpAutoLb.Core;

internal sealed class LbFirer
{
    private readonly TargetSwapper swapper;

    // Tracks per-action submissions that have been accepted by ActionManager
    // but whose firing hasn't yet been observed (IsReady still true). When
    // IsReady flips false on a subsequent tick we treat that as the confirmed
    // fire — the action is now in cooldown.
    private readonly Dictionary<uint, bool> pendingByAction = new();

    public DateTime? LastFiredUtc { get; private set; }

    public LbFirer(TargetSwapper swapper)
    {
        this.swapper = swapper;
    }

    // Returns true exactly once per *confirmed* fire — i.e., the tick we
    // observe that an action we previously submitted has transitioned from
    // ready -> not-ready (it landed and went on cooldown). Greedy underneath:
    // re-submits on every ready tick so we win the engine's queue slot
    // against competing rotation plugins (Rotation Solver et al.). A 250ms
    // throttle here meant we submitted at ~4Hz while RS submitted at 30Hz,
    // so RS overwrote our queued LB roughly 7 out of every 8 times.
    public bool TryFire(uint jobId, IBattleChara target, out uint firedActionId)
    {
        firedActionId = 0;
        var actionIds = LbCatalog.ResolveActionIds(jobId);
        if (actionIds.Count == 0) return false;

        var targetId = (ulong)target.EntityId;
        foreach (var actionId in actionIds)
        {
            var ready = ActionExec.IsReady(actionId, targetId);
            var wasPending = pendingByAction.TryGetValue(actionId, out var p) && p;

            // Confirmed fire: submission landed, action is now on cooldown.
            if (wasPending && !ready)
            {
                pendingByAction[actionId] = false;
                LastFiredUtc = DateTime.UtcNow;
                firedActionId = actionId;
                return true;
            }

            if (!ready) continue;

            // Greedy submit. ActionManager overwrites any queued copy of the
            // same action, so resubmitting every tick is harmless — and it's
            // what lets us beat RS to the queue slot. The TargetSwapper
            // itself early-outs when current target already equals target, so
            // the swap is a no-op after the first call.
            swapper.Swap(target);
            if (ActionExec.TryUse(actionId, targetId))
                pendingByAction[actionId] = true;
        }
        return false;
    }
}
