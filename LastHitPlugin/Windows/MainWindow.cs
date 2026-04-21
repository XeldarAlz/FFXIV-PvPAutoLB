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
using LuminaClassJob = Lumina.Excel.Sheets.ClassJob;

namespace LastHitPlugin.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;

    public MainWindow(Plugin plugin) : base("LastHit###LastHitMain")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(380, 380),
            MaximumSize = new Vector2(720, 900),
        };
        Size = new Vector2(420, 440);
        SizeCondition = ImGuiCond.FirstUseEver;
        Flags = ImGuiWindowFlags.NoCollapse;
        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var cfg = plugin.Configuration;
        var ctrl = plugin.Controller;

        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 5f);
        style.Push(ImGuiStyleVar.WindowRounding, 7f);
        style.Push(ImGuiStyleVar.ChildRounding, 6f);
        style.Push(ImGuiStyleVar.ItemSpacing, new Vector2(8, 6) * ImGuiHelpers.GlobalScale);

        DrawTitleBar();
        DrawMasterButton(cfg);
        ImGui.Spacing();
        DrawTargetSection(ctrl, cfg);
        ImGui.Spacing();
        DrawLimitBreakSection(cfg, ctrl);
        ImGui.Spacing();
        DrawFooter(ctrl);
    }

    // ── Title bar ────────────────────────────────────────────────────────────
    private void DrawTitleBar()
    {
        var iconSize = 22f * ImGuiHelpers.GlobalScale;
        var iconTex = Plugin.PluginIcon?.GetWrapOrDefault();
        if (iconTex != null)
            ImGui.Image(iconTex.Handle, new Vector2(iconSize, iconSize));
        else
        {
            using (ImRaii.PushFont(UiBuilder.IconFont))
            using (ImRaii.PushColor(ImGuiCol.Text, Styling.AccentRed))
                ImGui.TextUnformatted(FontAwesomeIcon.Crosshairs.ToIconString());
        }
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();

        ImGui.SetWindowFontScale(1.18f);
        ImGui.TextUnformatted("LastHit");
        ImGui.SetWindowFontScale(1.0f);

        if (Player.Available)
        {
            ImGui.SameLine();
            var jobName = GetJobAbbreviation(Player.Object!.ClassJob.RowId);
            if (!string.IsNullOrEmpty(jobName))
            {
                using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
                    ImGui.TextUnformatted("· " + jobName);
            }
        }

        var gearLabel = FontAwesomeIcon.Cog.ToIconString();
        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            var gearWidth = ImGui.CalcTextSize(gearLabel).X + ImGui.GetStyle().FramePadding.X * 2;
            ImGui.SameLine(ImGui.GetWindowContentRegionMax().X - gearWidth);
            if (ImGui.Button(gearLabel))
                plugin.ToggleConfigUi();
        }
        if (ImGui.IsItemHovered())
            using (ImRaii.Tooltip())
                ImGui.TextUnformatted("Open settings");

        ImGui.Separator();
    }

    private static string GetJobAbbreviation(uint jobId)
    {
        if (jobId == 0) return string.Empty;
        var sheet = Svc.Data.GetExcelSheet<LuminaClassJob>();
        return sheet?.GetRowOrDefault(jobId)?.Abbreviation.ToString() ?? string.Empty;
    }

    // ── Master button ────────────────────────────────────────────────────────
    private void DrawMasterButton(Configuration cfg)
    {
        var enabled = cfg.Enabled;
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
                cfg.Enabled = !cfg.Enabled;
                cfg.Save();
            }
        }
        if (ImGui.IsItemHovered())
            using (ImRaii.Tooltip())
                ImGui.TextUnformatted("Master switch. Click to toggle.");
    }

    // ── Target section ───────────────────────────────────────────────────────
    private void DrawTargetSection(LastHitController ctrl, Configuration cfg)
    {
        Styling.SectionLabel("Target");

        var candidates = TargetSelector.ScanHostiles(cfg.AutoSelectRangeYalms);
        if (candidates.Count == 0)
        {
            DrawEmptyCard($"Scanning — no hostile targets within {cfg.AutoSelectRangeYalms:F0}y.",
                FontAwesomeIcon.Satellite);
            return;
        }

        var picked = candidates[0];
        var pickedBelow = IsBelowThreshold(picked.CurrentHp, picked.MaxHp, cfg);
        DrawHeroCard(picked, pickedBelow, cfg);

        if (candidates.Count > 1)
        {
            ImGui.Spacing();
            using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
                ImGui.TextUnformatted($"OTHER IN RANGE ({candidates.Count - 1})");

            for (var i = 1; i < candidates.Count; i++)
                DrawCandidateRow(candidates[i], cfg);
        }
    }

    private void DrawEmptyCard(string message, FontAwesomeIcon icon)
    {
        using var style = Styling.PushCardStyle();
        using (ImRaii.PushColor(ImGuiCol.ChildBg, Styling.CardBgSoft))
        using (ImRaii.PushColor(ImGuiCol.Border, Styling.CardBorderDim))
        using (ImRaii.Child("##empty", new Vector2(-1, 52f * ImGuiHelpers.GlobalScale), true,
                   ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
        {
            ImGui.Spacing();
            using (ImRaii.PushFont(UiBuilder.IconFont))
            using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextMuted))
                ImGui.TextUnformatted(icon.ToIconString());
            ImGui.SameLine();
            using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
                ImGui.TextUnformatted(message);
        }
    }

    private void DrawHeroCard(IBattleChara target, bool below, Configuration cfg)
    {
        var cur = target.CurrentHp;
        var max = target.MaxHp;
        var fraction = max == 0 ? 0f : (float)cur / max;
        var pct = fraction * 100f;
        var distance = DistanceTo(target);
        var firing = below && cfg.Enabled;

        Vector4 borderColor;
        if (firing) borderColor = Styling.PulseColor(Styling.AccentRed, Styling.AccentRedBright, 800);
        else if (below) borderColor = new Vector4(0.45f, 0.22f, 0.24f, 1f); // muted red: would fire but paused
        else if (cfg.Enabled) borderColor = Styling.AccentOrange;
        else borderColor = Styling.CardBorderDim;

        using var style = Styling.PushCardStyle();
        using (ImRaii.PushColor(ImGuiCol.ChildBg, Styling.CardBgHero))
        using (ImRaii.PushColor(ImGuiCol.Border, borderColor))
        using (ImRaii.PushStyle(ImGuiStyleVar.ChildBorderSize, 1.5f))
        using (ImRaii.Child("##hero", new Vector2(-1, 92f * ImGuiHelpers.GlobalScale), true,
                   ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
        {
            // Name + distance row
            ImGui.SetWindowFontScale(1.22f);
            ImGui.TextUnformatted(target.Name.TextValue);
            ImGui.SetWindowFontScale(1.0f);

            var distLabel = $"{distance:F1} y";
            var distWidth = ImGui.CalcTextSize(distLabel).X;
            ImGui.SameLine(ImGui.GetContentRegionAvail().X + ImGui.GetCursorPosX() - distWidth);
            using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
                ImGui.TextUnformatted(distLabel);

            // HP bar with threshold line
            DrawHpBar(fraction, cur, max, pct, firing, cfg, 24f);

            // Status line
            ImGui.Spacing();
            DrawStatusBadge(below, firing, cfg);
        }
    }

    private void DrawStatusBadge(bool below, bool firing, Configuration cfg)
    {
        FontAwesomeIcon icon;
        Vector4 color;
        string text;
        if (firing)
        {
            icon = FontAwesomeIcon.BoltLightning;
            color = Styling.PulseColor(Styling.AccentRed, Styling.AccentRedBright, 600);
            text = "Firing — target below threshold";
        }
        else if (below)
        {
            icon = FontAwesomeIcon.Pause;
            color = new Vector4(0.75f, 0.45f, 0.48f, 1f);
            text = "Paused — would fire if plugin enabled";
        }
        else if (cfg.Enabled)
        {
            icon = FontAwesomeIcon.Hourglass;
            color = Styling.AccentOrange;
            text = cfg.ThresholdMode == ThresholdMode.Percent
                ? $"Waiting — drop below {cfg.HpThresholdPercent:F0}% HP"
                : $"Waiting — drop below {cfg.HpThresholdAbsolute:N0} HP";
        }
        else
        {
            icon = FontAwesomeIcon.Pause;
            color = Styling.TextMuted;
            text = "Plugin disabled";
        }
        using (ImRaii.PushFont(UiBuilder.IconFont))
        using (ImRaii.PushColor(ImGuiCol.Text, color))
            ImGui.TextUnformatted(icon.ToIconString());
        ImGui.SameLine();
        using (ImRaii.PushColor(ImGuiCol.Text, color))
            ImGui.TextUnformatted(text);
    }

    private static void DrawHpBar(float fraction, uint cur, uint max, float pct, bool firing,
        Configuration cfg, float heightDip)
    {
        var barColor = firing
            ? Styling.PulseColor(Styling.AccentRed, Styling.AccentRedBright, 600)
            : Styling.AccentGreen;

        var barHeight = heightDip * ImGuiHelpers.GlobalScale;
        var overlay = $"{cur:N0} / {max:N0}   ({pct:F1}%%)";
        using (ImRaii.PushColor(ImGuiCol.PlotHistogram, barColor))
        using (ImRaii.PushColor(ImGuiCol.FrameBg, new Vector4(0.06f, 0.07f, 0.08f, 0.90f)))
            ImGui.ProgressBar(fraction, new Vector2(-1, barHeight), overlay);

        var thresholdFraction = cfg.ThresholdMode == ThresholdMode.Percent
            ? Math.Clamp(cfg.HpThresholdPercent / 100f, 0f, 1f)
            : max == 0 ? 0f : Math.Clamp((float)cfg.HpThresholdAbsolute / max, 0f, 1f);

        var rectMin = ImGui.GetItemRectMin();
        var rectMax = ImGui.GetItemRectMax();
        var x = rectMin.X + (rectMax.X - rectMin.X) * thresholdFraction;
        var draw = ImGui.GetWindowDrawList();
        var lineColor = ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.75f));
        draw.AddLine(new Vector2(x, rectMin.Y - 1), new Vector2(x, rectMax.Y + 1), lineColor, 1.5f);
    }

    private static void DrawCandidateRow(IBattleChara target, Configuration cfg)
    {
        var cur = target.CurrentHp;
        var max = target.MaxHp;
        var fraction = max == 0 ? 0f : (float)cur / max;
        var pct = fraction * 100f;
        var below = IsBelowThreshold(cur, max, cfg);
        var distance = DistanceTo(target);

        var rowHeight = 30f * ImGuiHelpers.GlobalScale;
        var rowBg = below ? new Vector4(0.18f, 0.07f, 0.08f, 0.45f) : Styling.CardBgSoft;

        using (ImRaii.PushColor(ImGuiCol.ChildBg, rowBg))
        using (ImRaii.PushColor(ImGuiCol.Border, Styling.CardBorderDim))
        using (ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, new Vector2(8, 4) * ImGuiHelpers.GlobalScale))
        using (ImRaii.Child("##cand_" + target.GameObjectId, new Vector2(-1, rowHeight), true,
                   ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
        {
            var avail = ImGui.GetContentRegionAvail().X;
            var nameW = avail * 0.38f;
            var distW = avail * 0.12f;
            var barW = avail * 0.50f;

            using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextSecondary))
            {
                var name = target.Name.TextValue;
                if (ImGui.CalcTextSize(name).X > nameW)
                {
                    while (name.Length > 3 && ImGui.CalcTextSize(name + "…").X > nameW)
                        name = name[..^1];
                    name += "…";
                }
                ImGui.TextUnformatted(name);
            }
            ImGui.SameLine(nameW);
            using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
                ImGui.TextUnformatted($"{distance:F0}y");
            ImGui.SameLine(nameW + distW);

            var barColor = below ? Styling.AccentRed : Styling.AccentGreen;
            using (ImRaii.PushColor(ImGuiCol.PlotHistogram, barColor))
            using (ImRaii.PushColor(ImGuiCol.FrameBg, new Vector4(0.05f, 0.06f, 0.07f, 0.80f)))
                ImGui.ProgressBar(fraction, new Vector2(barW, 14f * ImGuiHelpers.GlobalScale), $"{pct:F0}%%");
        }
    }

    private static float DistanceTo(IBattleChara target)
    {
        if (!Player.Available) return 0f;
        var me = Player.Object!.Position;
        var t = target.Position;
        var dx = t.X - me.X;
        var dz = t.Z - me.Z;
        return MathF.Sqrt(dx * dx + dz * dz);
    }

    // ── Limit break section ──────────────────────────────────────────────────
    private void DrawLimitBreakSection(Configuration cfg, LastHitController ctrl)
    {
        Styling.SectionLabel("Limit Break");

        var jobId = Player.Available ? Player.Object!.ClassJob.RowId : 0u;
        var actionId = JobModuleRegistry.ResolveActionId(jobId);

        if (actionId == 0)
        {
            DrawEmptyCard("No PvP Limit Break mapped for current job.", FontAwesomeIcon.Ban);
            return;
        }

        var actionSheet = Svc.Data.GetExcelSheet<LuminaAction>();
        var row = actionSheet?.GetRowOrDefault(actionId);
        var actionName = row?.Name.ToString() ?? $"Action {actionId}";
        var iconId = row?.Icon ?? 0;
        var targetEntity = ctrl.LastResolvedTarget?.EntityId ?? 0xE000_0000u;
        var ready = ActionExec.IsReady(actionId, targetEntity);
        var target = ctrl.LastResolvedTarget;
        var wouldFire = ready && target != null && !target.IsDead
            && IsBelowThreshold(target.CurrentHp, target.MaxHp, cfg);
        var firing = wouldFire && cfg.Enabled;

        var cardHeight = 82f * ImGuiHelpers.GlobalScale;
        Vector4 borderColor;
        if (firing) borderColor = Styling.PulseColor(Styling.AccentRed, Styling.AccentRedBright, 600);
        else if (wouldFire) borderColor = new Vector4(0.45f, 0.22f, 0.24f, 1f);
        else if (ready && cfg.Enabled) borderColor = Styling.AccentOrange;
        else borderColor = Styling.CardBorderDim;

        using var style = Styling.PushCardStyle();
        using (ImRaii.PushColor(ImGuiCol.ChildBg, Styling.CardBg))
        using (ImRaii.PushColor(ImGuiCol.Border, borderColor))
        using (ImRaii.PushStyle(ImGuiStyleVar.ChildBorderSize, firing ? 1.5f : 1.0f))
        using (ImRaii.Child("##lbcard", new Vector2(-1, cardHeight), true,
                   ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
        {
            var iconSize = 56f * ImGuiHelpers.GlobalScale;
            DrawActionIconWithGlow(iconId, iconSize, firing, wouldFire, ready, cfg.Enabled);
            ImGui.SameLine();

            using (ImRaii.Group())
            {
                ImGui.Spacing();
                using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextStrong))
                    ImGui.TextUnformatted(actionName);

                var threshLabel = cfg.ThresholdMode == ThresholdMode.Percent
                    ? $"Fires below {cfg.HpThresholdPercent:F0}% HP"
                    : $"Fires below {cfg.HpThresholdAbsolute:N0} HP";
                using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
                    ImGui.TextUnformatted(threshLabel);

                DrawLbStatusPill(firing, wouldFire, ready, cfg.Enabled);
            }
        }
    }

    private static void DrawActionIconWithGlow(uint iconId, float size, bool firing, bool wouldFire, bool ready, bool enabled)
    {
        var pos = ImGui.GetCursorScreenPos();
        var draw = ImGui.GetWindowDrawList();
        var pad = 3f;

        if (firing)
        {
            var glowAlpha = 0.35f + 0.55f * Styling.Pulse(600);
            var glowColor = new Vector4(Styling.AccentRed.X, Styling.AccentRed.Y, Styling.AccentRed.Z, glowAlpha);
            var gp = 6f;
            draw.AddRectFilled(pos - new Vector2(gp, gp), pos + new Vector2(size + gp, size + gp),
                ImGui.GetColorU32(glowColor), 10f);
        }

        Vector4 borderColor;
        if (firing) borderColor = Styling.PulseColor(Styling.AccentRed, Styling.AccentRedBright, 600);
        else if (wouldFire) borderColor = new Vector4(0.45f, 0.22f, 0.24f, 1f);
        else if (ready && enabled) borderColor = Styling.AccentOrange;
        else borderColor = Styling.CardBorderDim;
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

    private static void DrawLbStatusPill(bool firing, bool wouldFire, bool ready, bool enabled)
    {
        FontAwesomeIcon icon;
        Vector4 color;
        string text;
        if (firing)
        {
            icon = FontAwesomeIcon.BoltLightning;
            color = Styling.PulseColor(Styling.AccentRed, Styling.AccentRedBright, 600);
            text = "FIRING";
        }
        else if (wouldFire)
        {
            icon = FontAwesomeIcon.Pause;
            color = new Vector4(0.75f, 0.45f, 0.48f, 1f);
            text = "PAUSED";
        }
        else if (!enabled)
        {
            icon = FontAwesomeIcon.Pause;
            color = Styling.TextMuted;
            text = "DISABLED";
        }
        else if (ready)
        {
            icon = FontAwesomeIcon.Circle;
            color = Styling.AccentOrange;
            text = "READY";
        }
        else
        {
            icon = FontAwesomeIcon.Ban;
            color = Styling.TextMuted;
            text = "UNAVAILABLE";
        }

        ImGui.Spacing();
        using (ImRaii.PushFont(UiBuilder.IconFont))
        using (ImRaii.PushColor(ImGuiCol.Text, color))
            ImGui.TextUnformatted(icon.ToIconString());
        ImGui.SameLine();
        using (ImRaii.PushColor(ImGuiCol.Text, color))
            ImGui.TextUnformatted(text);
    }

    // ── Footer ───────────────────────────────────────────────────────────────
    private void DrawFooter(LastHitController ctrl)
    {
        ImGui.Separator();
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextMuted))
        {
            var fired = ctrl.LastFiredUtc is { } ts
                ? $"Last fired {(DateTime.UtcNow - ts).TotalSeconds:F1}s ago"
                : "Last fired: never";
            var build = $"build {typeof(MainWindow).Assembly.GetName().Version}";
            ImGui.TextUnformatted(fired);
            var buildW = ImGui.CalcTextSize(build).X;
            ImGui.SameLine(ImGui.GetContentRegionAvail().X + ImGui.GetCursorPosX() - buildW);
            ImGui.TextUnformatted(build);
        }
    }

    private static bool IsBelowThreshold(uint cur, uint max, Configuration cfg)
    {
        if (cfg.ThresholdMode == ThresholdMode.Absolute)
            return cur < cfg.HpThresholdAbsolute;
        if (max == 0) return false;
        return 100f * cur / max < cfg.HpThresholdPercent;
    }
}
