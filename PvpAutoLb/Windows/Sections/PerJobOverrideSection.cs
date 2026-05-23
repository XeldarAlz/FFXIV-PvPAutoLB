using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Core;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections;

internal static class PerJobOverrideSection
{
    public static void Draw(Configuration cfg)
    {
        Styling.SectionLabel("Per-job override");

        var jobId = JobLookup.CurrentJobId;
        if (jobId == 0)
        {
            DrawOfflineCard();
            return;
        }

        var jobName = JobLookup.Name(jobId);
        var hasOverride = cfg.HasJobOverride(jobId);
        var height = hasOverride ? Layout.JobOverrideCardHeightExpanded : Layout.JobOverrideCardHeightCollapsed;

        using (Card.Begin("##joboverride", height * ImGuiHelpers.GlobalScale, Styling.CardBg, Styling.CardBorderDim))
        {
            if (ImGui.Checkbox($"Override for {jobName}", ref hasOverride))
            {
                if (hasOverride) cfg.EnsureJobOverride(jobId);
                else cfg.ClearJobOverride(jobId);
                cfg.Save();
            }
            Tooltip.OnHover("When on, this job uses its own threshold instead of the global one.");

            if (!cfg.HasJobOverride(jobId)) return;

            ImGui.Spacing();
            DrawControls(cfg, jobId);
        }
    }

    private static void DrawOfflineCard()
    {
        using (Card.Begin("##joboverride_off", Layout.JobOverrideOfflineCardHeight * ImGuiHelpers.GlobalScale, Styling.CardBgSoft, Styling.CardBorderDim))
        {
            using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
                ImGui.TextUnformatted("Log into a job to set a per-job override.");
        }
    }

    private static void DrawControls(Configuration cfg, uint jobId)
    {
        var j = cfg.EnsureJobOverride(jobId);

        var mode = j.Mode;
        if (ThresholdWidgets.DrawModeToggle("job", ref mode, segmentHeightDip: Layout.SegmentHeightCompact))
        {
            j.Mode = mode;
            cfg.Save();
        }

        ImGui.Spacing();

        var pct = j.Percent;
        var abs = j.Absolute;
        if (ThresholdWidgets.DrawValueControl("job", j.Mode, ref pct, ref abs))
        {
            j.Percent = pct;
            j.Absolute = abs;
            cfg.SaveDebounced();
        }
    }
}
