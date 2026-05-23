using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections;

internal static class ThresholdSection
{
    private const uint SamplePreviewMaxHp = 75_000u;

    public static void Draw(Configuration cfg)
    {
        Styling.SectionLabel("Threshold");

        using (Card.Begin("##threshold", Layout.ThresholdCardHeight * ImGuiHelpers.GlobalScale, Styling.CardBg, Styling.CardBorderDim))
        {
            DrawModeToggle(cfg);
            ImGui.Spacing();
            DrawValueControl(cfg);
            ImGui.Spacing();
            DrawPreview(cfg);
        }
    }

    private static void DrawModeToggle(Configuration cfg)
    {
        var mode = cfg.ThresholdMode;
        if (ThresholdWidgets.DrawModeToggle("thresh", ref mode))
        {
            cfg.ThresholdMode = mode;
            cfg.Save();
        }
    }

    private static void DrawValueControl(Configuration cfg)
    {
        var pct = cfg.HpThresholdPercent;
        var abs = cfg.HpThresholdAbsolute;
        if (ThresholdWidgets.DrawValueControl("thresh", cfg.ThresholdMode, ref pct, ref abs))
        {
            cfg.HpThresholdPercent = pct;
            cfg.HpThresholdAbsolute = abs;
            cfg.SaveDebounced();
        }
    }

    private static void DrawPreview(Configuration cfg)
    {
        var thresholdFrac = cfg.ThresholdMode == ThresholdMode.Percent
            ? Math.Clamp(cfg.HpThresholdPercent / 100f, 0.01f, 0.99f)
            : Math.Clamp((float)cfg.HpThresholdAbsolute / SamplePreviewMaxHp, 0.01f, 0.99f);

        var barHeight = Layout.PreviewBarHeight * ImGuiHelpers.GlobalScale;
        using (ImRaii.PushColor(ImGuiCol.PlotHistogram, Styling.AccentGreen))
        using (ImRaii.PushColor(ImGuiCol.FrameBg, new Vector4(0.06f, 0.07f, 0.08f, 0.90f)))
            ImGui.ProgressBar(1.0f, new Vector2(-1, barHeight), "preview");

        var rectMin = ImGui.GetItemRectMin();
        var rectMax = ImGui.GetItemRectMax();
        var x = rectMin.X + (rectMax.X - rectMin.X) * thresholdFrac;
        var draw = ImGui.GetWindowDrawList();
        var overlay = ImGui.GetColorU32(new Vector4(Styling.AccentRed.X, Styling.AccentRed.Y, Styling.AccentRed.Z, 0.32f));
        draw.AddRectFilled(rectMin, new Vector2(x, rectMax.Y), overlay, 3f);
        draw.AddLine(new Vector2(x, rectMin.Y - 2), new Vector2(x, rectMax.Y + 2),
            ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.90f)), 1.8f);

        ImGui.Spacing();
        var label = cfg.ThresholdMode == ThresholdMode.Percent
            ? $"Fires when target drops below {cfg.HpThresholdPercent:F0}% of its max HP."
            : $"Fires when target drops below {cfg.HpThresholdAbsolute:N0} HP.";
        using (ImRaii.PushFont(UiBuilder.IconFont))
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.AccentViolet))
            ImGui.TextUnformatted(FontAwesomeIcon.InfoCircle.ToIconString());
        ImGui.SameLine();
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextSecondary))
            ImGui.TextUnformatted(label);
    }
}
