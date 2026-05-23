using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections;

internal static class FeedbackSection
{
    public static void Draw(Configuration cfg)
    {
        Styling.SectionLabel("Feedback");

        using (Card.Begin("##feedback", Layout.FeedbackCardHeight * ImGuiHelpers.GlobalScale, Styling.CardBg, Styling.CardBorderDim))
        {
            var sound = cfg.PlaySoundOnFire;
            if (ImGui.Checkbox("Play sound on fire", ref sound))
            {
                cfg.PlaySoundOnFire = sound;
                cfg.Save();
            }
            Tooltip.OnHover("Plays a chat sound effect (1–16, like /se1 in chat) whenever the LB fires.");

            using (ImRaii.Disabled(!cfg.PlaySoundOnFire))
            {
                var id = cfg.FireSoundId;
                ImGui.SetNextItemWidth(160f * ImGuiHelpers.GlobalScale);
                if (ImGui.SliderInt("Sound ID", ref id, 1, 16))
                {
                    cfg.FireSoundId = id;
                    cfg.SaveDebounced();
                }
            }

            ImGui.Spacing();

            var chat = cfg.LogFireToChat;
            if (ImGui.Checkbox("Log to chat on fire", ref chat))
            {
                cfg.LogFireToChat = chat;
                cfg.Save();
            }
            Tooltip.OnHover("Prints a line to chat when an LB is fired, e.g. \"fired Seiton Tenchu on Striking Dummy\".");
        }
    }
}
