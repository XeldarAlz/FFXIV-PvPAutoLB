using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;

namespace PvpAutoLb.Windows.Components;

internal static class ThresholdWidgets
{
    public static bool DrawModeToggle(string idScope, ref ThresholdMode mode, float segmentHeightDip = Layout.SegmentHeightDefault)
    {
        var avail = ImGui.GetContentRegionAvail().X;
        var half = (avail - ImGui.GetStyle().ItemSpacing.X) / 2f;
        var size = new Vector2(half, segmentHeightDip * ImGuiHelpers.GlobalScale);

        var changed = false;
        if (SegmentedControl.DrawSegment("Percent of max", idScope + "_pct", mode == ThresholdMode.Percent, size))
        {
            mode = ThresholdMode.Percent;
            changed = true;
        }
        ImGui.SameLine();
        if (SegmentedControl.DrawSegment("Absolute HP", idScope + "_abs", mode == ThresholdMode.Absolute, size))
        {
            mode = ThresholdMode.Absolute;
            changed = true;
        }
        return changed;
    }

    public static bool DrawValueControl(string idScope, ThresholdMode mode, ref float percent, ref uint absolute)
    {
        ImGui.SetNextItemWidth(-1);
        if (mode == ThresholdMode.Percent)
        {
            var p = percent;
            if (ImGui.SliderFloat("##" + idScope + "_pctv", ref p, 1f, 99f, "%.0f%% of max HP"))
            {
                percent = p;
                return true;
            }
        }
        else
        {
            var a = (int)absolute;
            if (ImGui.DragInt("##" + idScope + "_absv", ref a, 100f, 1, 500_000, "%d HP"))
            {
                absolute = (uint)Math.Max(1, a);
                return true;
            }
        }
        return false;
    }
}
