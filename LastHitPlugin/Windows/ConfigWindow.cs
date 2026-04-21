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
        Size = new Vector2(480, 420);
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(420, 380),
            MaximumSize = new Vector2(720, 700),
        };
        config = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 5f);
        style.Push(ImGuiStyleVar.WindowRounding, 7f);
        style.Push(ImGuiStyleVar.ChildRounding, 6f);
        style.Push(ImGuiStyleVar.ItemSpacing, new Vector2(8, 6) * ImGuiHelpers.GlobalScale);

        DrawTitleBar();
        DrawMasterCard();
        ImGui.Spacing();
        DrawThresholdCard();
        ImGui.Spacing();
        DrawTargetingCard();
    }

    // ── Title bar ────────────────────────────────────────────────────────────
    private static void DrawTitleBar()
    {
        var iconSize = 22f * ImGuiHelpers.GlobalScale;
        var iconTex = Plugin.PluginIcon?.GetWrapOrDefault();
        if (iconTex != null)
        {
            ImGui.Image(iconTex.Handle, new Vector2(iconSize, iconSize));
            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
        }
        ImGui.SetWindowFontScale(1.12f);
        ImGui.TextUnformatted("LastHit Settings");
        ImGui.SetWindowFontScale(1.0f);
        ImGui.Separator();
    }

    // ── Master ───────────────────────────────────────────────────────────────
    private void DrawMasterCard()
    {
        var enabled = config.Enabled;
        var bg = enabled
            ? new Vector4(0.18f, 0.55f, 0.30f, 0.85f)
            : new Vector4(0.28f, 0.28f, 0.30f, 0.55f);
        var bgHover = bg + new Vector4(0.08f, 0.08f, 0.08f, 0f);
        var bgActive = bg - new Vector4(0.04f, 0.04f, 0.04f, 0f);

        using (ImRaii.PushColor(ImGuiCol.Button, bg))
        using (ImRaii.PushColor(ImGuiCol.ButtonHovered, bgHover))
        using (ImRaii.PushColor(ImGuiCol.ButtonActive, bgActive))
        {
            var label = enabled ? "● ENABLED" : "○ DISABLED";
            var height = 32f * ImGuiHelpers.GlobalScale;
            if (ImGui.Button(label, new Vector2(-1, height)))
            {
                config.Enabled = !config.Enabled;
                config.Save();
            }
        }
        Tooltip("Master switch. Click to toggle.");
    }

    // ── Threshold ────────────────────────────────────────────────────────────
    private void DrawThresholdCard()
    {
        Styling.SectionLabel("Threshold");

        using var cardStyle = Styling.PushCardStyle();
        using (ImRaii.PushColor(ImGuiCol.ChildBg, Styling.CardBg))
        using (ImRaii.PushColor(ImGuiCol.Border, Styling.CardBorderDim))
        using (ImRaii.Child("##threshold", new Vector2(-1, 148f * ImGuiHelpers.GlobalScale), true,
                   ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
        {
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
            DrawThresholdPreview();
        }
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
            : new Vector4(0.20f, 0.21f, 0.24f, 0.60f);
        var bgHover = bg + new Vector4(0.08f, 0.08f, 0.08f, 0f);

        using (ImRaii.PushColor(ImGuiCol.Button, bg))
        using (ImRaii.PushColor(ImGuiCol.ButtonHovered, bgHover))
        using (ImRaii.PushColor(ImGuiCol.Text, active ? Styling.TextStrong : Styling.TextSecondary))
        {
            if (ImGui.Button(label, size) && !active)
            {
                config.ThresholdMode = mode;
                config.Save();
            }
        }
    }

    private void DrawThresholdPreview()
    {
        // Sample max HP for preview — arbitrary round number so threshold mark lives in a visible spot.
        const uint sampleMax = 75_000u;
        var thresholdFrac = config.ThresholdMode == ThresholdMode.Percent
            ? Math.Clamp(config.HpThresholdPercent / 100f, 0.01f, 0.99f)
            : Math.Clamp((float)config.HpThresholdAbsolute / sampleMax, 0.01f, 0.99f);

        var barHeight = 18f * ImGuiHelpers.GlobalScale;

        using (ImRaii.PushColor(ImGuiCol.PlotHistogram, Styling.AccentGreen))
        using (ImRaii.PushColor(ImGuiCol.FrameBg, new Vector4(0.06f, 0.07f, 0.08f, 0.90f)))
            ImGui.ProgressBar(1.0f, new Vector2(-1, barHeight), "preview");

        var rectMin = ImGui.GetItemRectMin();
        var rectMax = ImGui.GetItemRectMax();
        var x = rectMin.X + (rectMax.X - rectMin.X) * thresholdFrac;
        var draw = ImGui.GetWindowDrawList();

        // Tinted "below-threshold" overlay from left edge to the marker.
        var overlay = ImGui.GetColorU32(new Vector4(Styling.AccentRed.X, Styling.AccentRed.Y, Styling.AccentRed.Z, 0.32f));
        draw.AddRectFilled(rectMin, new Vector2(x, rectMax.Y), overlay, 3f);

        // Marker line.
        var lineColor = ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.90f));
        draw.AddLine(new Vector2(x, rectMin.Y - 2), new Vector2(x, rectMax.Y + 2), lineColor, 1.8f);

        ImGui.Spacing();
        var label = config.ThresholdMode == ThresholdMode.Percent
            ? $"Fires when target drops below {config.HpThresholdPercent:F0}% of its max HP."
            : $"Fires when target drops below {config.HpThresholdAbsolute:N0} HP.";
        using (ImRaii.PushFont(UiBuilder.IconFont))
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.AccentViolet))
            ImGui.TextUnformatted(FontAwesomeIcon.InfoCircle.ToIconString());
        ImGui.SameLine();
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextSecondary))
            ImGui.TextUnformatted(label);
    }

    // ── Targeting ────────────────────────────────────────────────────────────
    private void DrawTargetingCard()
    {
        Styling.SectionLabel("Targeting");

        using var cardStyle = Styling.PushCardStyle();
        using (ImRaii.PushColor(ImGuiCol.ChildBg, Styling.CardBg))
        using (ImRaii.PushColor(ImGuiCol.Border, Styling.CardBorderDim))
        using (ImRaii.Child("##targeting", new Vector2(-1, 112f * ImGuiHelpers.GlobalScale), true,
                   ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
        {
            var auto = config.AutoSelectLowestHp;
            if (ImGui.Checkbox("Auto-select lowest-HP hostile in range", ref auto))
            {
                config.AutoSelectLowestHp = auto;
                config.Save();
            }
            Tooltip("When on, the plugin continuously scans all visible hostiles and targets the one with the lowest HP — overriding your manual hard target.");

            using (ImRaii.Disabled(!config.AutoSelectLowestHp))
            {
                var range = config.AutoSelectRangeYalms;
                ImGui.SetNextItemWidth(-1);
                if (ImGui.SliderFloat("##range", ref range, 5f, 50f, "Range: %.0f y"))
                {
                    config.AutoSelectRangeYalms = range;
                    config.Save();
                }

                ImGui.Spacing();
                using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
                    ImGui.TextUnformatted(RangeHint(range));
            }
        }
    }

    private static string RangeHint(float range) => range switch
    {
        <= 8f => "≈ melee range",
        <= 20f => "≈ mid range",
        <= 35f => "≈ ranged combat",
        _ => "≈ whole arena",
    };

    // ── Helpers ──────────────────────────────────────────────────────────────
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
}
