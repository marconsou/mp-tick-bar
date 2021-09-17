using ImGuiNET;
using ImGuiScene;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace MPTickBarApi4
{
    public class PluginUI : IDisposable
    {
        private Configuration Configuration { get; set; }

        private TextureWrap GaugeDefault { get; set; }

        private TextureWrap GaugeMaterialUIBlack { get; set; }

        private TextureWrap GaugeMaterialUIDiscord { get; set; }

        private TextureWrap JobStackDefault { get; set; }

        private TextureWrap JobStackMaterialUI { get; set; }

        public bool IsConfigurationWindowVisible { get; set; }

        public bool IsMPTickBarVisible { get; set; }

        public bool IsMpTickBarProgressResumed { get; set; }

        public bool IsUmbralIceIIIActivated { get; set; }

        private bool IsCircleOfPowerPreviewActivated { get; set; }

        public double ProgressTime { get; set; }

        public float FireIIICastTime { get; set; }

        private Vector2 ItemSpacingDefault { get; set; }

        private struct MPTickBarUI
        {
            public TextureWrap Gauge { get; set; }

            public TextureWrap JobStack { get; set; }
        }

        public PluginUI(Configuration configuration, TextureWrap gaugeDefault, TextureWrap gaugeMaterialUIBlack, TextureWrap gaugeMaterialUIDiscord, TextureWrap jobStackDefault, TextureWrap jobStackMaterialUI)
        {
            this.Configuration = configuration;
            this.GaugeDefault = gaugeDefault;
            this.GaugeMaterialUIBlack = gaugeMaterialUIBlack;
            this.GaugeMaterialUIDiscord = gaugeMaterialUIDiscord;
            this.JobStackDefault = jobStackDefault;
            this.JobStackMaterialUI = jobStackMaterialUI;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            this.GaugeDefault.Dispose();
            this.GaugeMaterialUIBlack.Dispose();
            this.GaugeMaterialUIDiscord.Dispose();
            this.JobStackDefault.Dispose();
            this.JobStackMaterialUI.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Draw()
        {
            this.DrawMPTickBarWindow();
            this.DrawConfigurationWindow();
        }

        private void PushStyleItemSpacing()
        {
            var style = ImGui.GetStyle();
            this.ItemSpacingDefault = new Vector2(style.ItemSpacing.X, style.ItemSpacing.Y);
            style.ItemSpacing = new Vector2(10.0f, 10.0f);
        }

        private void PopStyleItemSpacing()
        {
            ImGui.GetStyle().ItemSpacing = this.ItemSpacingDefault;
        }

        private static void RenderGaugeUIElement(MPTickBarUI mpTickBarUI, float uiScale, float offsetY, float elementWidth, float progress, int uiNumber, Vector4 tintColor)
        {
            var startX = 0.0f;
            var startY = offsetY;

            var elementHeight = 20.0f;
            var scaledElementWidth = elementWidth * uiScale;
            var scaledElementHeight = elementHeight * uiScale;

            var textureElementHeight = 40.0f;
            var textureY = (textureElementHeight * uiNumber) / mpTickBarUI.Gauge.Height;
            var textureH = (textureElementHeight * (uiNumber + 1)) / mpTickBarUI.Gauge.Height;
            ImGui.SetCursorPos(new Vector2(startX, startY));
            ImGui.Image(mpTickBarUI.Gauge.ImGuiHandle, new Vector2(scaledElementWidth * progress, scaledElementHeight), new Vector2(0.0f, textureY), new Vector2(progress, textureH), tintColor);
        }

        private static void RenderJobStackUIElement(MPTickBarUI mpTickBarUI, float uiScale, float startX, float startY, int uiNumber, Vector4 tintColor)
        {
            var textureX = uiNumber * 0.5f;
            var scaledElementWidth = 20.0f * uiScale;
            var scaledElementHeight = 20.0f * uiScale;
            ImGui.SetCursorPos(new Vector2(startX, startY));
            ImGui.Image(mpTickBarUI.JobStack.ImGuiHandle, new Vector2(scaledElementWidth, scaledElementHeight), new Vector2(textureX, 0.0f), new Vector2(textureX + 0.5f, 0.5f), tintColor);
        }

        private void DrawMPTickBar(bool isPreview)
        {
            var mpTickBarUI = this.GetMPTickBarUI();
            var progress = (float)(!isPreview ? (this.IsMpTickBarProgressResumed ? (this.ProgressTime % 1) : 0.0) : (((DateTime.Now.Second % 3) + (DateTime.Now.Millisecond / 1000.0)) / 3.0));
            var uiScale = !isPreview ? this.Configuration.UIScale : 2.0f;
            var offsetY = !isPreview ? 25.0f : 0.0f;
            var elementWidth = 160.0f;

            PluginUI.RenderGaugeUIElement(mpTickBarUI, uiScale, offsetY, elementWidth, 1.0f, 5, Vector4.One);
            PluginUI.RenderGaugeUIElement(mpTickBarUI, uiScale, offsetY, elementWidth, progress, 2, new Vector4(this.Configuration.ProgressBarTintColor, 1.0f));
            PluginUI.RenderGaugeUIElement(mpTickBarUI, uiScale, offsetY, elementWidth, 1.0f, 0, Vector4.One);

            if (this.Configuration.IsFastFireIIIMarkerVisible && (this.IsUmbralIceIIIActivated || isPreview))
            {
                elementWidth = (elementWidth) / 3.0f;
                var fireIIICastTime = (!isPreview) ? this.FireIIICastTime : PlayerHelpers.CalculatedFireIIICastTime(this.Configuration.FireIIICastTime, true, this.IsCircleOfPowerPreviewActivated);
                var FireIIICastOffset = (3.0f - fireIIICastTime) * elementWidth;
                var fastFireIIIMarkerOffset = FireIIICastOffset + (this.Configuration.FastFireIIIMarkerOffset * 0.70f);

                if (this.Configuration.FastFireIIIMarkerType == FastFireIIIMarkerType.Icon)
                {
                    var startX = (fastFireIIIMarkerOffset * uiScale);
                    var startY = offsetY - (this.Configuration.UIType == UIType.FinalFantasyXIVDefault ? 0.0f : 0.5f * uiScale);

                    PluginUI.RenderJobStackUIElement(mpTickBarUI, uiScale, startX, startY, 0, Vector4.One);
                    PluginUI.RenderJobStackUIElement(mpTickBarUI, uiScale, startX, startY, 1, new Vector4(this.Configuration.FastFireIIIMarkerTintColor, 1.0f));
                }
                else if (this.Configuration.FastFireIIIMarkerType == FastFireIIIMarkerType.Line)
                {
                    var windowPos = ImGui.GetWindowPos();
                    var lineMarkerStartX = 10.0f;
                    var startX = windowPos.X + ((lineMarkerStartX + fastFireIIIMarkerOffset) * uiScale);
                    var startY = windowPos.Y + (5.35f * uiScale) + offsetY;
                    var lineHeight = 8.0f * uiScale;
                    var thickness = 5.0f * uiScale;
                    ImGui.GetWindowDrawList().AddLine(new Vector2(startX, startY), new Vector2(startX, startY + lineHeight), ImGui.GetColorU32(new Vector4(this.Configuration.FastFireIIIMarkerTintColor, 1.0f)), thickness);
                }
            }
        }

        private void DrawConfiguration()
        {
            this.PushStyleItemSpacing();
            if (ImGui.BeginTabBar("BeginTabBar"))
            {
                if (ImGui.BeginTabItem("MP Tick Bar"))
                {
                    this.DrawMPTickBarTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Fast Fire III Marker"))
                {
                    this.DrawFastFireIIIMarkerTab();
                    ImGui.EndTabItem();
                }
            }
            this.PopStyleItemSpacing();
        }

        private void DrawMPTickBarTab()
        {
            var isMPTickBarVisible = this.Configuration.IsMPTickBarVisible;
            if (ImGui.Checkbox("Show", ref isMPTickBarVisible))
            {
                this.Configuration.IsMPTickBarVisible = isMPTickBarVisible;
                this.Configuration.Save();
            }

            var isMPTickBarLocked = this.Configuration.IsMPTickBarLocked;
            ImGui.SameLine(0.0f, 20.0f);
            if (ImGui.Checkbox("Lock", ref isMPTickBarLocked))
            {
                this.Configuration.IsMPTickBarLocked = isMPTickBarLocked;
                this.Configuration.Save();
            }

            var progressBarTintColor = this.Configuration.ProgressBarTintColor;
            ImGui.SameLine(0.0f, 20.0f);
            if (ImGui.ColorEdit3("Progress Bar", ref progressBarTintColor, ImGuiColorEditFlags.NoInputs))
            {
                this.Configuration.ProgressBarTintColor = progressBarTintColor;
                this.Configuration.Save();
            }

            var uiScale = this.Configuration.UIScale;
            if (ImGui.DragFloat("UI Scale", ref uiScale, 0.1f, 1.0f, 5.0f, "%.1f"))
            {
                this.Configuration.UIScale = uiScale;
                this.Configuration.Save();
            }

            var uiTypeItems = new List<string>();
            foreach (UIType item in Enum.GetValues(typeof(UIType)))
                uiTypeItems.Add(item.GetDescription());

            var uiType = (int)this.Configuration.UIType;
            if (ImGui.Combo("UI", ref uiType, uiTypeItems.ToArray(), uiTypeItems.Count))
            {
                this.Configuration.UIType = (UIType)uiType;
                this.Configuration.Save();
            }
        }

        private void DrawFastFireIIIMarkerTab()
        {
            var isFastFireIIIMarkerVisible = this.Configuration.IsFastFireIIIMarkerVisible;
            if (ImGui.Checkbox("Show", ref isFastFireIIIMarkerVisible))
            {
                this.Configuration.IsFastFireIIIMarkerVisible = isFastFireIIIMarkerVisible;
                this.Configuration.Save();
            }

            var fastFireIIIMarkerTintColor = this.Configuration.FastFireIIIMarkerTintColor;
            ImGui.SameLine(0.0f, 20.0f);
            if (ImGui.ColorEdit3("Marker", ref fastFireIIIMarkerTintColor, ImGuiColorEditFlags.NoInputs))
            {
                this.Configuration.FastFireIIIMarkerTintColor = fastFireIIIMarkerTintColor;
                this.Configuration.Save();
            }

            var fastFireIIIMarkerOffset = this.Configuration.FastFireIIIMarkerOffset;
            if (ImGui.DragInt("Offset", ref fastFireIIIMarkerOffset, 1, 0, 50, "%i"))
            {
                this.Configuration.FastFireIIIMarkerOffset = fastFireIIIMarkerOffset;
                this.Configuration.Save();
            }

            var fireIIICastTime = this.Configuration.FireIIICastTime;
            if (ImGui.DragFloat("Fire III Cast Time", ref fireIIICastTime, 0.01f, 2.95f, 3.5f, "%.2f"))
            {
                this.Configuration.FireIIICastTime = fireIIICastTime;
                this.Configuration.Save();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.TextUnformatted("This value will be calculated automatically on the next Dalamud API update.");
                ImGui.EndTooltip();
            }

            var fastFireIIIMarkerTypeItems = new List<string>();
            foreach (FastFireIIIMarkerType item in Enum.GetValues(typeof(FastFireIIIMarkerType)))
                fastFireIIIMarkerTypeItems.Add(item.GetDescription());

            var fastFireIIIMarkerType = (int)this.Configuration.FastFireIIIMarkerType;
            if (ImGui.Combo("Style", ref fastFireIIIMarkerType, fastFireIIIMarkerTypeItems.ToArray(), fastFireIIIMarkerTypeItems.Count))
            {
                this.Configuration.FastFireIIIMarkerType = (FastFireIIIMarkerType)fastFireIIIMarkerType;
                this.Configuration.Save();
            }
        }

        private void DrawMPTickBarWindow()
        {
            var isMPTickBarVisible = this.Configuration.IsMPTickBarVisible;
            if (!isMPTickBarVisible || !this.IsMPTickBarVisible)
                return;

            ImGuiWindowFlags windowFlags = this.Configuration.IsMPTickBarLocked ?
                ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse |
                ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoDecoration |
                ImGuiWindowFlags.NoBackground |
                ImGuiWindowFlags.NoMouseInputs |
                ImGuiWindowFlags.NoFocusOnAppearing |
                ImGuiWindowFlags.NoBringToFrontOnFocus |
                ImGuiWindowFlags.NoNavInputs |
                ImGuiWindowFlags.NoNavFocus |
                ImGuiWindowFlags.NoNav |
                ImGuiWindowFlags.NoInputs |
                ImGuiWindowFlags.NoDocking
                :
                ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse |
                ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoBackground;

            if (ImGui.Begin("MP Tick Bar", ref isMPTickBarVisible, windowFlags))
            {
                this.Configuration.IsMPTickBarVisible = isMPTickBarVisible;
                this.DrawMPTickBar(false);
                ImGui.End();
            }
        }

        private void DrawConfigurationWindow()
        {
            var isConfigurationWindowVisible = this.IsConfigurationWindowVisible;
            if (!isConfigurationWindowVisible)
                return;

            ImGuiWindowFlags windowFlags =
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse |
                ImGuiWindowFlags.NoCollapse;

            ImGui.SetNextWindowSize(new Vector2(337.0f, 299.0f), ImGuiCond.Always);
            if (ImGui.Begin("MP Tick Bar Configuration", ref isConfigurationWindowVisible, windowFlags))
            {
                ImGui.Text("Preview:");
                if (ImGui.BeginChild("Child", new Vector2(0.0f, 0.0f), false, windowFlags))
                {
                    this.PushStyleItemSpacing();
                    this.DrawMPTickBar(true);
                    var isCircleOfPowerPreviewActivated = this.IsCircleOfPowerPreviewActivated;
                    var fireIIICastTime = PlayerHelpers.CalculatedFireIIICastTime(this.Configuration.FireIIICastTime, true, this.IsCircleOfPowerPreviewActivated);
                    if (ImGui.Checkbox($"Circle of Power ({Math.Round(fireIIICastTime, 2)}s)", ref isCircleOfPowerPreviewActivated))
                    {
                        this.IsCircleOfPowerPreviewActivated = isCircleOfPowerPreviewActivated;
                    }
                    this.PopStyleItemSpacing();
                    ImGui.EndChild();
                }

                if (ImGui.BeginChild("Child", new Vector2(0.0f, 0.0f), false, windowFlags))
                {
                    this.DrawConfiguration();
                    ImGui.EndChild();
                }
                this.IsConfigurationWindowVisible = isConfigurationWindowVisible;
                ImGui.End();
            }
        }

        private MPTickBarUI GetMPTickBarUI()
        {
            MPTickBarUI ui = new();
            switch (this.Configuration.UIType)
            {
                case UIType.FinalFantasyXIVDefault:
                    ui.Gauge = this.GaugeDefault;
                    ui.JobStack = this.JobStackDefault;
                    break;
                case UIType.MaterialUIDiscord:
                    ui.Gauge = this.GaugeMaterialUIDiscord;
                    ui.JobStack = this.JobStackMaterialUI;
                    break;
                case UIType.MaterialUIBlack:
                    ui.Gauge = this.GaugeMaterialUIBlack;
                    ui.JobStack = this.JobStackMaterialUI;
                    break;
            }
            return ui;
        }
    }
}