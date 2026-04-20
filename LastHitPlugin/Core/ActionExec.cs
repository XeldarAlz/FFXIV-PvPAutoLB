using FFXIVClientStructs.FFXIV.Client.Game;

namespace LastHitPlugin.Core;

internal static unsafe class ActionExec
{
    public static bool TryUse(uint actionId)
    {
        if (actionId == 0) return false;
        var am = ActionManager.Instance();
        if (am == null) return false;
        if (am->AnimationLock > 0f) return false;
        if (am->GetActionStatus(ActionType.Action, actionId) != 0) return false;
        return am->UseAction(ActionType.Action, actionId);
    }

    public static uint GetStatus(uint actionId)
    {
        var am = ActionManager.Instance();
        return am == null ? uint.MaxValue : am->GetActionStatus(ActionType.Action, actionId);
    }

    public static bool IsReady(uint actionId) => actionId != 0 && GetStatus(actionId) == 0;
}
