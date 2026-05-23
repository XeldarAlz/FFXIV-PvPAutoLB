using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using PvpAutoLb.Windows.Sections;

namespace PvpAutoLb.Windows;

public sealed class ConfigWindow : Window, IDisposable
{
    private readonly Configuration cfg;

    public ConfigWindow(Plugin plugin) : base("PVP Auto LB — Settings###PvpAutoLbConfig")
    {
        Flags = ImGuiWindowFlags.NoCollapse;
        Size = new Vector2(480, 520);
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(360, 320),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
        cfg = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 5f);
        style.Push(ImGuiStyleVar.WindowRounding, 7f);
        style.Push(ImGuiStyleVar.ChildRounding, 6f);
        style.Push(ImGuiStyleVar.ItemSpacing, new Vector2(8, 6) * ImGuiHelpers.GlobalScale);

        ThresholdSection.Draw(cfg);
        ImGui.Spacing();
        PerJobOverrideSection.Draw(cfg);
        ImGui.Spacing();
        TargetingSection.Draw(cfg);
        ImGui.Spacing();
        FilterSection.Draw(cfg);
        ImGui.Spacing();
        BlocklistSection.Draw(cfg);
        ImGui.Spacing();
        FeedbackSection.Draw(cfg);
    }
}
