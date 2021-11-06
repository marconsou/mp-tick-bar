using Dalamud.Interface;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;

namespace MPTickBar
{
    public class PluginUI
    {
        protected Configuration Configuration { get; init; }

        private void SaveConfiguration<T>(T changedValue, Action<T> setter)
        {
            setter(changedValue);
            this.Configuration.Save();
        }

        private static void SameLine(float spacing)
        {
            if (spacing != 0.0f)
                ImGui.SameLine(0.0f, spacing);
        }

        protected void CheckBox(bool value, Action<bool> setter, string label, float spacing = 0.0f)
        {
            var changedValue = value;
            PluginUI.SameLine(spacing);
            if (ImGui.Checkbox(label, ref changedValue))
                this.SaveConfiguration(changedValue, setter);
        }

        protected void ColorEdit4(Vector4 value, Action<Vector4> setter, string label, float spacing = 0.0f)
        {
            var changedValue = value;
            PluginUI.SameLine(spacing);
            if (ImGui.ColorEdit4(label, ref changedValue, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar))
                this.SaveConfiguration(changedValue, setter);
        }

        protected void DragFloat(float value, Action<float> setter, string label, float speed, float min, float max, string format, float spacing = 0.0f)
        {
            var changedValue = value;
            PluginUI.SameLine(spacing);
            if (ImGui.DragFloat(label, ref changedValue, speed, min, max, format, ImGuiSliderFlags.AlwaysClamp))
                this.SaveConfiguration(changedValue, setter);
        }

        protected void DragFloat(float valueLeft, float valueRight, Action<float> setterLeft, Action<float> setterRight, string label, float speed, float min, float max, string format, float spacing = 0.0f)
        {
            this.DragFloat(valueLeft, setterLeft, " ", speed, min, max, format, spacing);
            this.DragFloat(valueRight, setterRight, label, speed, min, max, format, ImGui.GetStyle().ItemSpacing.X * ImGuiHelpers.GlobalScale);
        }

        protected void DragInt(int value, Action<int> setter, string label, int speed, int min, int max, string format, float spacing = 0.0f)
        {
            var changedValue = value;
            PluginUI.SameLine(spacing);
            if (ImGui.DragInt(label, ref changedValue, speed, min, max, format, ImGuiSliderFlags.AlwaysClamp))
                this.SaveConfiguration(changedValue, setter);
        }

        protected void DragInt(int valueLeft, int valueRight, Action<int> setterLeft, Action<int> setterRight, string label, int speed, int min, int max, string format, float spacing = 0.0f)
        {
            this.DragInt(valueLeft, setterLeft, " ", speed, min, max, format, spacing);
            this.DragInt(valueRight, setterRight, label, speed, min, max, format, ImGui.GetStyle().ItemSpacing.X * ImGuiHelpers.GlobalScale);
        }

        protected void Combo<T>(T value, Action<T> setter, string label)
        {
            var method = typeof(EnumExtensions).GetMethod("GetDescription", BindingFlags.Public | BindingFlags.Static, null, new[] { value.GetType() }, null);
            var options = new List<string>();
            var values = Enum.GetValues(typeof(T));
            foreach (var item in values)
            {
                var description = (string)method.Invoke(null, new[] { item });
                options.Add(description);
            }

            var changedValue = (int)(object)value;
            if (ImGui.Combo(label, ref changedValue, options.ToArray(), options.Count))
                this.SaveConfiguration((T)(object)changedValue, setter);
        }

        protected static void CollapsingHeader(string label, Action action)
        {
            if (ImGui.CollapsingHeader(label, ImGuiTreeNodeFlags.DefaultOpen))
                action();
            ImGui.Dummy(ImGui.GetStyle().ItemSpacing * ImGuiHelpers.GlobalScale);
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