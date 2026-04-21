using System;
using System.Collections.Generic;

namespace LastHitPlugin.Core;

internal static class JobModuleRegistry
{
    private static readonly List<IJobLimitBreakModule> Modules = new();

    public static void Register(IJobLimitBreakModule module) => Modules.Add(module);

    public static void Clear() => Modules.Clear();

    public static IReadOnlyList<uint> ResolveActionIds(uint classJobId)
    {
        foreach (var m in Modules)
        {
            var list = m.Resolve(classJobId);
            if (list.Count > 0) return list;
        }
        return Array.Empty<uint>();
    }

    public static uint ResolveActionId(uint classJobId)
    {
        var list = ResolveActionIds(classJobId);
        return list.Count > 0 ? list[0] : 0u;
    }
}
