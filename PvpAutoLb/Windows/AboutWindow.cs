using System;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;

namespace PvpAutoLb.Windows;

public sealed class AboutWindow : Window, IDisposable
{
    private const string RepoUrl = "https://github.com/XeldarAlz/FFXIV-AutoPVPLimitBreak";
    private const string IssuesUrl = "https://github.com/XeldarAlz/FFXIV-AutoPVPLimitBreak/issues";
    private const string DiscussionsUrl = "https://github.com/XeldarAlz/FFXIV-AutoPVPLimitBreak/discussions";
    private const string SecurityUrl = "https://github.com/XeldarAlz/FFXIV-AutoPVPLimitBreak/security/advisories/new";
    private const string Author = "XeldarAlz";
    private const string License = "AGPL-3.0-or-later";

    public AboutWindow() : base("PVP Auto LB — About###PvpAutoLbAbout")
    {
        Flags = ImGuiWindowFlags.NoCollapse;
        Size = new Vector2(560, 380);
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(360, 280),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
    }

    public void Dispose() { }

    public override void Draw()
    {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 5f);
        style.Push(ImGuiStyleVar.WindowRounding, 7f);
        style.Push(ImGuiStyleVar.ChildRounding, 6f);
        style.Push(ImGuiStyleVar.ItemSpacing, new Vector2(8, 8) * ImGuiHelpers.GlobalScale);

        DrawHeader();
        ImGui.Separator();
        ImGui.Spacing();
        DrawDetailsTable();
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        DrawDescription();
    }

    private static void DrawHeader()
    {
        ImGui.SetWindowFontScale(1.20f);
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextStrong))
            ImGui.TextUnformatted("PVP Auto LB");
        ImGui.SetWindowFontScale(1.0f);

        var version = typeof(AboutWindow).Assembly.GetName().Version?.ToString() ?? "?";
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
            ImGui.TextUnformatted($"build {version} · {License}");
    }

    private static void DrawDetailsTable()
    {
        if (!ImGui.BeginTable("##about_table", 2,
                ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoBordersInBody | ImGuiTableFlags.PadOuterX))
            return;

        ImGui.TableSetupColumn("##label", ImGuiTableColumnFlags.WidthFixed, 150f * ImGuiHelpers.GlobalScale);
        ImGui.TableSetupColumn("##value", ImGuiTableColumnFlags.WidthStretch);

        DrawTextRow("Author", Author);
        DrawLinkRow("GitHub", RepoUrl);
        DrawLinkRow("Report a bug", IssuesUrl);
        DrawLinkRow("Discussions", DiscussionsUrl);
        DrawLinkRow("Security disclosure", SecurityUrl);

        ImGui.EndTable();
    }

    private static void DrawDescription()
    {
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
        {
            ImGui.PushTextWrapPos(0f);
            ImGui.TextUnformatted(
                "PVP Auto LB fires your job's PvP Limit Break when an enemy's HP drops below " +
                "a configurable threshold. Bug reports and per-job test confirmations are welcome " +
                "via GitHub issues.");
            ImGui.PopTextWrapPos();
        }
    }

    private static void DrawTextRow(string label, string value)
    {
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
            ImGui.TextUnformatted(label);
        ImGui.TableSetColumnIndex(1);
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextStrong))
            ImGui.TextUnformatted(value);
    }

    private static void DrawLinkRow(string label, string url)
    {
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
            ImGui.TextUnformatted(label);

        ImGui.TableSetColumnIndex(1);
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.AccentBlue))
        {
            ImGui.PushTextWrapPos(ImGui.GetContentRegionMax().X);
            ImGui.TextUnformatted(url);
            ImGui.PopTextWrapPos();
        }
        if (!ImGui.IsItemHovered()) return;

        using (ImRaii.Tooltip())
            ImGui.TextUnformatted("Click to open · right-click to copy");
        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left)) OpenInBrowser(url);
        else if (ImGui.IsMouseClicked(ImGuiMouseButton.Right)) ImGui.SetClipboardText(url);
    }

    private static void OpenInBrowser(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Svc.Log.Warning(ex, "[PvpAutoLb] failed to launch browser for {0}, copied to clipboard instead", url);
            ImGui.SetClipboardText(url);
        }
    }
}
