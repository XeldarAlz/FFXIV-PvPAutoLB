using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Core;

namespace PvpAutoLb.Windows.Components;

internal static class HpBar
{
    private static readonly Vector4 ShieldColor = new(0.95f, 0.78f, 0.30f, 0.80f);
    private static readonly Vector4 ThresholdLineColor = new(1f, 1f, 1f, 0.75f);
    private static readonly Vector4 BarBg = new(0.06f, 0.07f, 0.08f, 0.90f);

    public static void Draw(uint cur, uint max, uint shield, bool firing, Configuration cfg, uint jobId, float heightDip)
    {
        var fraction = max == 0 ? 0f : (float)cur / max;
        var pct = fraction * 100f;
        var barColor = firing
            ? Styling.PulseColor(Styling.AccentRed, Styling.AccentRedBright, Styling.PulseFast)
            : Styling.AccentGreen;

        var barHeight = heightDip * ImGuiHelpers.GlobalScale;
        var overlay = shield > 0
            ? $"{cur:N0} (+{shield:N0}) / {max:N0}   ({pct:F1}%%)"
            : $"{cur:N0} / {max:N0}   ({pct:F1}%%)";

        using (ImRaii.PushColor(ImGuiCol.PlotHistogram, barColor))
        using (ImRaii.PushColor(ImGuiCol.FrameBg, BarBg))
            ImGui.ProgressBar(fraction, new Vector2(-1, barHeight), overlay);

        var rectMin = ImGui.GetItemRectMin();
        var rectMax = ImGui.GetItemRectMax();
        var width = rectMax.X - rectMin.X;
        var draw = ImGui.GetWindowDrawList();

        if (shield > 0 && max > 0)
        {
            var shieldFraction = Math.Clamp((float)shield / max, 0f, 1f - fraction);
            var startX = rectMin.X + width * fraction;
            var endX = startX + width * shieldFraction;
            draw.AddRectFilled(new Vector2(startX, rectMin.Y), new Vector2(endX, rectMax.Y),
                ImGui.GetColorU32(ShieldColor));
        }

        var th = cfg.EffectiveThresholdFor(jobId);
        var thresholdFraction = th.Mode == ThresholdMode.Percent
            ? Math.Clamp(th.Percent / 100f, 0f, 1f)
            : max == 0 ? 0f : Math.Clamp((float)th.Absolute / max, 0f, 1f);

        var x = rectMin.X + width * thresholdFraction;
        draw.AddLine(new Vector2(x, rectMin.Y - 1), new Vector2(x, rectMax.Y + 1),
            ImGui.GetColorU32(ThresholdLineColor), 1.5f);
    }
}
