using System.Collections.Generic;
using System.Linq;
using ECommons.DalamudServices;
using LastHitPlugin.Core;
using LuminaAction = Lumina.Excel.Sheets.Action;
using LuminaActionCategory = Lumina.Excel.Sheets.ActionCategory;
using LuminaClassJob = Lumina.Excel.Sheets.ClassJob;

namespace LastHitPlugin.Jobs;

internal sealed class DefaultPvpLimitBreakModule : IJobLimitBreakModule
{
    private const uint LimitBreakCategoryId = 15;
    private const uint UnsetRowId = uint.MaxValue;

    private readonly Dictionary<uint, uint> cache;

    public DefaultPvpLimitBreakModule()
    {
        cache = BuildCache();
        LogResolvedMap();
        if (cache.Count == 0) DumpDiagnostics();
    }

    public bool TryResolve(uint classJobId, out uint actionId)
        => cache.TryGetValue(classJobId, out actionId);

    private static Dictionary<uint, uint> BuildCache()
    {
        var sheet = Svc.Data.GetExcelSheet<LuminaAction>();
        if (sheet == null) return new Dictionary<uint, uint>();

        return sheet
            .Where(a => a.IsPvP)
            .Where(a => a.ActionCategory.RowId == LimitBreakCategoryId)
            .Where(a => a.ClassJob.RowId != 0 && a.ClassJob.RowId != UnsetRowId)
            .GroupBy(a => a.ClassJob.RowId)
            .ToDictionary(g => g.Key, g => g.First().RowId);
    }

    private void LogResolvedMap()
    {
        Svc.Log.Info($"[LastHit] Resolved {cache.Count} PvP Limit Breaks");
        var jobs = Svc.Data.GetExcelSheet<LuminaClassJob>();
        var actions = Svc.Data.GetExcelSheet<LuminaAction>();
        foreach (var kv in cache.OrderBy(k => k.Key))
        {
            var jobName = jobs?.GetRowOrDefault(kv.Key)?.Abbreviation.ToString() ?? $"Job{kv.Key}";
            var actionName = actions?.GetRowOrDefault(kv.Value)?.Name.ToString() ?? $"Action{kv.Value}";
            Svc.Log.Debug($"[LastHit]   Job {kv.Key} ({jobName}) -> Action {kv.Value} ({actionName})");
        }
    }

    private static void DumpDiagnostics()
    {
        var sheet = Svc.Data.GetExcelSheet<LuminaAction>();
        if (sheet == null) { Svc.Log.Error("[LastHit] Diag: Action sheet is null"); return; }

        var categories = Svc.Data.GetExcelSheet<LuminaActionCategory>();
        Svc.Log.Info("[LastHit] Diag: IsPvP actions by ActionCategory:");
        foreach (var g in sheet.Where(a => a.IsPvP).GroupBy(a => a.ActionCategory.RowId).OrderByDescending(g => g.Count()))
        {
            var name = categories?.GetRowOrDefault(g.Key)?.Name.ToString() ?? "?";
            Svc.Log.Info($"[LastHit]   cat={g.Key} ({name}) count={g.Count()}");
        }
    }
}
