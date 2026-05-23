using System;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.GameHelpers;

namespace PvpAutoLb.Core;

internal static class Geo
{
    public static float Distance2D(IBattleChara a, IBattleChara b)
    {
        var dx = a.Position.X - b.Position.X;
        var dz = a.Position.Z - b.Position.Z;
        return MathF.Sqrt(dx * dx + dz * dz);
    }

    public static float DistanceToPlayer(IBattleChara t)
    {
        if (!Player.Available) return 0f;
        var me = Player.Object!.Position;
        var dx = t.Position.X - me.X;
        var dz = t.Position.Z - me.Z;
        return MathF.Sqrt(dx * dx + dz * dz);
    }
}
