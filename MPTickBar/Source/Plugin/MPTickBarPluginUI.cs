using Dalamud.Interface;
using ImGuiNET;
using ImGuiScene;
using MPTickBar.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MPTickBar
{
    public sealed class MPTickBarPluginUI : PluginUI, IDisposable
    {
        private TextureWrap GaugeDefault { get; }

        private TextureWrap GaugeMaterialUIDiscord { get; }

        private TextureWrap GaugeMaterialUIBlack { get; }

        private TextureWrap GaugeMaterialUISilver { get; }

        private TextureWrap GaugeSolidBar { get; }

        private TextureWrap MarkerDefault { get; }

        private TextureWrap MarkerMaterialUI { get; }

        private TextureWrap MarkerLine { get; }

        private TextureWrap FireIIICastIndicator { get; }

        private TextureWrap Numbers { get; }

        public int Level { get; set; }

        public bool IsConfigurationWindowVisible { get; set; }

        public bool IsMPTickBarVisible { get; set; }

        public bool IsPlayingAsBLM { get; set; }

        public bool IsCircleOfPowerActivated { get; set; }

        private bool IsCircleOfPowerActivatedPreview { get; set; }

        public bool IsUmbralIceIIIActivated { get; set; }

        private double Progress { get; set; }

        private double ProgressPreview { get; set; }

        private List<RegressEffect> RegressEffects { get; } = new(2);

        private readonly List<int> RotateVertexIndices = new();

        private class MPTickBarUI
        {
            public TextureWrap Gauge { get; init; }

            public TextureWrap Marker { get; init; }
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

        public MPTickBarPluginUI(Configuration configuration, UiBuilder uiBuilder)
        {
            this.Configuration = configuration;

            this.GaugeDefault = uiBuilder.LoadImage(Resources.GaugeDefault);
            this.GaugeMaterialUIDiscord = uiBuilder.LoadImage(Resources.GaugeMaterialUIDiscord);
            this.GaugeMaterialUIBlack = uiBuilder.LoadImage(Resources.GaugeMaterialUIBlack);
            this.GaugeMaterialUISilver = uiBuilder.LoadImage(Resources.GaugeMaterialUISilver);
            this.GaugeSolidBar = uiBuilder.LoadImage(Resources.GaugeSolidBar);
            this.MarkerDefault = uiBuilder.LoadImage(Resources.MarkerDefault);
            this.MarkerMaterialUI = uiBuilder.LoadImage(Resources.MarkerMaterialUI);
            this.MarkerLine = uiBuilder.LoadImage(Resources.MarkerLine);
            this.FireIIICastIndicator = uiBuilder.LoadImage(Resources.FireIIICastIndicator);
            this.Numbers = uiBuilder.LoadImage(Resources.Numbers);

            this.RegressEffects.AddRange(Enumerable.Repeat(new RegressEffect(), this.RegressEffects.Capacity));
        }

        public void Dispose()
        {
            this.GaugeDefault.Dispose();
            this.GaugeMaterialUIDiscord.Dispose();
            this.GaugeMaterialUIBlack.Dispose();
            this.GaugeMaterialUISilver.Dispose();
            this.GaugeSolidBar.Dispose();
            this.MarkerDefault.Dispose();
            this.MarkerMaterialUI.Dispose();
            this.MarkerLine.Dispose();
            this.FireIIICastIndicator.Dispose();
            this.Numbers.Dispose();
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

        public void Update(double progress)
        {
            this.Progress = progress / 3.0;
            this.ProgressPreview = ImGui.GetTime() % 3.0 / 3.0;
        }

        public double GetProgress(bool isPreview)
        {
            return !isPreview ? this.Progress : this.ProgressPreview;
        }

        private MPTickBarUI GetMPTickBarUI()
        {
            TextureWrap gauge = this.Configuration.ProgressBar.UI switch
            {
                ProgressBarUI.Default => this.GaugeDefault,
                ProgressBarUI.MaterialUIDiscord => this.GaugeMaterialUIDiscord,
                ProgressBarUI.MaterialUIBlack => this.GaugeMaterialUIBlack,
                ProgressBarUI.MaterialUISilver => this.GaugeMaterialUISilver,
                ProgressBarUI.SolidBar => this.GaugeSolidBar,
                _ => this.GaugeDefault,
            };

            TextureWrap marker = this.Configuration.FastFireIIIMarker.UI switch
            {
                FastFireIIIMarkerUI.Default => this.MarkerDefault,
                FastFireIIIMarkerUI.MaterialUI => this.MarkerMaterialUI,
                FastFireIIIMarkerUI.Line => this.MarkerLine,
                _ => this.MarkerDefault,
            };

            return new() { Gauge = gauge, Marker = marker };
        }

        private void RenderBackgroundUIElement(MPTickBarUI mpTickBarUI, float offsetX, float offsetY, float gaugeElementWidth, float gaugeElementHeight, float textureToElementScale, int uiNumber, Vector4 color)
        {
            var width = gaugeElementWidth;
            var height = gaugeElementHeight;
            var scaledWidth = width * this.Configuration.ProgressBar.ScaleHorizontal;
            var scaledHeight = height * this.Configuration.ProgressBar.ScaleVertical;
            var x = offsetX;
            var y = offsetY;
            var textureElementHeight = gaugeElementHeight / textureToElementScale;
            var textureX = 0.0f;
            var textureY = (textureElementHeight * uiNumber) / mpTickBarUI.Gauge.Height;
            var textureW = 1.0f;
            var textureH = textureY + (textureElementHeight / mpTickBarUI.Gauge.Height);
            ImGui.SetCursorPos(new Vector2(x, y));
            ImGui.Image(mpTickBarUI.Gauge.ImGuiHandle, new Vector2(scaledWidth, scaledHeight), new Vector2(textureX, textureY), new Vector2(textureW, textureH), color);
        }

        private void RenderBarUIElement(MPTickBarUI mpTickBarUI, float offsetX, float offsetY, float gaugeElementWidth, float gaugeElementHeight, float textureToElementScale, double progress, int uiNumber, bool isProgress, Vector4 color)
        {
            var barTextureOffsetX = 12.0f * textureToElementScale;
            var width = (gaugeElementWidth - (barTextureOffsetX * 2.0f)) * progress;
            var height = gaugeElementHeight;
            var scaledWidth = width * this.Configuration.ProgressBar.ScaleHorizontal;
            var scaledHeight = height * this.Configuration.ProgressBar.ScaleVertical;
            var x = offsetX + (barTextureOffsetX * this.Configuration.ProgressBar.ScaleHorizontal);
            var y = offsetY;
            var textureElementX = barTextureOffsetX / textureToElementScale;
            var textureElementHeight = gaugeElementHeight / textureToElementScale;
            var textureX = textureElementX / mpTickBarUI.Gauge.Width;
            var textureY = (textureElementHeight * uiNumber) / mpTickBarUI.Gauge.Height;
            var textureW = textureX + ((1.0f - (textureX * 2.0f)) * (isProgress ? progress : 1.0f));
            var textureH = textureY + (textureElementHeight / mpTickBarUI.Gauge.Height);
            ImGui.SetCursorPos(new Vector2((float)(x), y));
            ImGui.Image(mpTickBarUI.Gauge.ImGuiHandle, new Vector2((float)scaledWidth, scaledHeight), new Vector2(textureX, textureY), new Vector2((float)textureW, textureH), color);
        }

        private void RenderMarkerUIElement(MPTickBarUI mpTickBarUI, float offsetX, float offsetY, float jobStackDimension, float fastFireIIIMarkerOffset, int uiNumber, Vector4 color)
        {
            var x = offsetX + (fastFireIIIMarkerOffset * this.Configuration.ProgressBar.ScaleHorizontal);
            var y = offsetY;
            var width = jobStackDimension;
            var height = jobStackDimension;
            var scaledWidth = width * this.Configuration.ProgressBar.ScaleHorizontal;
            var scaledHeight = height * this.Configuration.ProgressBar.ScaleVertical;
            var textureX = uiNumber * 0.5f;
            var textureY = 0.0f;
            var textureW = textureX + 0.5f;
            var textureH = 1.0f;
            ImGui.SetCursorPos(new Vector2(x, y));
            ImGui.Image(mpTickBarUI.Marker.ImGuiHandle, new Vector2(scaledWidth, scaledHeight), new Vector2(textureX, textureY), new Vector2(textureW, textureH), color);
        }

        private void RenderFireIIICastIndicator(float offsetX, float offsetY, float gaugeElementWidth, float gaugeElementHeight)
        {
            var adjustX = 20.0f;
            var width = this.FireIIICastIndicator.Width;
            var height = this.FireIIICastIndicator.Height;
            var scaledWidth = width * this.Configuration.FireIIICastIndicator.Scale;
            var scaledHeight = height * this.Configuration.FireIIICastIndicator.Scale;
            var x = offsetX + this.Configuration.FireIIICastIndicator.OffsetX + gaugeElementWidth + adjustX;
            var y = offsetY + this.Configuration.FireIIICastIndicator.OffsetY + (gaugeElementHeight / 2.0f);
            var textureX = 0.0f;
            var textureY = 0.0f;
            var textureW = 1.0f;
            var textureH = 1.0f;
            ImGui.SetCursorPos(new Vector2(x - (scaledWidth / 2.0f), y - (scaledHeight / 2.0f)));
            ImGui.Image(this.FireIIICastIndicator.ImGuiHandle, new Vector2(scaledWidth, scaledHeight), new Vector2(textureX, textureY), new Vector2(textureW, textureH), this.Configuration.FireIIICastIndicator.IndicatorColor);
        }

        private void RenderNumbers(float offsetX, float offsetY, float gaugeElementWidth, float gaugeElementHeight, bool isPreview)
        {
            var adjustY = 10.0f;
            var digitTotal = 10.0f;
            var width = (this.Numbers.Width / digitTotal);
            var height = this.Numbers.Height;
            var scaledWidth = width * this.Configuration.NumberPercentage.Scale;
            var scaledHeight = height * this.Configuration.NumberPercentage.Scale;
            var textureY = 0.0f;
            var textureH = 1.0f;
            int percentage = (int)(this.GetProgress(isPreview) * 100.0);
            var percentageText = percentage.ToString();
            var x = offsetX + this.Configuration.NumberPercentage.OffsetX + (gaugeElementWidth / 2.0f);
            var y = offsetY + this.Configuration.NumberPercentage.OffsetY + (gaugeElementHeight / 2.0f) + adjustY;
            var totalNumberWidth = scaledWidth * percentageText.Length;

            foreach (var digitText in percentageText)
            {
                var digit = char.GetNumericValue(digitText);
                var textureX = (width * digit) / this.Numbers.Width;
                var textureW = textureX + (width / this.Numbers.Width);
                ImGui.SetCursorPos(new Vector2(x - (totalNumberWidth / 2.0f), y - (scaledHeight / 2.0f)));
                ImGui.Image(this.Numbers.ImGuiHandle, new Vector2(scaledWidth, scaledHeight), new Vector2((float)textureX, textureY), new Vector2((float)textureW, textureH), this.Configuration.NumberPercentage.NumberPercentageColor);
                x += scaledWidth;
            }
        }

        private void VertexDataUpdate(float offsetX, float offsetY, float gaugeElementWidth, float gaugeElementHeight, bool isPreview)
        {
            static void Rotate(ref float x, ref float y, float degrees)
            {
                var angle = Math.PI * degrees / 180.0;
                var cos = (float)Math.Cos(angle);
                var sin = (float)Math.Sin(angle);
                var newX = x * cos - y * sin;
                var newY = x * sin + y * cos;
                x = newX;
                y = newY;
            }

            var windowPos = ImGui.GetWindowPos();
            var vertexBuffer = ImGui.GetWindowDrawList().VtxBuffer;
            var scale = !isPreview ? this.Configuration.General.Scale : 1.5f;
            var rotate = !isPreview ? this.Configuration.ProgressBar.Rotate : 0;
            var gaugeElementWidthCenter = gaugeElementWidth / 2.0f;
            var gaugeElementHeightCenter = gaugeElementHeight / 2.0f;
            for (var i = 0; i < vertexBuffer.Size; i++)
            {
                var x = (vertexBuffer[i].pos.X - windowPos.X - offsetX - gaugeElementWidthCenter) * scale;
                var y = (vertexBuffer[i].pos.Y - windowPos.Y - offsetY - gaugeElementHeightCenter) * scale;

                if (rotate != 0)
                {
                    if (this.RotateVertexIndices.Contains(i))
                        Rotate(ref x, ref y, rotate);
                }

                vertexBuffer[i].pos.X = windowPos.X + offsetX + x + gaugeElementWidthCenter;
                vertexBuffer[i].pos.Y = windowPos.Y + offsetY + y + gaugeElementHeightCenter;
            }
        }

        private void AddLastVertexIndicesToRotate(bool isPreview, int numberOfVertices)
        {
            if ((!isPreview) && (this.Configuration.ProgressBar.Rotate != 0))
            {
                if (numberOfVertices == 0)
                    this.RotateVertexIndices.Clear();

                var vertexSize = ImGui.GetWindowDrawList().VtxBuffer.Size;
                var startIndex = numberOfVertices > 0 ? vertexSize - numberOfVertices : 0;
                for (var i = startIndex; i < vertexSize; i++)
                    this.RotateVertexIndices.Add(i);
            }
        }

        private void DrawMPTickBar(bool isPreview)
        {
            var mpTickBarUI = this.GetMPTickBarUI();
            var textureToElementScale = 0.5f;
            var gaugeElementWidth = mpTickBarUI.Gauge.Width * textureToElementScale;
            var gaugeElementHeight = (mpTickBarUI.Gauge.Height / 6.0f) * textureToElementScale;
            var offsetX = !isPreview ? (this.Configuration.General.OffsetX + 20.0f) : 60.0f;
            var offsetY = !isPreview ? (this.Configuration.General.OffsetY + 20.0f) : 45.0f;

            var fastFireIIICastTime = PlayerHelpers.GetFastFireIIICastTime(this.Level, (!isPreview) ? this.IsCircleOfPowerActivated : this.IsCircleOfPowerActivatedPreview);
            var fastFireIIIMarkerOffset = (3.0f - fastFireIIICastTime + this.Configuration.FastFireIIIMarker.TimeOffset) * (gaugeElementWidth / 3.0f);
            var jobStackDimension = 20.0f;
            var fastFireIIIMarkerIconAdjustX = (jobStackDimension / 2.0f) / gaugeElementWidth;
            var isProgressAfterMarker = this.GetProgress(isPreview) > (fastFireIIIMarkerOffset / gaugeElementWidth) + fastFireIIIMarkerIconAdjustX;

            this.RenderBackgroundUIElement(mpTickBarUI, offsetX, offsetY, gaugeElementWidth, gaugeElementHeight, textureToElementScale, 5, this.Configuration.ProgressBar.BackgroundColor);

            var regressEffect = this.RegressEffects[!isPreview ? 0 : 1];
            regressEffect.Update(this.GetProgress(isPreview));
            if (this.Configuration.ProgressBar.IsRegressEffectEnabled && regressEffect.IsRegressing)
                this.RenderBarUIElement(mpTickBarUI, offsetX, offsetY, gaugeElementWidth, gaugeElementHeight, textureToElementScale, regressEffect.Regress, 4, false, this.Configuration.ProgressBar.RegressBarColor);

            this.RenderBarUIElement(mpTickBarUI, offsetX, offsetY, gaugeElementWidth, gaugeElementHeight, textureToElementScale, this.GetProgress(isPreview), 2, true, (isProgressAfterMarker) ? this.Configuration.ProgressBar.ProgressBarAfterMarkerColor : this.Configuration.ProgressBar.ProgressBarColor);
            this.RenderBackgroundUIElement(mpTickBarUI, offsetX, offsetY, gaugeElementWidth, gaugeElementHeight, textureToElementScale, 0, this.Configuration.ProgressBar.EdgeColor);

            if ((this.Configuration.FastFireIIIMarker.Visibility == FastFireIIIMarkerVisibility.Visible) ||
               ((this.Configuration.FastFireIIIMarker.Visibility == FastFireIIIMarkerVisibility.UnderUmbralIceIII) && this.IsUmbralIceIIIActivated) ||
               ((this.Configuration.FastFireIIIMarker.Visibility != FastFireIIIMarkerVisibility.Hidden) && isPreview))
            {
                if (this.Configuration.FastFireIIIMarker.UI != FastFireIIIMarkerUI.Line)
                    this.RenderMarkerUIElement(mpTickBarUI, offsetX, offsetY, jobStackDimension, fastFireIIIMarkerOffset, 0, this.Configuration.FastFireIIIMarker.BackgroundColor);

                this.RenderMarkerUIElement(mpTickBarUI, offsetX, offsetY, jobStackDimension, fastFireIIIMarkerOffset, 1, this.Configuration.FastFireIIIMarker.MarkerColor);
            }

            this.AddLastVertexIndicesToRotate(isPreview, 0);

            if (((this.Configuration.FireIIICastIndicator.Visibility == FireIIICastIndicatorVisibility.Visible) ||
                ((this.Configuration.FireIIICastIndicator.Visibility == FireIIICastIndicatorVisibility.UnderUmbralIceIII) && this.IsUmbralIceIIIActivated) ||
                ((this.Configuration.FireIIICastIndicator.Visibility != FireIIICastIndicatorVisibility.Hidden) && isPreview)) && ((isProgressAfterMarker && !isPreview) || isPreview))
                this.RenderFireIIICastIndicator(offsetX, offsetY, gaugeElementWidth, gaugeElementHeight);

            if ((this.Configuration.NumberPercentage.Visibility == NumberPercentageVisibility.Visible) ||
               ((this.Configuration.NumberPercentage.Visibility == NumberPercentageVisibility.UnderUmbralIceIII) && this.IsUmbralIceIIIActivated) ||
               ((this.Configuration.NumberPercentage.Visibility != NumberPercentageVisibility.Hidden) && isPreview))
                this.RenderNumbers(offsetX, offsetY, gaugeElementWidth, gaugeElementHeight, isPreview);

            this.VertexDataUpdate(offsetX, offsetY, gaugeElementWidth, gaugeElementHeight, isPreview);
        }

        private void DrawMPTickBarWindow()
        {
            var isMPTickBarVisible = this.IsMPTickBarVisible;
            if (!isMPTickBarVisible)
                return;

            var windowFlags = this.Configuration.General.IsLocked ?
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

            ImGui.SetNextWindowSize(new Vector2(481.0f, 641.0f), ImGuiCond.Always);
            if (ImGui.Begin("MP Tick Bar configuration", ref isConfigurationWindowVisible, windowFlags))
            {
                this.IsConfigurationWindowVisible = isConfigurationWindowVisible;

                ImGui.Text("Preview:");
                if (ImGui.BeginChild("Preview (Child)", new Vector2(465.0f, 122.0f), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
                {
                    if (this.IsPlayingAsBLM)
                        this.DrawMPTickBar(true);
                }
                ImGui.EndChild();

                static string CastTimeFormat(bool isPlayingAsBLM, float castTime) => isPlayingAsBLM ? (((int)(castTime * 100)) / 100.0f).ToString("0.00s") : "N/A";
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
                        var uiElementWidth = 190.0f;

                        var tabName = "General";
                        if (ImGui.BeginTabItem(tabName))
                        {
                            this.GeneralTab(tabName, uiElementWidth);
                            ImGui.EndTabItem();
                        }

                        tabName = "Progress Bar";
                        if (ImGui.BeginTabItem(tabName))
                        {
                            this.ProgressBarTab(tabName, uiElementWidth);
                            ImGui.EndTabItem();
                        }

                        tabName = "Fast Fire III Marker";
                        if (ImGui.BeginTabItem(tabName))
                        {
                            this.FastFireIIIMarkerTab(tabName, uiElementWidth);
                            ImGui.EndTabItem();
                        }

                        tabName = "Fire III Cast Indicator";
                        if (ImGui.BeginTabItem(tabName))
                        {
                            this.FireIIICastIndicatorTab(tabName, uiElementWidth);
                            ImGui.EndTabItem();
                        }

                        tabName = "Number (%)";
                        if (ImGui.BeginTabItem(tabName))
                        {
                            this.NumberPercentageTab(tabName, uiElementWidth);
                            ImGui.EndTabItem();
                        }
                    }
                    ImGui.EndTabBar();

                    ImGui.SetCursorPos(new Vector2(0.0f, ImGui.GetWindowHeight() - 23.0f));
                    if (ImGui.Button("Reset all settings to default"))
                    {
                        this.Configuration.Reset();
                        this.Configuration.Save();
                    }
                }
                ImGui.EndChild();
            }
            ImGui.End();
        }

        private void GeneralTab(string tabName, float uiElementWidth)
        {
            var config = this.Configuration.General;

            if (this.BeginGroupPanel(tabName, "Location", 2))
            {
                this.CheckBox(config.IsLocked, x => config.IsLocked = x, "Lock");
                this.DragInt(config.OffsetX, config.OffsetY, x => config.OffsetX = x, x => config.OffsetY = x, "Offset", 1, -2000, 2000, "%i", uiElementWidth);
            }
            this.EndGroupPanel();

            if (this.BeginGroupPanel(tabName, "Dimension", 1))
                this.DragFloat(config.Scale, x => config.Scale = x, "Scale", 0.01f, 0.1f, 5.0f, "%.2f", uiElementWidth);
            this.EndGroupPanel();

            if (this.BeginGroupPanel(tabName, "Functional", 2))
            {
                this.CheckBox(config.IsAutostartEnabled, x => config.IsAutostartEnabled = x, "Autostart");
                PluginUI.Tooltip("Enable the progress bar to start automatically when changing zones or before combat starts (at full MP).\n\nAfter a while, the game stops sending the required data to trigger this functionality. Once you die, it'll work again.");
                this.Combo(config.Visibility, x => config.Visibility = x, "Visibility", uiElementWidth);
            }
            this.EndGroupPanel();
        }

        private void ProgressBarTab(string tabName, float uiElementWidth)
        {
            var config = this.Configuration.ProgressBar;

            if (this.BeginGroupPanel(tabName, "Dimension", 1))
                this.DragFloat(config.ScaleHorizontal, config.ScaleVertical, x => config.ScaleHorizontal = x, x => config.ScaleVertical = x, "Scale", 0.01f, 0.1f, 5.0f, "%.2f", uiElementWidth);
            this.EndGroupPanel();

            if (this.BeginGroupPanel(tabName, "Transform", 1))
                this.DragInt(config.Rotate, x => config.Rotate = x, "Rotate", 1, 0, 360, "%i", uiElementWidth);
            this.EndGroupPanel();

            if (this.BeginGroupPanel(tabName, "Visual", 5))
            {
                this.ColorEdit4(config.ProgressBarColor, x => config.ProgressBarColor = x, "Progress Bar");
                this.ColorEdit4(config.ProgressBarAfterMarkerColor, x => config.ProgressBarAfterMarkerColor = x, "Progress Bar (After reaching the marker)", new Vector2(0.0f, 10.0f));
                this.ColorEdit4(config.BackgroundColor, x => config.BackgroundColor = x, "Background");
                this.ColorEdit4(config.EdgeColor, x => config.EdgeColor = x, "Edge");
                this.CheckBox(config.IsRegressEffectEnabled, x => config.IsRegressEffectEnabled = x, "Regress Effect");
                PluginUI.Tooltip("Show the bar effect animation when it goes from full to an empty state.");
                this.ColorEdit4(config.RegressBarColor, x => config.RegressBarColor = x, "Regress Bar", new Vector2(0.0f, 10.0f));
                this.Combo(config.UI, x => config.UI = x, "UI", uiElementWidth);
            }
            this.EndGroupPanel();
        }

        private void FastFireIIIMarkerTab(string tabName, float uiElementWidth)
        {
            var config = this.Configuration.FastFireIIIMarker;

            if (this.BeginGroupPanel(tabName, "Visual", 3))
            {
                this.ColorEdit4(config.MarkerColor, x => config.MarkerColor = x, "Marker");
                this.ColorEdit4(config.BackgroundColor, x => config.BackgroundColor = x, "Background");
                this.Combo(config.UI, x => config.UI = x, "UI", uiElementWidth);
            }
            this.EndGroupPanel();

            if (this.BeginGroupPanel(tabName, "Functional", 2))
            {
                this.DragFloat(config.TimeOffset, x => config.TimeOffset = x, "Time Offset (s)", 0.01f, -0.5f, 0.5f, "%.2f", uiElementWidth);
                this.Combo(config.Visibility, x => config.Visibility = x, "Visibility", uiElementWidth);
            }
            this.EndGroupPanel();

            config.TimeOffset = Math.Clamp(config.TimeOffset, -0.5f, 0.5f);
        }

        private void FireIIICastIndicatorTab(string tabName, float uiElementWidth)
        {
            var config = this.Configuration.FireIIICastIndicator;

            if (this.BeginGroupPanel(tabName, "Location", 1))
                this.DragInt(config.OffsetX, config.OffsetY, x => config.OffsetX = x, x => config.OffsetY = x, "Offset", 1, -2000, 2000, "%i", uiElementWidth);
            this.EndGroupPanel();

            if (this.BeginGroupPanel(tabName, "Dimension", 1))
                this.DragFloat(config.Scale, x => config.Scale = x, "Scale", 0.01f, 0.1f, 5.0f, "%.2f", uiElementWidth);
            this.EndGroupPanel();

            if (this.BeginGroupPanel(tabName, "Visual", 1))
                this.ColorEdit4(config.IndicatorColor, x => config.IndicatorColor = x, "Indicator");
            this.EndGroupPanel();

            if (this.BeginGroupPanel(tabName, "Functional", 1))
                this.Combo(config.Visibility, x => config.Visibility = x, "Visibility", uiElementWidth);
            this.EndGroupPanel();
        }

        private void NumberPercentageTab(string tabName, float uiElementWidth)
        {
            var config = this.Configuration.NumberPercentage;

            if (this.BeginGroupPanel(tabName, "Location", 1))
                this.DragInt(config.OffsetX, config.OffsetY, x => config.OffsetX = x, x => config.OffsetY = x, "Offset", 1, -2000, 2000, "%i", uiElementWidth);
            this.EndGroupPanel();

            if (this.BeginGroupPanel(tabName, "Dimension", 1))
                this.DragFloat(config.Scale, x => config.Scale = x, "Scale", 0.01f, 0.1f, 5.0f, "%.2f", uiElementWidth);
            this.EndGroupPanel();

            if (this.BeginGroupPanel(tabName, "Visual", 1))
                this.ColorEdit4(config.NumberPercentageColor, x => config.NumberPercentageColor = x, "Number (%)");
            this.EndGroupPanel();

            if (this.BeginGroupPanel(tabName, "Functional", 1))
                this.Combo(config.Visibility, x => config.Visibility = x, "Visibility", uiElementWidth);
            this.EndGroupPanel();
        }
    }
}