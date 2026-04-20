using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

namespace LastHitPlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration config;

    public ConfigWindow(Plugin plugin) : base("LastHit — Settings###LastHitConfig")
    {
        Flags = ImGuiWindowFlags.NoCollapse;
        Size = new Vector2(440, 340);
        SizeCondition = ImGuiCond.FirstUseEver;
        config = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        DrawMasterButton();
        ImGui.Spacing();
        ImGui.Spacing();
        DrawThresholdSection();
        ImGui.Spacing();
        DrawTargetingSection();
    }

    private void DrawMasterButton()
    {
        var enabled = config.Enabled;
        var bg = enabled
            ? new Vector4(0.20f, 0.55f, 0.30f, 0.85f)
            : new Vector4(0.35f, 0.35f, 0.35f, 0.55f);
        var bgHover = bg + new Vector4(0.10f, 0.10f, 0.10f, 0f);
        var bgActive = bg - new Vector4(0.05f, 0.05f, 0.05f, 0f);

        using (ImRaii.PushColor(ImGuiCol.Button, bg))
        using (ImRaii.PushColor(ImGuiCol.ButtonHovered, bgHover))
        using (ImRaii.PushColor(ImGuiCol.ButtonActive, bgActive))
        {
            var label = enabled ? "● ENABLED" : "○ DISABLED";
            var height = 34f * ImGuiHelpers.GlobalScale;
            if (ImGui.Button(label, new Vector2(-1, height)))
            {
                config.Enabled = !config.Enabled;
                config.Save();
            }
        }
        Tooltip("Master switch. Click to toggle.");
    }

    private void DrawThresholdSection()
    {
        SectionHeader("Threshold");

        DrawSegmentedMode();

        ImGui.Spacing();

        if (config.ThresholdMode == ThresholdMode.Percent)
        {
            var pct = config.HpThresholdPercent;
            ImGui.SetNextItemWidth(-1);
            if (ImGui.SliderFloat("##pct", ref pct, 1f, 99f, "%.0f%% of max HP"))
            {
                config.HpThresholdPercent = pct;
                config.Save();
            }
        }
        else
        {
            var abs = (int)config.HpThresholdAbsolute;
            ImGui.SetNextItemWidth(-1);
            if (ImGui.DragInt("##abs", ref abs, 100f, 1, 500000, "%d HP"))
            {
                config.HpThresholdAbsolute = (uint)Math.Max(1, abs);
                config.Save();
            }
        }

        ImGui.Spacing();
        DrawPreviewBox();
    }

    private void DrawSegmentedMode()
    {
        var avail = ImGui.GetContentRegionAvail().X;
        var half = (avail - ImGui.GetStyle().ItemSpacing.X) / 2f;
        var height = 28f * ImGuiHelpers.GlobalScale;

        DrawSegment("Percent of max", ThresholdMode.Percent, new Vector2(half, height));
        ImGui.SameLine();
        DrawSegment("Absolute HP", ThresholdMode.Absolute, new Vector2(half, height));
    }

    private void DrawSegment(string label, ThresholdMode mode, Vector2 size)
    {
        var active = config.ThresholdMode == mode;
        var bg = active
            ? new Vector4(0.25f, 0.40f, 0.65f, 0.90f)
            : new Vector4(0.25f, 0.25f, 0.25f, 0.60f);
        var bgHover = bg + new Vector4(0.10f, 0.10f, 0.10f, 0f);

        using (ImRaii.PushColor(ImGuiCol.Button, bg))
        using (ImRaii.PushColor(ImGuiCol.ButtonHovered, bgHover))
        {
            if (ImGui.Button(label, size) && !active)
            {
                config.ThresholdMode = mode;
                config.Save();
            }
        }
    }

    private void DrawPreviewBox()
    {
        var preview = config.ThresholdMode == ThresholdMode.Percent
            ? $"Fires when target HP drops below {config.HpThresholdPercent:F0}% of its max."
            : $"Fires when target HP drops below {config.HpThresholdAbsolute:N0}.";

        var padding = new Vector2(10, 8) * ImGuiHelpers.GlobalScale;
        using (ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, padding))
        using (ImRaii.PushColor(ImGuiCol.ChildBg, new Vector4(0.10f, 0.10f, 0.12f, 0.55f)))
        using (ImRaii.Child("##preview", new Vector2(-1, 36f * ImGuiHelpers.GlobalScale), true,
                   ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
        {
            using (ImRaii.PushFont(UiBuilder.IconFont))
            using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudViolet))
                ImGui.TextUnformatted(FontAwesomeIcon.InfoCircle.ToIconString());
            ImGui.SameLine();
            using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudGrey))
                ImGui.TextUnformatted(preview);
        }
    }

    private void DrawTargetingSection()
    {
        SectionHeader("Targeting");

        var auto = config.AutoSelectLowestHp;
        if (ImGui.Checkbox("Auto-select lowest-HP enemy when no manual target", ref auto))
        {
            config.AutoSelectLowestHp = auto;
            config.Save();
        }
        Tooltip("When you have no target, scan for the lowest-HP hostile within range.");

        using (ImRaii.Disabled(!config.AutoSelectLowestHp))
        {
            var range = config.AutoSelectRangeYalms;
            ImGui.SetNextItemWidth(-1);
            if (ImGui.SliderFloat("##range", ref range, 5f, 50f, "Range: %.0f y"))
            {
                config.AutoSelectRangeYalms = range;
                config.Save();
            }
        }
    }

    private static void Tooltip(string text)
    {
        if (!ImGui.IsItemHovered()) return;
        using (ImRaii.Tooltip())
        {
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 24);
            ImGui.TextUnformatted(text);
            ImGui.PopTextWrapPos();
        }
    }

    private static void SectionHeader(string label)
    {
        using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudGrey3))
            ImGui.TextUnformatted(label.ToUpperInvariant());
        ImGui.Separator();
    }
}
