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

        private TextureWrap FireIIICastIndicator { get; set; }

        private TextureWrap NumberPercentage { get; set; }

        public int Level { get; set; }

        public bool IsConfigurationWindowVisible { get; set; }

        public bool IsMPTickBarVisible { get; set; }

        public bool IsPlayingAsBLM { get; set; }

        public bool IsCircleOfPowerActivated { get; set; }

        private bool IsCircleOfPowerActivatedPreview { get; set; }

        public bool IsUmbralIceIIIActivated { get; set; }

        private double Progress { get; set; }

        private double ProgressPreview { get; set; }

        private List<RegressEffect> RegressEffects { get; set; } = new List<RegressEffect>();

        private class MPTickBarUI
        {
            public TextureWrap Gauge { get; set; }

            public TextureWrap JobStack { get; set; }
        }

        private class RegressEffect
        {
            public bool IsRegressing => this.Regress > 0;

            public double Regress { get; private set; }

            public void Update(double progress)
            {
                var progressScale = 7.5;
                this.Regress = 1.0f - (progress * progressScale);

                if (progress <= 0)
                    this.Regress = 0;
            }
        }

        public MPTickBarPluginUI(Configuration configuration, TextureWrap gaugeDefault, TextureWrap gaugeMaterialUIBlack, TextureWrap gaugeMaterialUIDiscord, TextureWrap jobStackDefault, TextureWrap jobStackMaterialUI, TextureWrap fireIIICastIndicator, TextureWrap numberPercentage)
        {
            this.Configuration = configuration;
            this.GaugeDefault = gaugeDefault;
            this.GaugeMaterialUIBlack = gaugeMaterialUIBlack;
            this.GaugeMaterialUIDiscord = gaugeMaterialUIDiscord;
            this.JobStackDefault = jobStackDefault;
            this.JobStackMaterialUI = jobStackMaterialUI;
            this.FireIIICastIndicator = fireIIICastIndicator;
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
            this.FireIIICastIndicator.Dispose();
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

        public void ResetProgress()
        {
            this.Progress = 0.0;
        }

        public void Update(double incrementedTime)
        {
            var fireIIICastSeconds = 3.0;

            this.Progress += incrementedTime / fireIIICastSeconds;
            if (this.Progress > 1)
                this.Progress -= 1;

            this.ProgressPreview = ((DateTime.Now.Second % fireIIICastSeconds) + (DateTime.Now.Millisecond / 1000.0)) / fireIIICastSeconds;
        }

        public double GetProgress(bool isPreview)
        {
            return !isPreview ? this.Progress : this.ProgressPreview;
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

        private static void RenderBackgroundUIElement(MPTickBarUI mpTickBarUI, float offsetX, float offsetY, float gaugeElementWidth, float gaugeElementHeight, float textureToElementScale, int uiNumber)
        {
            var x = offsetX;
            var y = offsetY;
            var width = gaugeElementWidth;
            var height = gaugeElementHeight;
            var textureElementHeight = gaugeElementHeight / textureToElementScale;
            var textureX = 0.0f;
            var textureY = (textureElementHeight * uiNumber) / mpTickBarUI.Gauge.Height;
            var textureW = 1.0f;
            var textureH = textureY + (textureElementHeight / mpTickBarUI.Gauge.Height);
            ImGui.SetCursorPos(new Vector2(x, y));
            ImGui.Image(mpTickBarUI.Gauge.ImGuiHandle, new Vector2(width, height), new Vector2(textureX, textureY), new Vector2(textureW, textureH), Vector4.One);
        }

        private static void RenderBarUIElement(MPTickBarUI mpTickBarUI, float offsetX, float offsetY, float gaugeElementWidth, float gaugeElementHeight, float barTextureOffsetX, float textureToElementScale, double progress, int uiNumber, bool isProgress, Vector3 tintColor)
        {
            var x = offsetX + barTextureOffsetX;
            var y = offsetY;
            var width = (gaugeElementWidth - (barTextureOffsetX * 2.0f)) * progress;
            var height = gaugeElementHeight;
            var textureElementX = barTextureOffsetX / textureToElementScale;
            var textureElementHeight = gaugeElementHeight / textureToElementScale;
            var textureX = textureElementX / mpTickBarUI.Gauge.Width;
            var textureY = (textureElementHeight * uiNumber) / mpTickBarUI.Gauge.Height;
            var textureW = textureX + ((1.0f - (textureX * 2.0f)) * (isProgress ? progress : 1.0f));
            var textureH = textureY + (textureElementHeight / mpTickBarUI.Gauge.Height);
            ImGui.SetCursorPos(new Vector2(x, y));
            ImGui.Image(mpTickBarUI.Gauge.ImGuiHandle, new Vector2((float)width, height), new Vector2(textureX, textureY), new Vector2((float)textureW, textureH), new Vector4(tintColor, 1.0f));
        }

        private static void RenderJobStackUIElement(MPTickBarUI mpTickBarUI, float offsetX, float offsetY, float jobStackDimension, float fastFireIIIMarkerOffset, int uiNumber, Vector3 tintColor)
        {
            var x = offsetX + fastFireIIIMarkerOffset;
            var y = offsetY;
            var width = jobStackDimension;
            var height = jobStackDimension;
            var textureX = uiNumber * 0.5f;
            var textureY = 0.0f;
            var textureW = textureX + 0.5f;
            var textureH = 0.5f;
            ImGui.SetCursorPos(new Vector2(x, y));
            ImGui.Image(mpTickBarUI.JobStack.ImGuiHandle, new Vector2(width, height), new Vector2(textureX, textureY), new Vector2(textureW, textureH), new Vector4(tintColor, 1.0f));
        }

        private static void RenderLine(float offsetX, float offsetY, float fastFireIIIMarkerOffset, Vector3 tintColor)
        {
            var windowPos = ImGui.GetWindowPos();
            var adjustX = 10.5f;
            var adjustY = 6.0f;
            var xBegin = offsetX + fastFireIIIMarkerOffset + windowPos.X + adjustX;
            var yBegin = offsetY + windowPos.Y + adjustY;
            var xEnd = xBegin;
            var yEnd = yBegin + 8.0f;
            var thickness = 5.0f;
            ImGui.GetWindowDrawList().AddLine(new Vector2(xBegin, yBegin), new Vector2(xEnd, yEnd), ImGui.GetColorU32(new Vector4(tintColor, 1.0f)), thickness);
        }

        private void RenderNumbers(float offsetX, float offsetY, float gaugeElementWidth, float gaugeElementHeight, bool isPreview)
        {
            var digitTotal = 10.0f;
            var width = this.NumberPercentage.Width / digitTotal;
            var height = this.NumberPercentage.Height;
            var textureY = 0.0f;
            var textureH = 1.0f;
            int percentage = (int)(this.GetProgress(isPreview) * 100.0);
            var percentageText = percentage.ToString();
            var textAdjustY = 10.0f;
            var x = offsetX + this.Configuration.NumberPercentageOffsetX + (gaugeElementWidth / 2.0f) - (percentageText.Length * width / 2.0f);
            var y = offsetY + this.Configuration.NumberPercentageOffsetY + (gaugeElementHeight / 2.0f) - (height / 2.0f) + textAdjustY;

            foreach (var digitText in percentageText)
            {
                var digit = char.GetNumericValue(digitText);
                var textureX = (width * digit) / this.NumberPercentage.Width;
                var textureW = textureX + (width / this.NumberPercentage.Width);
                ImGui.SetCursorPos(new Vector2(x, y));
                ImGui.Image(this.NumberPercentage.ImGuiHandle, new Vector2(23, 18), new Vector2((float)textureX, textureY), new Vector2((float)textureW, textureH), new Vector4(this.Configuration.NumberPercentageTintColor, 1.0f));
                x += width;
            }
        }

        private void RenderFireIIICastIndicator(float offsetX, float offsetY, float gaugeElementWidth, float gaugeElementHeight)
        {
            var adjustX = 7.0f;
            var adjustSize = 0.8f;
            var width = this.FireIIICastIndicator.Width * adjustSize;
            var height = this.FireIIICastIndicator.Height * adjustSize;
            var x = offsetX + this.Configuration.FireIIICastIndicatorOffsetX + gaugeElementWidth - adjustX;
            var y = offsetY + this.Configuration.FireIIICastIndicatorOffsetY + (gaugeElementHeight / 2.0f) - (height / 2.0f);
            var textureX = 0.0f;
            var textureY = 0.0f;
            var textureW = 1.0f;
            var textureH = 1.0f;
            ImGui.SetCursorPos(new Vector2(x, y));
            ImGui.Image(this.FireIIICastIndicator.ImGuiHandle, new Vector2(width, height), new Vector2(textureX, textureY), new Vector2(textureW, textureH), new Vector4(this.Configuration.FireIIICastIndicatorTintColor, 1.0f));
        }

        private void DrawMPTickBar(bool isPreview)
        {
            var mpTickBarUI = this.GetMPTickBarUI();
            var textureToElementScale = 0.5f;
            var barTextureOffsetX = 12.0f * textureToElementScale;
            var gaugeElementWidth = mpTickBarUI.Gauge.Width * textureToElementScale;
            var gaugeElementHeight = 40.0f * textureToElementScale;
            var offsetX = !isPreview ? 50.0f : 4.0f;
            var offsetY = !isPreview ? 90.0f : 40.0f;

            MPTickBarPluginUI.RenderBackgroundUIElement(mpTickBarUI, offsetX, offsetY, gaugeElementWidth, gaugeElementHeight, textureToElementScale, 5);

            var regressEffect = this.RegressEffects[!isPreview ? 0 : 1];
            regressEffect.Update(this.GetProgress(isPreview));
            if (this.Configuration.IsRegressEffectVisible && regressEffect.IsRegressing)
                MPTickBarPluginUI.RenderBarUIElement(mpTickBarUI, offsetX, offsetY, gaugeElementWidth, gaugeElementHeight, barTextureOffsetX, textureToElementScale, regressEffect.Regress, 4, false, Vector3.One);

            MPTickBarPluginUI.RenderBarUIElement(mpTickBarUI, offsetX, offsetY, gaugeElementWidth, gaugeElementHeight, barTextureOffsetX, textureToElementScale, this.GetProgress(isPreview), 2, true, this.Configuration.ProgressBarTintColor);
            MPTickBarPluginUI.RenderBackgroundUIElement(mpTickBarUI, offsetX, offsetY, gaugeElementWidth, gaugeElementHeight, textureToElementScale, 0);

            var fireIIICastSeconds = 3.0f;
            var fastFireIIICastTime = PlayerHelpers.GetFastFireIIICastTime(this.Level, (!isPreview) ? this.IsCircleOfPowerActivated : this.IsCircleOfPowerActivatedPreview);
            var fastFireIIIMarkerOffset = (fireIIICastSeconds - fastFireIIICastTime + this.Configuration.FastFireIIIMarkerTimeOffset) * (gaugeElementWidth / fireIIICastSeconds);
            var jobStackDimension = 20.0f;

            if ((this.Configuration.FastFireIIIMarkerVisibility == FastFireIIIMarkerVisibility.Visible) ||
               ((this.Configuration.FastFireIIIMarkerVisibility == FastFireIIIMarkerVisibility.UnderUmbralIceIII) && this.IsUmbralIceIIIActivated) ||
               ((this.Configuration.FastFireIIIMarkerVisibility != FastFireIIIMarkerVisibility.Hidden) && isPreview))
            {
                if (this.Configuration.FastFireIIIMarkerType == FastFireIIIMarkerType.Icon)
                {
                    MPTickBarPluginUI.RenderJobStackUIElement(mpTickBarUI, offsetX, offsetY, jobStackDimension, fastFireIIIMarkerOffset, 0, Vector3.One);
                    MPTickBarPluginUI.RenderJobStackUIElement(mpTickBarUI, offsetX, offsetY, jobStackDimension, fastFireIIIMarkerOffset, 1, this.Configuration.FastFireIIIMarkerTintColor);
                }
                else if (this.Configuration.FastFireIIIMarkerType == FastFireIIIMarkerType.Line)
                    MPTickBarPluginUI.RenderLine(offsetX, offsetY, fastFireIIIMarkerOffset, this.Configuration.FastFireIIIMarkerTintColor);
            }

            if ((this.Configuration.NumberPercentageVisibility == NumberPercentageVisibility.Visible) ||
               ((this.Configuration.NumberPercentageVisibility == NumberPercentageVisibility.UnderUmbralIceIII) && this.IsUmbralIceIIIActivated) ||
               ((this.Configuration.NumberPercentageVisibility != NumberPercentageVisibility.Hidden) && isPreview))
                this.RenderNumbers(offsetX, offsetY, gaugeElementWidth, gaugeElementHeight, isPreview);

            var fastFireIIIMarkerIconAdjustX = (jobStackDimension / 2.0f) / gaugeElementWidth;
            var isProgressAtMarker = this.GetProgress(isPreview) > (fastFireIIIMarkerOffset / gaugeElementWidth) + fastFireIIIMarkerIconAdjustX;
            if (((this.Configuration.FireIIICastIndicatorVisibility == FireIIICastIndicatorVisibility.Visible) ||
                ((this.Configuration.FireIIICastIndicatorVisibility == FireIIICastIndicatorVisibility.UnderUmbralIceIII) && this.IsUmbralIceIIIActivated) ||
                ((this.Configuration.FireIIICastIndicatorVisibility != FireIIICastIndicatorVisibility.Hidden) && isPreview)) && isProgressAtMarker)
                this.RenderFireIIICastIndicator(offsetX, offsetY, gaugeElementWidth, gaugeElementHeight);

            var windowPos = ImGui.GetWindowPos();
            var vertexBuffer = ImGui.GetWindowDrawList().VtxBuffer;
            var uiScale = (!isPreview ? this.Configuration.UIScale : 2.0f) - 1.0f;
            for (int i = 0; i < vertexBuffer.Size; i++)
            {
                vertexBuffer[i].pos.X += (vertexBuffer[i].pos.X - windowPos.X) * uiScale - (offsetX * uiScale);
                vertexBuffer[i].pos.Y += (vertexBuffer[i].pos.Y - windowPos.Y) * uiScale - (offsetY * uiScale);
            }
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
            {
                if (ImGui.BeginChild("MP Tick Bar (Child)", new Vector2(0.0f, 0.0f), false, windowFlags))
                    this.DrawMPTickBar(false);
                ImGui.EndChild();
            }
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

            ImGui.SetNextWindowSize(new Vector2(418.0f, 413.0f), ImGuiCond.Always);
            if (ImGui.Begin("MP Tick Bar configuration", ref isConfigurationWindowVisible, windowFlags))
            {
                this.IsConfigurationWindowVisible = isConfigurationWindowVisible;

                ImGui.Text("Preview:");
                if (ImGui.BeginChild("Preview (Child)", new Vector2(402.0f, 122.0f), false))
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

                if (ImGui.BeginChild("Tabs (Child)", new Vector2(0.0f, 0.0f), false, windowFlags))
                {
                    if (ImGui.BeginTabBar("Tab Bar"))
                    {
                        if (ImGui.BeginTabItem("MP Tick Bar"))
                        {
                            this.CheckBox(this.Configuration.IsMPTickBarLocked, x => this.Configuration.IsMPTickBarLocked = x, "Lock");
                            this.CheckBox(this.Configuration.IsRegressEffectVisible, x => this.Configuration.IsRegressEffectVisible = x, "Regress Effect", new Vector2(0.0f, 20.0f));
                            PluginUI.Tooltip("Show/Hide bar effect animation when it goes from full to empty state.");
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

                        if (ImGui.BeginTabItem("Fire III Cast Indicator"))
                        {
                            this.ColorEdit3(this.Configuration.FireIIICastIndicatorTintColor, x => this.Configuration.FireIIICastIndicatorTintColor = x, "Indicator");
                            this.Combo(this.Configuration.FireIIICastIndicatorVisibility, x => this.Configuration.FireIIICastIndicatorVisibility = x, "Visibility");
                            this.DragInt(this.Configuration.FireIIICastIndicatorOffsetX, x => this.Configuration.FireIIICastIndicatorOffsetX = x, "Offset X", 1, -10, 10, "%i");
                            this.DragInt(this.Configuration.FireIIICastIndicatorOffsetY, x => this.Configuration.FireIIICastIndicatorOffsetY = x, "Offset Y", 1, -10, 10, "%i");
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