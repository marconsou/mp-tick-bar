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

        private static float ItemSpacingVertical => 8.0f;

        private void SaveConfiguration<T>(T changedValue, Action<T> setter)
        {
            setter(changedValue);
            this.Configuration.Save();
        }

        private static void SameLine(Vector2? sameLinePosition)
        {
            if (sameLinePosition != null)
                ImGui.SameLine(sameLinePosition.Value.X, sameLinePosition.Value.Y);
        }

        private static void PushItemSpacingVar()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0.0f, PluginUI.ItemSpacingVertical));
        }

        protected void CheckBox(bool value, Action<bool> setter, string label, Vector2? sameLinePosition = null)
        {
            PluginUI.PushItemSpacingVar();

            var changedValue = value;
            PluginUI.SameLine(sameLinePosition);
            if (ImGui.Checkbox(label, ref changedValue))
                this.SaveConfiguration(changedValue, setter);

            ImGui.PopStyleVar();
        }

        protected void ColorEdit4(Vector4 value, Action<Vector4> setter, string label, Vector2? sameLinePosition = null)
        {
            PluginUI.PushItemSpacingVar();

            var changedValue = value;
            PluginUI.SameLine(sameLinePosition);
            if (ImGui.ColorEdit4(label, ref changedValue, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar))
                this.SaveConfiguration(changedValue, setter);

            ImGui.PopStyleVar();
        }

        protected void DragFloat(float value, Action<float> setter, string label, float speed, float min, float max, string format, Vector2? sameLinePosition = null)
        {
            PluginUI.PushItemSpacingVar();

            var changedValue = value;
            PluginUI.SameLine(sameLinePosition);
            if (ImGui.DragFloat(label, ref changedValue, speed, min, max, format))
                this.SaveConfiguration(changedValue, setter);

            ImGui.PopStyleVar();
        }

        protected void DragFloat(float valueLeft, float valueRight, Action<float> setterLeft, Action<float> setterRight, string label, float speed, float min, float max, string format, Vector2? sameLinePosition = null)
        {
            this.DragFloat(valueLeft, setterLeft, " ", speed, min, max, format, sameLinePosition);
            this.DragFloat(valueRight, setterRight, label, speed, min, max, format, new(0.0f, PluginUI.ItemSpacingVertical));
        }

        protected void DragInt(int value, Action<int> setter, string label, int speed, int min, int max, string format, Vector2? sameLinePosition = null)
        {
            PluginUI.PushItemSpacingVar();

            var changedValue = value;
            PluginUI.SameLine(sameLinePosition);
            if (ImGui.DragInt(label, ref changedValue, speed, min, max, format))
                this.SaveConfiguration(changedValue, setter);

            ImGui.PopStyleVar();
        }

        protected void DragInt(int valueLeft, int valueRight, Action<int> setterLeft, Action<int> setterRight, string label, int speed, int min, int max, string format, Vector2? sameLinePosition = null)
        {
            this.DragInt(valueLeft, setterLeft, " ", speed, min, max, format, sameLinePosition);
            this.DragInt(valueRight, setterRight, label, speed, min, max, format, new(0.0f, PluginUI.ItemSpacingVertical));
        }

        protected void Combo<T>(T value, Action<T> setter, string label)
        {
            PluginUI.PushItemSpacingVar();

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

            ImGui.PopStyleVar();
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