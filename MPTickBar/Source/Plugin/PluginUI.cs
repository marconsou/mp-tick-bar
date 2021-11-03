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

        private GroupPanelData GroupPanel { get; set; }

        private static float ItemSpacingVertical => 8.0f;

        private readonly struct GroupPanelData
        {
            public string Text { get; init; }

            public Vector2 Offset { get; init; }

            public Vector2 Padding { get; init; }

            public Vector2 Position { get; init; }

            public Vector2 BackgroundPosition { get; init; }
        }

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
            if (ImGui.ColorEdit4(label, ref changedValue, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.NoTooltip))
                this.SaveConfiguration(changedValue, setter);

            ImGui.PopStyleVar();
        }

        protected void DragFloat(float value, Action<float> setter, string label, float speed, float min, float max, string format, float width, Vector2? sameLinePosition = null)
        {
            PluginUI.PushItemSpacingVar();

            var changedValue = value;
            PluginUI.SameLine(sameLinePosition);
            ImGui.SetNextItemWidth(width);
            if (ImGui.DragFloat(label, ref changedValue, speed, min, max, format))
                this.SaveConfiguration(changedValue, setter);

            ImGui.PopStyleVar();
        }

        protected void DragFloat(float valueLeft, float valueRight, Action<float> setterLeft, Action<float> setterRight, string label, float speed, float min, float max, string format, float width, Vector2? sameLinePosition = null)
        {
            this.DragFloat(valueLeft, setterLeft, "", speed, min, max, format, width, sameLinePosition);
            this.DragFloat(valueRight, setterRight, label, speed, min, max, format, width, new(0.0f, PluginUI.ItemSpacingVertical));
        }

        protected void DragInt(int value, Action<int> setter, string label, int speed, int min, int max, string format, float width, Vector2? sameLinePosition = null)
        {
            PluginUI.PushItemSpacingVar();

            var changedValue = value;
            PluginUI.SameLine(sameLinePosition);
            ImGui.SetNextItemWidth(width);
            if (ImGui.DragInt(label, ref changedValue, speed, min, max, format))
                this.SaveConfiguration(changedValue, setter);

            ImGui.PopStyleVar();
        }

        protected void DragInt(int valueLeft, int valueRight, Action<int> setterLeft, Action<int> setterRight, string label, int speed, int min, int max, string format, float width, Vector2? sameLinePosition = null)
        {
            this.DragInt(valueLeft, setterLeft, "", speed, min, max, format, width, sameLinePosition);
            this.DragInt(valueRight, setterRight, label, speed, min, max, format, width, new(0.0f, PluginUI.ItemSpacingVertical));
        }

        protected void Combo<T>(T value, Action<T> setter, string label, float width)
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
            ImGui.SetNextItemWidth(width);
            if (ImGui.Combo(label, ref changedValue, options.ToArray(), options.Count))
                this.SaveConfiguration((T)(object)changedValue, setter);

            ImGui.PopStyleVar();
        }

        protected bool BeginGroupPanel(string strId, string label, int numberOfLines)
        {
            this.GroupPanel = new()
            {
                Text = label,
                Offset = new(PluginUI.ItemSpacingVertical, 0.0f),
                Padding = new(4.0f, 2.0f),
                Position = ImGui.GetCursorPos(),
                BackgroundPosition = ImGui.GetCursorScreenPos()
            };

            ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(0.0f, PluginUI.ItemSpacingVertical));
            if (ImGui.BeginChild($"{strId}.{label}", new(0.0f, 20.0f + (numberOfLines * 31.0f)), true))
            {
                ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(0.0f, 11.0f));
                return true;
            }
            return false;
        }

        protected void EndGroupPanel()
        {
            ImGui.EndChild();

            var textSize = ImGui.CalcTextSize(this.GroupPanel.Text);
            var textBgPos = this.GroupPanel.BackgroundPosition + this.GroupPanel.Offset;
            var xMin = textBgPos.X;
            var yMin = textBgPos.Y - this.GroupPanel.Padding.Y;
            var xMax = textBgPos.X + textSize.X + (this.GroupPanel.Padding.X * 2.0f);
            var yMax = textBgPos.Y + textSize.Y + this.GroupPanel.Padding.Y;
            var color = ImGui.GetStyle().Colors[(int)ImGuiCol.TabActive];
            var rounding = ImGui.GetStyle().TabRounding;
            ImGui.GetWindowDrawList().AddRectFilled(new(xMin, yMin), new(xMax, yMax), ImGui.GetColorU32(new Vector4(color.X, color.Y, color.Z, 1.0f)), rounding);

            var nextCursorPos = ImGui.GetCursorPos();
            ImGui.SetCursorPosX(this.GroupPanel.Position.X + this.GroupPanel.Offset.X + this.GroupPanel.Padding.X);
            ImGui.SetCursorPosY(this.GroupPanel.Position.Y + this.GroupPanel.Offset.Y);
            ImGui.Text(this.GroupPanel.Text);
            ImGui.SetCursorPos(nextCursorPos);
        }

        protected static void Tooltip(string message, float width = 400.0f)
        {
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(width);
                ImGui.Text(message);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }
    }
}