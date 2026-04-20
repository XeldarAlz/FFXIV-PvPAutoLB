using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using LastHitPlugin.Core;
using LuminaAction = Lumina.Excel.Sheets.Action;

namespace LastHitPlugin.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;

    public MainWindow(Plugin plugin)
        : base("LastHit###LastHitMain", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(360, 320),
            MaximumSize = new Vector2(700, 700),
        };
        Size = new Vector2(400, 360);
        SizeCondition = ImGuiCond.FirstUseEver;
        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var cfg = plugin.Configuration;
        var ctrl = plugin.Controller;

        DrawTitleBar();
        ImGui.Spacing();
        DrawMasterButton(cfg);
        ImGui.Spacing();
        ImGui.Spacing();
        DrawTargetSection(ctrl, cfg);
        ImGui.Spacing();
        DrawLimitBreakSection(cfg, ctrl);
        ImGui.Spacing();
        DrawFooter(ctrl);
    }

    private void DrawTitleBar()
    {
        using (ImRaii.PushFont(UiBuilder.IconFont))
        using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudRed))
            ImGui.TextUnformatted(FontAwesomeIcon.Crosshairs.ToIconString());
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();

        ImGui.SetWindowFontScale(1.15f);
        ImGui.TextUnformatted("LastHit");
        ImGui.SetWindowFontScale(1.0f);

        var gearLabel = FontAwesomeIcon.Cog.ToIconString();
        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            var gearWidth = ImGui.CalcTextSize(gearLabel).X + ImGui.GetStyle().FramePadding.X * 2;
            ImGui.SameLine(ImGui.GetWindowContentRegionMax().X - gearWidth);
            if (ImGui.Button(gearLabel))
                plugin.ToggleConfigUi();
        }
        if (ImGui.IsItemHovered())
        {
            using (ImRaii.Tooltip())
                ImGui.TextUnformatted("Open settings");
        }
    }

    private void DrawMasterButton(Configuration cfg)
    {
        var enabled = cfg.Enabled;
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
                cfg.Enabled = !cfg.Enabled;
                cfg.Save();
            }
        }
        if (ImGui.IsItemHovered())
        {
            using (ImRaii.Tooltip())
                ImGui.TextUnformatted("Click to toggle.");
        }
    }

    private void DrawTargetSection(LastHitController ctrl, Configuration cfg)
    {
        SectionHeader("Target");

        var target = ctrl.LastResolvedTarget;
        if (target == null || target.IsDead)
        {
            using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudGrey))
                ImGui.TextUnformatted("No valid target.");
            return;
        }

        var cur = target.CurrentHp;
        var max = target.MaxHp;
        var fraction = max == 0 ? 0f : (float)cur / max;
        var pct = fraction * 100f;

        var below = IsBelowThreshold(cur, max, cfg);

        ImGui.SetWindowFontScale(1.25f);
        ImGui.TextUnformatted(target.Name.TextValue);
        ImGui.SetWindowFontScale(1.0f);

        DrawHpBar(fraction, cur, max, pct, below, cfg);
    }

    private static void DrawHpBar(float fraction, uint cur, uint max, float pct, bool below, Configuration cfg)
    {
        var barColor = below
            ? PulseColor(ImGuiColors.DalamudRed, new Vector4(1f, 0.4f, 0.4f, 1f))
            : ImGuiColors.ParsedGreen;

        var barHeight = 26f * ImGuiHelpers.GlobalScale;
        var overlay = $"{cur:N0} / {max:N0}  ({pct:F1}%%)";
        using (ImRaii.PushColor(ImGuiCol.PlotHistogram, barColor))
            ImGui.ProgressBar(fraction, new Vector2(-1, barHeight), overlay);

        var thresholdFraction = cfg.ThresholdMode == ThresholdMode.Percent
            ? Math.Clamp(cfg.HpThresholdPercent / 100f, 0f, 1f)
            : max == 0 ? 0f : Math.Clamp((float)cfg.HpThresholdAbsolute / max, 0f, 1f);

        var rectMin = ImGui.GetItemRectMin();
        var rectMax = ImGui.GetItemRectMax();
        var x = rectMin.X + (rectMax.X - rectMin.X) * thresholdFraction;
        var draw = ImGui.GetWindowDrawList();
        var lineColor = ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.85f));
        draw.AddLine(new Vector2(x, rectMin.Y - 2), new Vector2(x, rectMax.Y + 2), lineColor, 2f);
    }

    private static Vector4 PulseColor(Vector4 baseColor, Vector4 bright)
    {
        var t = (float)((Math.Sin(Environment.TickCount / 220.0) + 1.0) * 0.5);
        return Vector4.Lerp(baseColor, bright, t);
    }

    private void DrawLimitBreakSection(Configuration cfg, LastHitController ctrl)
    {
        SectionHeader("Limit Break");

        var jobId = Player.Available ? Player.Object!.ClassJob.RowId : 0u;
        var actionId = JobModuleRegistry.ResolveActionId(jobId);

        if (actionId == 0)
        {
            using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudGrey))
                ImGui.TextUnformatted("No PvP Limit Break mapped for current job.");
            return;
        }

        var actionSheet = Svc.Data.GetExcelSheet<LuminaAction>();
        var row = actionSheet?.GetRowOrDefault(actionId);
        var actionName = row?.Name.ToString() ?? $"Action {actionId}";
        var iconId = row?.Icon ?? 0;
        var ready = ActionExec.IsReady(actionId);

        var iconSize = 56f * ImGuiHelpers.GlobalScale;
        var borderColor = ready ? ImGuiColors.ParsedGreen : ImGuiColors.DalamudGrey3;
        DrawActionIcon(iconId, iconSize, borderColor);
        ImGui.SameLine();

        using (ImRaii.Group())
        {
            ImGui.Spacing();
            ImGui.TextUnformatted(actionName);
            var threshLabel = cfg.ThresholdMode == ThresholdMode.Percent
                ? $"Fires below {cfg.HpThresholdPercent:F0}% HP"
                : $"Fires below {cfg.HpThresholdAbsolute:N0} HP";
            using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudGrey))
                ImGui.TextUnformatted(threshLabel);
            DrawWillFireLine(cfg, ctrl, ready);
        }
    }

    private static void DrawActionIcon(uint iconId, float size, Vector4 borderColor)
    {
        var pos = ImGui.GetCursorScreenPos();
        var draw = ImGui.GetWindowDrawList();
        var pad = 3f;
        draw.AddRectFilled(pos - new Vector2(pad, pad), pos + new Vector2(size + pad, size + pad),
            ImGui.GetColorU32(borderColor), 6f);

        if (iconId != 0)
        {
            var tex = Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(iconId)).GetWrapOrDefault();
            if (tex != null)
            {
                ImGui.Image(tex.Handle, new Vector2(size, size));
                return;
            }
        }
        ImGui.Dummy(new Vector2(size, size));
    }

    private static void DrawWillFireLine(Configuration cfg, LastHitController ctrl, bool ready)
    {
        var target = ctrl.LastResolvedTarget;
        var willFire = ready && target != null && !target.IsDead
            && IsBelowThreshold(target.CurrentHp, target.MaxHp, cfg);

        ImGui.Spacing();
        var color = willFire
            ? ImGuiColors.DalamudRed
            : ready ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudGrey3;
        var icon = willFire
            ? FontAwesomeIcon.BoltLightning
            : ready ? FontAwesomeIcon.Circle : FontAwesomeIcon.Ban;
        using (ImRaii.PushFont(UiBuilder.IconFont))
        using (ImRaii.PushColor(ImGuiCol.Text, color))
            ImGui.TextUnformatted(icon.ToIconString());
        ImGui.SameLine();
        using (ImRaii.PushColor(ImGuiCol.Text, color))
            ImGui.TextUnformatted(willFire ? "Firing" : ready ? "Ready" : "Unavailable");
    }

    private void DrawFooter(LastHitController ctrl)
    {
        ImGui.Separator();
        using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudGrey3))
        {
            if (ctrl.LastFiredUtc is { } ts)
            {
                var ago = DateTime.UtcNow - ts;
                ImGui.TextUnformatted($"Last fired {ago.TotalSeconds:F1}s ago");
            }
            else
            {
                ImGui.TextUnformatted("Last fired: never");
            }
        }
    }

    private static bool IsBelowThreshold(uint cur, uint max, Configuration cfg)
    {
        if (cfg.ThresholdMode == ThresholdMode.Absolute)
            return cur < cfg.HpThresholdAbsolute;
        if (max == 0) return false;
        return 100f * cur / max < cfg.HpThresholdPercent;
    }

    private static void SectionHeader(string label)
    {
        using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudGrey3))
            ImGui.TextUnformatted(label.ToUpperInvariant());
        ImGui.Separator();
    }
}
