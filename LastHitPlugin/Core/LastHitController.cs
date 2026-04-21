using System;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Throttlers;

namespace LastHitPlugin.Core;

internal sealed class LastHitController : IDisposable
{
    private readonly Configuration config;

    public DateTime? LastFiredUtc { get; private set; }
    public IBattleChara? LastResolvedTarget { get; private set; }

    public LastHitController(Configuration config)
    {
        this.config = config;
        Svc.Framework.Update += OnTick;
    }

    public void Dispose()
    {
        Svc.Framework.Update -= OnTick;
    }

    private void OnTick(IFramework _)
    {
        if (!config.Enabled) { LastResolvedTarget = null; return; }
        if (!Player.Available) { LastResolvedTarget = null; return; }
        if (!EzThrottler.Throttle("LastHit.Tick", 100)) return;

        var target = ResolveTarget();
        LastResolvedTarget = target;
        if (target == null) return;

        if (!IsBelowThreshold(target)) return;

        var actionIds = JobModuleRegistry.ResolveActionIds(Player.Object!.ClassJob.RowId);
        if (actionIds.Count == 0) return;

        foreach (var actionId in actionIds)
        {
            if (!ActionExec.IsReady(actionId)) continue;
            if (!EzThrottler.Throttle($"LastHit.Fire.{actionId}", 500)) continue;
            if (ActionExec.TryUse(actionId))
            {
                LastFiredUtc = DateTime.UtcNow;
                return;
            }
        }
    }

    private bool IsBelowThreshold(IBattleChara target)
    {
        if (config.ThresholdMode == ThresholdMode.Absolute)
            return target.CurrentHp < config.HpThresholdAbsolute;

        if (target.MaxHp == 0) return false;
        var pct = 100f * target.CurrentHp / target.MaxHp;
        return pct < config.HpThresholdPercent;
    }

    private IBattleChara? ResolveTarget()
    {
        if (Svc.Targets.Target is IBattleChara manual && !manual.IsDead)
            return manual;
        if (!config.AutoSelectLowestHp) return null;
        return TargetSelector.PickLowestHp(config.AutoSelectRangeYalms);
    }
}
