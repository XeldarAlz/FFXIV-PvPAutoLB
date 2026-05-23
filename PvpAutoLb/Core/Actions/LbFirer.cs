using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.Throttlers;

namespace PvpAutoLb.Core;

internal sealed class LbFirer
{
    private readonly TargetSwapper swapper;
    private readonly Dictionary<uint, string> fireKeyCache = new();

    public DateTime? LastFiredUtc { get; private set; }

    public LbFirer(TargetSwapper swapper)
    {
        this.swapper = swapper;
    }

    // Returns true once a fire was submitted to the action queue. The caller
    // owns stats/feedback so the firer stays single-purpose.
    public bool TryFire(uint jobId, IBattleChara target, out uint firedActionId)
    {
        firedActionId = 0;
        var actionIds = LbCatalog.ResolveActionIds(jobId);
        if (actionIds.Count == 0) return false;

        var targetId = (ulong)target.EntityId;
        foreach (var actionId in actionIds)
        {
            if (!ActionExec.IsReady(actionId, targetId)) continue;
            if (!EzThrottler.Throttle(GetFireThrottleKey(actionId), PvpAutoLbConstants.FireThrottleMs)) continue;

            swapper.Swap(target);

            if (ActionExec.TryUse(actionId, targetId))
            {
                LastFiredUtc = DateTime.UtcNow;
                firedActionId = actionId;
                return true;
            }
        }
        return false;
    }

    private string GetFireThrottleKey(uint actionId)
    {
        if (!fireKeyCache.TryGetValue(actionId, out var key))
        {
            key = PvpAutoLbConstants.ThrottleKeys.FirePrefix + actionId;
            fireKeyCache[actionId] = key;
        }
        return key;
    }
}
