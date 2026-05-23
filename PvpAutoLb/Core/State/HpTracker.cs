using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;

namespace PvpAutoLb.Core;

internal sealed class HpTracker
{
    private const double WindowSeconds = 1.0;
    private const double MinDtSeconds = 0.25;
    private const double PruneStaleAfterSeconds = 5.0;

    private readonly Dictionary<ulong, Window> windows = new();
    private DateTime lastPrune = DateTime.UtcNow;

    public void Sample(IBattleChara t)
    {
        var now = DateTime.UtcNow;
        if (!windows.TryGetValue(t.EntityId, out var w) || (now - w.OldAt).TotalSeconds > WindowSeconds)
        {
            windows[t.EntityId] = new Window(t.CurrentHp, now, t.CurrentHp, now);
        }
        else
        {
            windows[t.EntityId] = w with { NewHp = t.CurrentHp, NewAt = now };
        }

        if ((now - lastPrune).TotalSeconds > PruneStaleAfterSeconds)
        {
            Prune(now);
            lastPrune = now;
        }
    }

    public TimeSpan? PredictTimeToDeath(IBattleChara t)
    {
        if (!windows.TryGetValue(t.EntityId, out var w)) return null;
        var dt = (w.NewAt - w.OldAt).TotalSeconds;
        if (dt < MinDtSeconds) return null;
        if (w.NewHp >= w.OldHp) return null;
        var hpPerSec = (w.OldHp - w.NewHp) / dt;
        if (hpPerSec <= 0) return null;
        return TimeSpan.FromSeconds(t.CurrentHp / hpPerSec);
    }

    private void Prune(DateTime now)
    {
        var stale = new List<ulong>();
        foreach (var kv in windows)
        {
            if ((now - kv.Value.NewAt).TotalSeconds > PruneStaleAfterSeconds)
                stale.Add(kv.Key);
        }
        foreach (var id in stale) windows.Remove(id);
    }

    private readonly record struct Window(uint OldHp, DateTime OldAt, uint NewHp, DateTime NewAt);
}
