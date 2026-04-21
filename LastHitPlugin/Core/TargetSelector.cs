using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace LastHitPlugin.Core;

internal static class TargetSelector
{
    public static IReadOnlyList<IBattleChara> ScanHostiles(float rangeYalms)
    {
        if (!Player.Available) return Array.Empty<IBattleChara>();
        var me = Player.Object!;
        var meId = me.GameObjectId;
        var mePos = me.Position;
        var rangeSq = rangeYalms * rangeYalms;

        return Svc.Objects
            .OfType<IBattleChara>()
            .Where(o => o.GameObjectId != meId)
            .Where(o => !o.IsDead && o.IsTargetable)
            .Where(IsEnemy)
            .Where(o =>
            {
                var dx = o.Position.X - mePos.X;
                var dz = o.Position.Z - mePos.Z;
                return dx * dx + dz * dz <= rangeSq;
            })
            .OrderBy(o => o.CurrentHp)
            .ToList();
    }

    private static bool IsEnemy(IBattleChara o)
    {
        if (o is IPlayerCharacter)
            return o.StatusFlags.HasFlag(StatusFlags.Hostile);
        if (o is IBattleNpc npc)
            return npc.BattleNpcKind == BattleNpcSubKind.Enemy;
        return false;
    }

    public static IBattleChara? PickLowestHp(float rangeYalms)
    {
        var list = ScanHostiles(rangeYalms);
        return list.Count > 0 ? list[0] : null;
    }
}
