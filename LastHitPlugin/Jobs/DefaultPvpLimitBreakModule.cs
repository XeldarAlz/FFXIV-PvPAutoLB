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
    private static readonly uint[] Empty = System.Array.Empty<uint>();

    private readonly Dictionary<uint, uint[]> cache;

    public DefaultPvpLimitBreakModule()
    {
        cache = BuildCache();
        LogResolvedMap();
        if (cache.Count == 0) DumpDiagnostics();
    }

    public IReadOnlyList<uint> Resolve(uint classJobId)
        => cache.TryGetValue(classJobId, out var ids) ? ids : Empty;

    private static Dictionary<uint, uint[]> BuildCache()
    {
        var sheet = Svc.Data.GetExcelSheet<LuminaAction>();
        if (sheet == null) return new Dictionary<uint, uint[]>();

        var primaries = sheet
            .Where(a => a.IsPvP)
            .Where(a => a.ActionCategory.RowId == LimitBreakCategoryId)
            .Where(a => a.ClassJob.RowId != 0 && a.ClassJob.RowId != UnsetRowId)
            .GroupBy(a => a.ClassJob.RowId)
            .ToDictionary(g => g.Key, g => g.Select(a => a.RowId).ToList());

        var followUpsByParent = sheet
            .Where(a => a.IsPvP && a.ActionCombo.RowId != 0)
            .GroupBy(a => a.ActionCombo.RowId)
            .ToDictionary(g => g.Key, g => g.Select(a => a.RowId).ToArray());

        var result = new Dictionary<uint, uint[]>();
        foreach (var kv in primaries)
        {
            var ordered = new List<uint>(kv.Value);
            var seen = new HashSet<uint>(kv.Value);
            var queue = new Queue<uint>(kv.Value);
            while (queue.Count > 0)
            {
                var parent = queue.Dequeue();
                if (!followUpsByParent.TryGetValue(parent, out var followUps)) continue;
                foreach (var id in followUps)
                {
                    if (seen.Add(id))
                    {
                        ordered.Add(id);
                        queue.Enqueue(id);
                    }
                }
            }
            result[kv.Key] = ordered.ToArray();
        }
        return result;
    }

    private void LogResolvedMap()
    {
        Svc.Log.Info($"[LastHit] Resolved PvP Limit Breaks for {cache.Count} jobs");
        var jobs = Svc.Data.GetExcelSheet<LuminaClassJob>();
        var actions = Svc.Data.GetExcelSheet<LuminaAction>();
        foreach (var kv in cache.OrderBy(k => k.Key))
        {
            var jobName = jobs?.GetRowOrDefault(kv.Key)?.Abbreviation.ToString() ?? $"Job{kv.Key}";
            var parts = string.Join(", ", kv.Value.Select(id =>
            {
                var n = actions?.GetRowOrDefault(id)?.Name.ToString() ?? $"Action{id}";
                return $"{id} ({n})";
            }));
            Svc.Log.Debug($"[LastHit]   Job {kv.Key} ({jobName}) -> [{parts}]");
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
