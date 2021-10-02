using ImGuiNET;
using ImGuiScene;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace MPTickBar
{
    public sealed class MPTickBarPluginUI : PluginUI, IDisposable
    {
        private TextureWrap GaugeDefault { get; set; }

        private TextureWrap GaugeMaterialUIBlack { get; set; }

        private TextureWrap GaugeMaterialUIDiscord { get; set; }

        private TextureWrap JobStackDefault { get; set; }

        private TextureWrap JobStackMaterialUI { get; set; }

        private TextureWrap NumberPercentage { get; set; }

        public int Level { get; set; }

        public bool IsConfigurationWindowVisible { get; set; }

        public bool IsMPTickBarVisible { get; set; }

        public bool IsPlayingAsBLM { get; set; }

        public bool IsCircleOfPowerActivated { get; set; }

        private bool IsCircleOfPowerActivatedPreview { get; set; }

        public bool IsUmbralIceIIIActivated { get; set; }

        private double ProgressTime { get; set; }

        private double ProgressTimePreview { get; set; }

        private List<RegressEffect> RegressEffects { get; set; } = new List<RegressEffect>();

        private class MPTickBarUI
        {
            public TextureWrap Gauge { get; set; }

            public TextureWrap JobStack { get; set; }
        }

        private class RegressEffect
        {
            public bool IsRegressing => this.Regress > 0.0f;

            public double Regress { get; private set; }

            private double LastProgress { get; set; }

            private double CurrentTime { get; set; }

            private double LastTime { get; set; }

            public void CheckForStartRegressing(double progress)
            {
                if (progress < this.LastProgress)
                    this.Regress = this.LastProgress;
            }

            public void Update(double progress)
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
            var style = ImGui.GetStyle();
            var styleDefault = new Vector2(style.ItemSpacing.X, style.ItemSpacing.Y);
            style.ItemSpacing = new Vector2(10.0f, 10.0f);

            this.DrawMPTickBarWindow();
            this.DrawConfigurationWindow();

            ImGui.GetStyle().ItemSpacing = styleDefault;
        }

        public void ResetProgressTime()
        {
            this.ProgressTime = 0.0;
        }

        public void UpdateProgressTime(double incrementedTime)
        {
            this.ProgressTime = (this.ProgressTime + (incrementedTime / 3.0)) % 1.0;
            this.ProgressTimePreview = ((DateTime.Now.Second % 3.0) + (DateTime.Now.Millisecond / 1000.0)) / 3.0;
        }

        public double GetProgressTime(bool isPreview)
        {
            return !isPreview ? this.ProgressTime : this.ProgressTimePreview;
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

        private static void RenderGaugeUIElement(MPTickBarUI mpTickBarUI, float uiScale, float offsetX, float offsetY, float elementWidth, double progress, int uiNumber, Vector4 tintColor, bool isBar, bool isProgress = false)
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
            ImGui.Image(mpTickBarUI.Gauge.ImGuiHandle, new Vector2((float)width, height), new Vector2(textureX, textureY), new Vector2((float)textureW, textureH), !isProgress ? tintColor : Vector4.One);
        }

        private void RenderRegressEffect(MPTickBarUI mpTickBarUI, float uiScale, float offsetX, float offsetY, float elementWidth, double progress, bool isPreview)
        {
            var regressEffect = this.RegressEffects[!isPreview ? 0 : 1];

            regressEffect.CheckForStartRegressing(progress);
            if (regressEffect.IsRegressing)
            {
                if (this.Configuration.IsRegressEffectVisible)
                    MPTickBarPluginUI.RenderGaugeUIElement(mpTickBarUI, uiScale, offsetX, offsetY, elementWidth, regressEffect.Regress, 4, Vector4.One, true, true);
            }
            regressEffect.Update(progress);
        }

        private static void RenderJobStackUIElement(MPTickBarUI mpTickBarUI, float uiScale, float startX, float startY, int uiNumber, Vector4 tintColor)
        {
            var textureX = uiNumber * 0.5f;
            var scaledElementWidth = 20.0f * uiScale;
            var scaledElementHeight = 20.0f * uiScale;
            ImGui.SetCursorPos(new Vector2(startX, startY));
            ImGui.Image(mpTickBarUI.JobStack.ImGuiHandle, new Vector2(scaledElementWidth, scaledElementHeight), new Vector2(textureX, 0.0f), new Vector2(textureX + 0.5f, 0.5f), tintColor);
        }

        private void RenderNumbers(float uiScale, float offsetX, float offsetY, float barElementWidth, bool isPreview)
        {
            var digitTotal = 10;
            var elementWidth = this.NumberPercentage.Width / digitTotal;
            var elementHeight = this.NumberPercentage.Height;
            var scaledElementWidth = elementWidth * uiScale;
            var scaledElementHeight = elementHeight * uiScale;
            var percentage = (int)(this.GetProgressTime(isPreview) * 100.0);
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
                digitOffsetX += elementWidth;
            }
        }

        private void DrawMPTickBar(bool isPreview)
        {
            var mpTickBarUI = this.GetMPTickBarUI();
            var uiScale = !isPreview ? this.Configuration.UIScale : 2.0f;
            var offsetX = !isPreview ? 4.0f : 0.0f;
            var offsetY = !isPreview ? (25.0f * uiScale) : 40.0f;
            var barElementWidth = 160.0f;
            var elementWidth = barElementWidth;

            MPTickBarPluginUI.RenderGaugeUIElement(mpTickBarUI, uiScale, offsetX, offsetY, elementWidth, 1.0f, 5, Vector4.One, false);
            this.RenderRegressEffect(mpTickBarUI, uiScale, offsetX, offsetY, elementWidth, this.GetProgressTime(isPreview), isPreview);
            MPTickBarPluginUI.RenderGaugeUIElement(mpTickBarUI, uiScale, offsetX, offsetY, elementWidth, this.GetProgressTime(isPreview), 2, new Vector4(this.Configuration.ProgressBarTintColor, 1.0f), true);
            MPTickBarPluginUI.RenderGaugeUIElement(mpTickBarUI, uiScale, offsetX, offsetY, elementWidth, 1.0f, 0, Vector4.One, false);

            if ((this.Configuration.FastFireIIIMarkerVisibility == FastFireIIIMarkerVisibility.Visible) ||
               ((this.Configuration.FastFireIIIMarkerVisibility == FastFireIIIMarkerVisibility.InUmbralIceIII) && this.IsUmbralIceIIIActivated) ||
               ((this.Configuration.FastFireIIIMarkerVisibility != FastFireIIIMarkerVisibility.Hidden) && isPreview))
            {
                var fastFireIIICastTime = PlayerHelpers.GetFastFireIIICastTime(this.Level, (!isPreview) ? this.IsCircleOfPowerActivated : this.IsCircleOfPowerActivatedPreview);
                var fastFireIIIMarkerOffset = (3.0f - fastFireIIICastTime + this.Configuration.FastFireIIIMarkerTimeOffset) * (elementWidth / 3.0f);
                var isProgressAtMarker = (fastFireIIIMarkerOffset / elementWidth) > this.GetProgressTime(isPreview);

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
                this.RenderNumbers(uiScale, offsetX, offsetY, barElementWidth, isPreview);
        }

        private void DrawMPTickBarWindow()
        {
            var isMPTickBarVisible = this.IsMPTickBarVisible;
            if (!isMPTickBarVisible)
                return;

            var windowFlags = this.Configuration.IsMPTickBarLocked ?
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
                this.DrawMPTickBar(false);
            ImGui.End();
        }

        private void DrawConfigurationWindow()
        {
            var isConfigurationWindowVisible = this.IsConfigurationWindowVisible;
            if (!isConfigurationWindowVisible)
                return;

            var windowFlags =
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoCollapse;

            ImGui.SetNextWindowSize(new Vector2(337.0f, 411.0f), ImGuiCond.Always);
            if (ImGui.Begin("MP Tick Bar configuration", ref isConfigurationWindowVisible, windowFlags))
            {
                this.IsConfigurationWindowVisible = isConfigurationWindowVisible;

                ImGui.Text("Preview:");
                if (ImGui.BeginChild("Child Preview", new Vector2(321.0f, 120.0f), false, windowFlags))
                {
                    if (this.IsPlayingAsBLM)
                        this.DrawMPTickBar(true);
                }
                ImGui.EndChild();

                static string CastTimeFormat(bool isPlayingAsBLM, float castTime) => isPlayingAsBLM ? (((int)(castTime * 100)) / 100.0f).ToString("0.00s") : "?";
                var previewFastFireIIICastTime = CastTimeFormat(this.IsPlayingAsBLM, PlayerHelpers.GetFastFireIIICastTime(this.Level, this.IsCircleOfPowerActivatedPreview));
                var currentFastFireIIICastTime = CastTimeFormat(this.IsPlayingAsBLM, PlayerHelpers.GetFastFireIIICastTime(this.Level, this.IsCircleOfPowerActivated));

                this.CheckBox(this.IsCircleOfPowerActivatedPreview, x => this.IsCircleOfPowerActivatedPreview = x, $"Circle of Power (Preview): {previewFastFireIIICastTime}");

                var text = "Fast Fire III cast time (Current): ";
                ImGui.Text(text);
                ImGui.SameLine(ImGui.CalcTextSize(text).X + 7.0f, 0.0f);
                ImGui.TextColored(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), currentFastFireIIICastTime);

                if (ImGui.BeginChild("Child Configuration", new Vector2(0.0f, 0.0f), false, windowFlags))
                {
                    if (ImGui.BeginTabBar("BeginTabBar"))
                    {
                        if (ImGui.BeginTabItem("MP Tick Bar"))
                        {
                            this.CheckBox(this.Configuration.IsMPTickBarLocked, x => this.Configuration.IsMPTickBarLocked = x, "Lock");
                            this.CheckBox(this.Configuration.IsRegressEffectVisible, x => this.Configuration.IsRegressEffectVisible = x, "Regress Effect", new Vector2(0.0f, 20.0f));
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text("Show/Hide bar effect animation when it goes from full to empty state.");
                                ImGui.EndTooltip();
                            }
                            this.ColorEdit3(this.Configuration.ProgressBarTintColor, x => this.Configuration.ProgressBarTintColor = x, "Progress Bar", new Vector2(0.0f, 20.0f));
                            this.Combo(this.Configuration.MPTickBarVisibility, x => this.Configuration.MPTickBarVisibility = x, "Visibility");
                            this.Combo(this.Configuration.UIType, x => this.Configuration.UIType = x, "UI");
                            this.DragFloat(this.Configuration.UIScale, x => this.Configuration.UIScale = x, "UI Scale", 0.1f, 1.0f, 5.0f, "%.1f");
                            ImGui.EndTabItem();
                        }

                        if (ImGui.BeginTabItem("Fast Fire III Marker"))
                        {
                            this.ColorEdit3(this.Configuration.FastFireIIIMarkerTintColor, x => this.Configuration.FastFireIIIMarkerTintColor = x, "Marker");
                            this.Combo(this.Configuration.FastFireIIIMarkerVisibility, x => this.Configuration.FastFireIIIMarkerVisibility = x, "Visibility");
                            this.Combo(this.Configuration.FastFireIIIMarkerType, x => this.Configuration.FastFireIIIMarkerType = x, "Style");
                            this.DragFloat(this.Configuration.FastFireIIIMarkerTimeOffset, x => this.Configuration.FastFireIIIMarkerTimeOffset = x, "Time Offset (s)", 0.01f, 0.0f, 0.5f, "%.2f");
                            ImGui.EndTabItem();
                        }

                        if (ImGui.BeginTabItem("Number (%)"))
                        {
                            this.ColorEdit3(this.Configuration.NumberPercentageTintColor, x => this.Configuration.NumberPercentageTintColor = x, "Number (%)");
                            this.Combo(this.Configuration.NumberPercentageVisibility, x => this.Configuration.NumberPercentageVisibility = x, "Visibility");
                            this.DragInt(this.Configuration.NumberPercentageOffsetX, x => this.Configuration.NumberPercentageOffsetX = x, "Offset X", 1, -50, 50, "%i");
                            this.DragInt(this.Configuration.NumberPercentageOffsetY, x => this.Configuration.NumberPercentageOffsetY = x, "Offset Y", 1, -30, 10, "%i");
                            ImGui.EndTabItem();
                        }
                    }
                    ImGui.EndTabBar();
                }
                ImGui.EndChild();
            }
            ImGui.End();
        }
    }
}