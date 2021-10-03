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

        protected void CheckBox(bool value, Action<bool> setter, string label, Vector2? sameLinePosition = null)
        {
            var changedValue = value;
            PluginUI.SameLine(sameLinePosition);
            if (ImGui.Checkbox(label, ref changedValue))
                this.SaveConfiguration(changedValue, setter);
        }

        protected void ColorEdit3(Vector3 value, Action<Vector3> setter, string label, Vector2? sameLinePosition = null)
        {
            var changedValue = value;
            PluginUI.SameLine(sameLinePosition);
            if (ImGui.ColorEdit3(label, ref changedValue, ImGuiColorEditFlags.NoInputs))
                this.SaveConfiguration(changedValue, setter);
        }

        protected void DragFloat(float value, Action<float> setter, string label, float speed, float min, float max, string format, Vector2? sameLinePosition = null)
        {
            var changedValue = value;
            PluginUI.SameLine(sameLinePosition);
            if (ImGui.DragFloat(label, ref changedValue, speed, min, max, format))
                this.SaveConfiguration(changedValue, setter);
        }

        protected void DragInt(int value, Action<int> setter, string label, int speed, int min, int max, string format, Vector2? sameLinePosition = null)
        {
            var changedValue = value;
            PluginUI.SameLine(sameLinePosition);
            if (ImGui.DragInt(label, ref changedValue, speed, min, max, format))
                this.SaveConfiguration(changedValue, setter);
        }

        protected void Combo<T>(T value, Action<T> setter, string label)
        {
            var method = typeof(EnumExtensions).GetMethod("GetDescription", BindingFlags.Public | BindingFlags.Static, null, new Type[] { value.GetType() }, null);
            var options = new List<string>();
            var values = Enum.GetValues(typeof(T));
            foreach (var item in values)
            {
                var description = (string)method.Invoke(null, new object[] { item });
                options.Add(description);
            }

            var changedValue = (int)(object)value;
            if (ImGui.Combo(label, ref changedValue, options.ToArray(), options.Count))
                this.SaveConfiguration((T)(object)changedValue, setter);
        }

        protected static void Tooltip(string message)
        {
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text(message);
                ImGui.EndTooltip();
            }
        }

        private static void SameLine(Vector2? sameLinePosition)
        {
            if (sameLinePosition != null)
                ImGui.SameLine(sameLinePosition.Value.X, sameLinePosition.Value.Y);
        }
    }
}