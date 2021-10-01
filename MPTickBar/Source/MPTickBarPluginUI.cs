using ImGuiNET;
using ImGuiScene;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace MPTickBar
{
    public sealed class MPTickBarPluginUI : IDisposable
    {
        private Configuration Configuration { get; set; }

        private TextureWrap GaugeDefault { get; set; }

        private TextureWrap GaugeMaterialUIBlack { get; set; }

        private TextureWrap GaugeMaterialUIDiscord { get; set; }

        private TextureWrap JobStackDefault { get; set; }

        private TextureWrap JobStackMaterialUI { get; set; }

        private TextureWrap NumberPercentage { get; set; }

        public int Level { get; set; }

        public bool IsPlayingAsBLM { get; set; }

        public bool IsConfigurationWindowVisible { get; set; }

        public bool IsMPTickBarVisible { get; set; }

        public bool IsCircleOfPowerActivated { get; set; }

        private bool IsCircleOfPowerPreviewActivated { get; set; }

        public bool IsUmbralIceIIIActivated { get; set; }

        public double ProgressTime { get; set; }

        public float FireIIICastTime { get; set; }

        private Vector2 ItemSpacingDefault { get; set; }

        private List<RegressEffect> RegressEffects { get; set; } = new List<RegressEffect>();

        private class MPTickBarUI
        {
            public TextureWrap Gauge { get; set; }

            public TextureWrap JobStack { get; set; }
        }

        private class RegressEffect
        {
            public bool IsRegressing => this.Regress > 0.0f;

            public float Regress { get; private set; }

            private float LastProgress { get; set; }

            private double CurrentTime { get; set; }

            private double LastTime { get; set; }

            public void CheckForStartRegressing(float progress)
            {
                if (progress < this.LastProgress)
                    this.Regress = this.LastProgress;
            }

            public void Update(float progress)
            {
                this.CurrentTime = ImGui.GetTime();
                if (this.IsRegressing)
                {
                    var speedScale = 3.0f;
                    var incrementedTime = this.CurrentTime - this.LastTime;
                    this.Regress -= (float)incrementedTime * speedScale;
                    this.Regress = Math.Max(this.Regress, 0.0f);
                }
                this.LastProgress = progress;
                this.LastTime = this.CurrentTime;
            }
        }

        public MPTickBarPluginUI(Configuration configuration, TextureWrap gaugeDefault, TextureWrap gaugeMaterialUIBlack, TextureWrap gaugeMaterialUIDiscord, TextureWrap jobStackDefault, TextureWrap jobStackMaterialUI, TextureWrap numberPercentage)
        {
            this.Configuration = configuration;
            this.GaugeDefault = gaugeDefault;
            this.GaugeMaterialUIBlack = gaugeMaterialUIBlack;
            this.GaugeMaterialUIDiscord = gaugeMaterialUIDiscord;
            this.JobStackDefault = jobStackDefault;
            this.JobStackMaterialUI = jobStackMaterialUI;
            this.NumberPercentage = numberPercentage;

            this.RegressEffects.Add(new RegressEffect());
            this.RegressEffects.Add(new RegressEffect());
        }

        public void Dispose()
        {
            this.GaugeDefault.Dispose();
            this.GaugeMaterialUIBlack.Dispose();
            this.GaugeMaterialUIDiscord.Dispose();
            this.JobStackDefault.Dispose();
            this.JobStackMaterialUI.Dispose();
            this.NumberPercentage.Dispose();
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

        private static void RenderGaugeUIElement(MPTickBarUI mpTickBarUI, float uiScale, float offsetX, float offsetY, float elementWidth, float progress, int uiNumber, Vector4 tintColor, bool isBar, bool isProgress = false)
        {
            var progressBarTexturePaddingX = 12.0f;
            var progressBarTextureOffsetX = isBar ? (progressBarTexturePaddingX * (elementWidth / mpTickBarUI.Gauge.Width)) : 0.0f;
            if (isBar)
                elementWidth -= (progressBarTextureOffsetX * 2.0f);
            var elementHeight = 20.0f;

            var textureElementHeight = 40.0f;
            var textureX = isBar ? (progressBarTexturePaddingX / mpTickBarUI.Gauge.Width) : 0.0f;
            var textureY = (textureElementHeight * uiNumber) / mpTickBarUI.Gauge.Height;
            var textureW = isBar ? (((!isProgress ? (textureX + ((1.0f - (textureX * 2.0f)) * progress)) : (1.0f - textureX)))) : 1.0f;
            var textureH = (textureElementHeight * (uiNumber + 1)) / mpTickBarUI.Gauge.Height;

            var startX = offsetX + (isBar ? (progressBarTextureOffsetX * uiScale) : 0.0f);
            var startY = offsetY;
            var width = elementWidth * uiScale * progress;
            var height = elementHeight * uiScale;
            ImGui.SetCursorPos(new Vector2(startX, startY));
            ImGui.Image(mpTickBarUI.Gauge.ImGuiHandle, new Vector2(width, height), new Vector2(textureX, textureY), new Vector2(textureW, textureH), !isProgress ? tintColor : Vector4.One);
        }

        private void RenderRegressEffect(MPTickBarUI mpTickBarUI, float uiScale, float offsetX, float offsetY, float elementWidth, float progress, bool isPreview)
        {
            var regressEffectIndex = !isPreview ? 0 : 1;
            var regressEffects = this.RegressEffects[regressEffectIndex];

            regressEffects.CheckForStartRegressing(progress);

            if (regressEffects.IsRegressing)
            {
                if (this.Configuration.IsRegressEffectVisible)
                    MPTickBarPluginUI.RenderGaugeUIElement(mpTickBarUI, uiScale, offsetX, offsetY, elementWidth, regressEffects.Regress, 4, Vector4.One, true, true);
            }
            regressEffects.Update(progress);
        }

        private static void RenderJobStackUIElement(MPTickBarUI mpTickBarUI, float uiScale, float startX, float startY, int uiNumber, Vector4 tintColor)
        {
            var textureX = uiNumber * 0.5f;
            var scaledElementWidth = 20.0f * uiScale;
            var scaledElementHeight = 20.0f * uiScale;
            ImGui.SetCursorPos(new Vector2(startX, startY));
            ImGui.Image(mpTickBarUI.JobStack.ImGuiHandle, new Vector2(scaledElementWidth, scaledElementHeight), new Vector2(textureX, 0.0f), new Vector2(textureX + 0.5f, 0.5f), tintColor);
        }

        private void RenderNumbers(float uiScale, float offsetX, float offsetY, float progress, float barElementWidth)
        {
            var digitTotal = 10;
            var elementWidth = this.NumberPercentage.Width / digitTotal;
            var elementHeight = this.NumberPercentage.Height;
            var scaledElementWidth = elementWidth * uiScale;
            var scaledElementHeight = elementHeight * uiScale;
            var percentage = (int)(progress * 100.0f);
            var percentageText = percentage.ToString();
            var baseX = this.Configuration.NumberPercentageOffsetX + (barElementWidth / 2.0f) - (percentageText.Length * elementWidth / 2.0f);
            var baseY = this.Configuration.NumberPercentageOffsetY + 11.0f;
            var digitOffsetX = 0.0f;
            foreach (var digitText in percentageText)
            {
                var digit = char.GetNumericValue(digitText);
                var baseTextureX = (elementWidth) * digit;
                var textureX = (float)baseTextureX / this.NumberPercentage.Width;
                var textureW = (float)(baseTextureX + elementWidth) / this.NumberPercentage.Width;
                ImGui.SetCursorPos(new Vector2((baseX + digitOffsetX + offsetX / uiScale) * uiScale, (baseY + offsetY / uiScale) * uiScale));
                ImGui.Image(this.NumberPercentage.ImGuiHandle, new Vector2(scaledElementWidth, scaledElementHeight), new Vector2(textureX, 0.0f), new Vector2(textureW, 1.0f), new Vector4(this.Configuration.NumberPercentageTintColor, 1.0f));

                digitOffsetX += (elementWidth);
            }
        }

        private void DrawMPTickBar(bool isPreview)
        {
            var mpTickBarUI = this.GetMPTickBarUI();
            var progress = (float)(!isPreview ? (this.ProgressTime % 1) : (((DateTime.Now.Second % 3) + (DateTime.Now.Millisecond / 1000.0)) / 3.0));
            var uiScale = !isPreview ? this.Configuration.UIScale : 2.0f;
            var offsetX = !isPreview ? 4.0f : 0.0f;
            var offsetY = !isPreview ? (25.0f * uiScale) : 40.0f;
            var barElementWidth = 160.0f;
            var elementWidth = barElementWidth;

            MPTickBarPluginUI.RenderGaugeUIElement(mpTickBarUI, uiScale, offsetX, offsetY, elementWidth, 1.0f, 5, Vector4.One, false);
            this.RenderRegressEffect(mpTickBarUI, uiScale, offsetX, offsetY, elementWidth, progress, isPreview);
            MPTickBarPluginUI.RenderGaugeUIElement(mpTickBarUI, uiScale, offsetX, offsetY, elementWidth, progress, 2, new Vector4(this.Configuration.ProgressBarTintColor, 1.0f), true);
            MPTickBarPluginUI.RenderGaugeUIElement(mpTickBarUI, uiScale, offsetX, offsetY, elementWidth, 1.0f, 0, Vector4.One, false);

            if ((this.Configuration.FastFireIIIMarkerVisibility == FastFireIIIMarkerVisibility.Visible) ||
               ((this.Configuration.FastFireIIIMarkerVisibility == FastFireIIIMarkerVisibility.InUmbralIceIII) && this.IsUmbralIceIIIActivated) ||
               ((this.Configuration.FastFireIIIMarkerVisibility != FastFireIIIMarkerVisibility.Hidden) && isPreview))
            {
                elementWidth = (elementWidth) / 3.0f;
                var fireIIICastTime = (!isPreview) ? this.FireIIICastTime : PlayerHelpers.GetFastFireIIICastTime(this.Level, this.IsCircleOfPowerPreviewActivated);
                var FireIIICastOffset = (3.0f - fireIIICastTime) * elementWidth;
                var fastFireIIIMarkerOffset = FireIIICastOffset + (this.Configuration.FastFireIIIMarkerTimeOffset * elementWidth);

                if (this.Configuration.FastFireIIIMarkerType == FastFireIIIMarkerType.Icon)
                {
                    var startX = offsetX + (fastFireIIIMarkerOffset * uiScale);
                    var startY = offsetY - (this.Configuration.UIType == UIType.FinalFantasyXIVDefault ? 0.0f : 0.5f * uiScale);

                    MPTickBarPluginUI.RenderJobStackUIElement(mpTickBarUI, uiScale, startX, startY, 0, Vector4.One);
                    MPTickBarPluginUI.RenderJobStackUIElement(mpTickBarUI, uiScale, startX, startY, 1, new Vector4(this.Configuration.FastFireIIIMarkerTintColor, 1.0f));
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

            if ((this.Configuration.NumberPercentageVisibility == NumberPercentageVisibility.Visible) ||
               ((this.Configuration.NumberPercentageVisibility == NumberPercentageVisibility.InUmbralIceIII) && this.IsUmbralIceIIIActivated) ||
               ((this.Configuration.NumberPercentageVisibility != NumberPercentageVisibility.Hidden) && isPreview))
                this.RenderNumbers(uiScale, offsetX, offsetY, progress, barElementWidth);
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

                if (ImGui.BeginTabItem("Number (%)"))
                {
                    this.DrawNumberPercentageTab();
                    ImGui.EndTabItem();
                }
            }
            this.PopStyleItemSpacing();
        }

        private void DrawMPTickBarTab()
        {
            var isMPTickBarLocked = this.Configuration.IsMPTickBarLocked;
            if (ImGui.Checkbox("Lock", ref isMPTickBarLocked))
            {
                this.Configuration.IsMPTickBarLocked = isMPTickBarLocked;
                this.Configuration.Save();
            }

            var isRegressEffectVisible = this.Configuration.IsRegressEffectVisible;
            ImGui.SameLine(0.0f, 20.0f);
            if (ImGui.Checkbox("Regress Effect", ref isRegressEffectVisible))
            {
                this.Configuration.IsRegressEffectVisible = isRegressEffectVisible;
                this.Configuration.Save();
            }

            var progressBarTintColor = this.Configuration.ProgressBarTintColor;
            ImGui.SameLine(0.0f, 20.0f);
            if (ImGui.ColorEdit3("Progress Bar", ref progressBarTintColor, ImGuiColorEditFlags.NoInputs))
            {
                this.Configuration.ProgressBarTintColor = progressBarTintColor;
                this.Configuration.Save();
            }

            var mpTickBarVisibilityItems = new List<string>();
            foreach (MPTickBarVisibility item in Enum.GetValues(typeof(MPTickBarVisibility)))
                mpTickBarVisibilityItems.Add(item.GetDescription());

            var mpTickBarVisibility = (int)this.Configuration.MPTickBarVisibility;
            if (ImGui.Combo("Visibility", ref mpTickBarVisibility, mpTickBarVisibilityItems.ToArray(), mpTickBarVisibilityItems.Count))
            {
                this.Configuration.MPTickBarVisibility = (MPTickBarVisibility)mpTickBarVisibility;
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

            var uiScale = this.Configuration.UIScale;
            if (ImGui.DragFloat("UI Scale", ref uiScale, 0.1f, 1.0f, 5.0f, "%.1f"))
            {
                this.Configuration.UIScale = uiScale;
                this.Configuration.Save();
            }
        }

        private void DrawFastFireIIIMarkerTab()
        {
            var fastFireIIIMarkerTintColor = this.Configuration.FastFireIIIMarkerTintColor;
            if (ImGui.ColorEdit3("Marker", ref fastFireIIIMarkerTintColor, ImGuiColorEditFlags.NoInputs))
            {
                this.Configuration.FastFireIIIMarkerTintColor = fastFireIIIMarkerTintColor;
                this.Configuration.Save();
            }

            var fastFireIIIMarkerVisibilityItems = new List<string>();
            foreach (FastFireIIIMarkerVisibility item in Enum.GetValues(typeof(FastFireIIIMarkerVisibility)))
                fastFireIIIMarkerVisibilityItems.Add(item.GetDescription());

            var fastFireIIIMarkerVisibility = (int)this.Configuration.FastFireIIIMarkerVisibility;
            if (ImGui.Combo("Visibility", ref fastFireIIIMarkerVisibility, fastFireIIIMarkerVisibilityItems.ToArray(), fastFireIIIMarkerVisibilityItems.Count))
            {
                this.Configuration.FastFireIIIMarkerVisibility = (FastFireIIIMarkerVisibility)fastFireIIIMarkerVisibility;
                this.Configuration.Save();
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

            var fastFireIIIMarkerTimeOffset = this.Configuration.FastFireIIIMarkerTimeOffset;
            if (ImGui.DragFloat("Time Offset (s)", ref fastFireIIIMarkerTimeOffset, 0.01f, 0.0f, 0.5f, "%.2f"))
            {
                this.Configuration.FastFireIIIMarkerTimeOffset = fastFireIIIMarkerTimeOffset;
                this.Configuration.Save();
            }
        }

        private void DrawNumberPercentageTab()
        {
            var numberPercentageTintColor = this.Configuration.NumberPercentageTintColor;
            if (ImGui.ColorEdit3("Number (%)", ref numberPercentageTintColor, ImGuiColorEditFlags.NoInputs))
            {
                this.Configuration.NumberPercentageTintColor = numberPercentageTintColor;
                this.Configuration.Save();
            }

            var numberPercentageVisibilityItems = new List<string>();
            foreach (NumberPercentageVisibility item in Enum.GetValues(typeof(NumberPercentageVisibility)))
                numberPercentageVisibilityItems.Add(item.GetDescription());

            var numberPercentageVisibility = (int)this.Configuration.NumberPercentageVisibility;
            if (ImGui.Combo("Visibility", ref numberPercentageVisibility, numberPercentageVisibilityItems.ToArray(), numberPercentageVisibilityItems.Count))
            {
                this.Configuration.NumberPercentageVisibility = (NumberPercentageVisibility)numberPercentageVisibility;
                this.Configuration.Save();
            }

            var numberPercentageOffsetX = this.Configuration.NumberPercentageOffsetX;
            if (ImGui.DragInt("Offset X", ref numberPercentageOffsetX, 1, -50, 50, "%i"))
            {
                this.Configuration.NumberPercentageOffsetX = numberPercentageOffsetX;
                this.Configuration.Save();
            }

            var numberPercentageOffsetY = this.Configuration.NumberPercentageOffsetY;
            if (ImGui.DragInt("Offset Y", ref numberPercentageOffsetY, 1, -30, 10, "%i"))
            {
                this.Configuration.NumberPercentageOffsetY = numberPercentageOffsetY;
                this.Configuration.Save();
            }
        }

        private void DrawMPTickBarWindow()
        {
            var isMPTickBarVisible = this.IsMPTickBarVisible;
            if (!isMPTickBarVisible)
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
                ImGuiWindowFlags.NoTitleBar;

            if (ImGui.Begin("MP Tick Bar", ref isMPTickBarVisible, windowFlags))
            {
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

            ImGui.SetNextWindowSize(new Vector2(339.0f, 399.0f), ImGuiCond.Always);
            if (ImGui.Begin("MP Tick Bar configuration", ref isConfigurationWindowVisible, windowFlags))
            {
                static string CastTimeFormat(bool isPlayingAsBLM, float castTime)
                {
                    return isPlayingAsBLM ? (((int)(castTime * 100)) / 100.0f).ToString("0.00s") : "?";
                }

                ImGui.Text("Preview:");
                if (ImGui.BeginChild("Child Preview", new Vector2(320.0f * 3, 120.0f), false, windowFlags))
                {
                    if (this.IsPlayingAsBLM)
                        this.DrawMPTickBar(true);
                    ImGui.EndChild();
                }

                this.PushStyleItemSpacing();
                var isCircleOfPowerPreviewActivated = this.IsCircleOfPowerPreviewActivated;
                var previewFastFireIIICastTime = PlayerHelpers.GetFastFireIIICastTime(this.Level, this.IsCircleOfPowerPreviewActivated);
                if (ImGui.Checkbox($"Circle of Power (Preview): {CastTimeFormat(this.IsPlayingAsBLM, previewFastFireIIICastTime)}", ref isCircleOfPowerPreviewActivated))
                {
                    this.IsCircleOfPowerPreviewActivated = isCircleOfPowerPreviewActivated;
                }
                var currentFastFireIIICastTime = PlayerHelpers.GetFastFireIIICastTime(this.Level, this.IsCircleOfPowerActivated);
                var text = "Fast Fire III cast time (Current): ";
                ImGui.Text(text);
                ImGui.SameLine(ImGui.CalcTextSize(text).X + 7.0f, 0.0f);
                ImGui.TextColored(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), $"{CastTimeFormat(this.IsPlayingAsBLM, currentFastFireIIICastTime)}");
                this.PopStyleItemSpacing();

                if (ImGui.BeginChild("Child Configuration", new Vector2(0.0f, 0.0f), false, windowFlags))
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
            var mpTickBarUI = new MPTickBarUI();
            switch (this.Configuration.UIType)
            {
                case UIType.FinalFantasyXIVDefault:
                    mpTickBarUI.Gauge = this.GaugeDefault;
                    mpTickBarUI.JobStack = this.JobStackDefault;
                    break;
                case UIType.MaterialUIDiscord:
                    mpTickBarUI.Gauge = this.GaugeMaterialUIDiscord;
                    mpTickBarUI.JobStack = this.JobStackMaterialUI;
                    break;
                case UIType.MaterialUIBlack:
                    mpTickBarUI.Gauge = this.GaugeMaterialUIBlack;
                    mpTickBarUI.JobStack = this.JobStackMaterialUI;
                    break;
            }
            return mpTickBarUI;
        }
    }
}