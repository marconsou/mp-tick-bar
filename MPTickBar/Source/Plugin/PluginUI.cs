using Dalamud.Interface;
using ImGuiNET;
using System;
using System.Numerics;

namespace MPTickBar
{
    public class PluginUI
    {
        protected Configuration Configuration { get; init; }

        protected static float Spacing => ImGui.GetStyle().ItemSpacing.X * ImGuiHelpers.GlobalScale;

        private static bool SameLine(float spacing)
        {
            if (spacing != 0.0f)
            {
                ImGui.SameLine(0.0f, spacing);
                return true;
            }
            return false;
        }

        private static void Dummy(float spacing = 0.0f)
        {
            if (!PluginUI.SameLine(spacing))
                ImGui.Dummy(ImGui.GetStyle().ItemSpacing * ImGuiHelpers.GlobalScale);
        }

        private void UIElement<T>(Func<T, (bool, T)> function, T value, Action<T> setter, float spacing)
        {
            PluginUI.Dummy(spacing);
            PluginUI.SameLine(spacing);

            var result = function(value);

            if (result.Item1)
            {
                setter(result.Item2);
                this.Configuration.Save();
            }
        }

        protected void CheckBox(bool value, Action<bool> setter, string label, float spacing = 0.0f)
            => this.UIElement((param) => { return (ImGui.Checkbox(label, ref param), param); }, value, setter, spacing);

        protected void ColorEdit4(Vector4 value, Action<Vector4> setter, string label, float spacing = 0.0f)
            => this.UIElement((param) => { return (ImGui.ColorEdit4(label, ref param, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar), param); }, value, setter, spacing);

        protected void DragFloat(float value, Action<float> setter, string label, float speed, float min, float max, string format, float spacing = 0.0f)
            => this.UIElement((param) => { return (ImGui.DragFloat(label, ref param, speed, min, max, format, ImGuiSliderFlags.AlwaysClamp), param); }, value, setter, spacing);

        protected void DragFloat(float valueLeft, float valueRight, Action<float> setterLeft, Action<float> setterRight, string label, float speed, float min, float max, string format, float spacing = 0.0f)
        {
            this.UIElement((param) => { return (ImGui.DragFloat($"##{label}", ref param, speed, min, max, format, ImGuiSliderFlags.AlwaysClamp), param); }, valueLeft, setterLeft, spacing);
            this.UIElement((param) => { return (ImGui.DragFloat(label, ref param, speed, min, max, format, ImGuiSliderFlags.AlwaysClamp), param); }, valueRight, setterRight, PluginUI.Spacing);
        }

        protected void DragInt(int value, Action<int> setter, string label, int speed, int min, int max, string format, float spacing = 0.0f)
            => this.UIElement((param) => { return (ImGui.DragInt(label, ref param, speed, min, max, format, ImGuiSliderFlags.AlwaysClamp), param); }, value, setter, spacing);

        protected void DragInt(int valueLeft, int valueRight, Action<int> setterLeft, Action<int> setterRight, string label, int speed, int min, int max, string format, float spacing = 0.0f)
        {
            this.UIElement((param) => { return (ImGui.DragInt($"##{label}", ref param, speed, min, max, format, ImGuiSliderFlags.AlwaysClamp), param); }, valueLeft, setterLeft, spacing);
            this.UIElement((param) => { return (ImGui.DragInt(label, ref param, speed, min, max, format, ImGuiSliderFlags.AlwaysClamp), param); }, valueRight, setterRight, PluginUI.Spacing);
        }

        protected void Combo<T>(T value, Action<T> setter, string label, float spacing = 0.0f) where T : Enum
        {
            var options = value.GetNames();
            this.UIElement((param) => { var convertedParam = (int)(object)param; return (ImGui.Combo(label, ref convertedParam, options, options.Length), (T)(object)convertedParam); }, value, setter, spacing);
        }

        protected static void Text(string[] texts)
        {
            PluginUI.Dummy();
            foreach (var item in texts)
                ImGui.Text(item);
        }

        protected static void CollapsingHeader(string label, Action action)
        {
            if (ImGui.CollapsingHeader(label, ImGuiTreeNodeFlags.DefaultOpen))
            {
                action();
                PluginUI.Dummy();
            }
        }

        protected static void Tooltip(string message, float width = 420.0f)
        {
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(width * ImGuiHelpers.GlobalScale);
                ImGui.Text(message);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }
    }
}