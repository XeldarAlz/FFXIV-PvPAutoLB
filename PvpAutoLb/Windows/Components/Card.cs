using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace PvpAutoLb.Windows.Components;

internal static class Card
{
    public static CardScope Begin(string id, float height, Vector4 background, Vector4 border, float borderSize = 1f)
    {
        var style = Styling.PushCardStyle();
        var bg = ImRaii.PushColor(ImGuiCol.ChildBg, background);
        var br = ImRaii.PushColor(ImGuiCol.Border, border);
        var sz = ImRaii.PushStyle(ImGuiStyleVar.ChildBorderSize, borderSize);
        var child = ImRaii.Child(id, new Vector2(-1, height), true,
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
        return new CardScope(child, sz, br, bg, style);
    }

    public ref struct CardScope
    {
        private ImRaii.ChildDisposable child;
        private readonly IDisposable sz;
        private readonly IDisposable br;
        private readonly IDisposable bg;
        private readonly IDisposable style;

        internal CardScope(ImRaii.ChildDisposable child, IDisposable sz, IDisposable br, IDisposable bg, IDisposable style)
        {
            this.child = child;
            this.sz = sz;
            this.br = br;
            this.bg = bg;
            this.style = style;
        }

        public void Dispose()
        {
            child.Dispose();
            sz?.Dispose();
            br?.Dispose();
            bg?.Dispose();
            style?.Dispose();
        }
    }
}
