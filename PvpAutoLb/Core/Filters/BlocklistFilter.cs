using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;

namespace PvpAutoLb.Core;

internal static class BlocklistFilter
{
    public static bool IsBlocked(IBattleChara target, IReadOnlyList<string> blocklist)
    {
        if (blocklist.Count == 0) return false;
        var name = target.Name.TextValue;
        for (var i = 0; i < blocklist.Count; i++)
        {
            if (string.Equals(blocklist[i], name, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
