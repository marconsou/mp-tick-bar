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
        public PlayerState PlayerState { get; set; }

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

        private TextureWrap FireIIIIcon { get; }

        public bool IsConfigurationWindowVisible { get; set; }

        private double Progress { get; set; }

        private RegressEffectData RegressEffect { get; } = new();

        private List<int> RotateVertexIndices { get; } = new();

        private List<(int, float)> ScaleHorizontalVertexIndices { get; } = new();

        private List<(int, float)> ScaleVerticalVertexIndices { get; } = new();

        private class MPTickBarUI
        {
            public TextureWrap Gauge { get; init; }

            public TextureWrap Marker { get; init; }
        }

        private class RegressEffectData
        {
            public bool IsRegressing => this.Regress > 0;

            public double Regress { get; private set; }

            private double LastProgress { get; set; }

            public void Update(double progress)
            {
                if ((progress == 0.0) && (this.LastProgress == 0.0))
                    this.Regress = 0.0;
                else if (this.LastProgress > progress)
                    this.Regress = this.LastProgress;
                else if (progress > 0.08)
                    this.Regress -= (progress - this.LastProgress) * 12.0;

                this.Regress = Math.Clamp(this.Regress, 0.0, 1.0);
                this.LastProgress = progress;
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
            this.FireIIIIcon = uiBuilder.LoadImage(Resources.FireIIIIcon);
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
            this.FireIIIIcon.Dispose();
        }

        public void Draw()
        {
            var style = ImGui.GetStyle();

            var windowPadding = new Vector2(style.WindowPadding.X, style.WindowPadding.Y);
            var windowRounding = style.WindowRounding;
            style.WindowPadding = Vector2.Zero;
            style.WindowRounding = 0.0f;

            this.DrawMPTickBarWindow();
            style.WindowPadding = windowPadding;
            style.WindowRounding = windowRounding;

            this.DrawConfigurationWindow();
        }

        public void Update(double progress)
        {
            this.Progress = progress / 3.0;
            this.RegressEffect.Update(this.Progress);
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

        private void RenderBackgroundUIElement(MPTickBarUI mpTickBarUI, float offsetX, float offsetY, float gaugeWidth, float gaugeHeight, float textureToElementScale, int uiNumber, Vector4 color)
        {
            var x = offsetX;
            var y = offsetY;
            var width = gaugeWidth;
            var height = gaugeHeight;
            var textureElementHeight = gaugeHeight / textureToElementScale;
            var textureX = 0.0f;
            var textureY = (textureElementHeight * uiNumber) / mpTickBarUI.Gauge.Height;
            var textureW = 1.0f;
            var textureH = textureY + (textureElementHeight / mpTickBarUI.Gauge.Height);
            ImGui.SetCursorPos(new(x, y));
            ImGui.Image(mpTickBarUI.Gauge.ImGuiHandle, new(width, height), new(textureX, textureY), new(textureW, textureH), color);
        }

        private void RenderBarUIElement(MPTickBarUI mpTickBarUI, float offsetX, float offsetY, float gaugeWidth, float gaugeHeight, float textureToElementScale, double progress, int uiNumber, bool isProgress, Vector4 color)
        {
            var barTextureOffsetX = 12.0f * textureToElementScale;
            var width = (float)((gaugeWidth - (barTextureOffsetX * 2.0f)) * progress);
            var height = gaugeHeight;
            var x = offsetX + barTextureOffsetX;
            var y = offsetY;
            var textureElementX = barTextureOffsetX / textureToElementScale;
            var textureElementHeight = gaugeHeight / textureToElementScale;
            var textureX = textureElementX / mpTickBarUI.Gauge.Width;
            var textureY = (textureElementHeight * uiNumber) / mpTickBarUI.Gauge.Height;
            var textureW = textureX + (float)((1.0f - (textureX * 2.0f)) * (isProgress ? progress : 1.0f));
            var textureH = textureY + (textureElementHeight / mpTickBarUI.Gauge.Height);
            ImGui.SetCursorPos(new(x, y));
            ImGui.Image(mpTickBarUI.Gauge.ImGuiHandle, new(width, height), new(textureX, textureY), new(textureW, textureH), color);
        }

        private void RenderMarkerUIElement(MPTickBarUI mpTickBarUI, float offsetX, float offsetY, float dimension, float fastFireIIIMarkerOffset, int uiNumber, Vector4 color)
        {
            var adjustX = -4.0f;
            var x = offsetX + adjustX + fastFireIIIMarkerOffset;
            var y = offsetY;
            var width = dimension * this.Configuration.FastFireIIIMarker.ScaleHorizontal;
            var height = dimension * this.Configuration.FastFireIIIMarker.ScaleVertical;
            var textureX = uiNumber * 0.5f;
            var textureY = 0.0f;
            var textureW = textureX + 0.5f;
            var textureH = 1.0f;
            ImGui.SetCursorPos(new(x - ((width - dimension) / 2.0f), y - ((height - dimension) / 2.0f)));
            ImGui.Image(mpTickBarUI.Marker.ImGuiHandle, new(width, height), new(textureX, textureY), new(textureW, textureH), color);
        }

        private void RenderFireIIICastIndicator(float offsetX, float offsetY, float gaugeWidth, float gaugeHeight)
        {
            var adjustX = 20.0f;
            var width = this.FireIIICastIndicator.Width * this.Configuration.FireIIICastIndicator.Scale;
            var height = this.FireIIICastIndicator.Height * this.Configuration.FireIIICastIndicator.Scale;
            var x = offsetX + this.Configuration.FireIIICastIndicator.OffsetX + gaugeWidth + adjustX;
            var y = offsetY + this.Configuration.FireIIICastIndicator.OffsetY + (gaugeHeight / 2.0f);
            var textureX = 0.0f;
            var textureY = 0.0f;
            var textureW = 1.0f;
            var textureH = 1.0f;
            ImGui.SetCursorPos(new(x - (width / 2.0f), y - (height / 2.0f)));
            ImGui.Image(this.FireIIICastIndicator.ImGuiHandle, new(width, height), new(textureX, textureY), new(textureW, textureH), this.Configuration.FireIIICastIndicator.IndicatorColor);
        }

        private void RenderNumbers(float offsetX, float offsetY, float gaugeWidth, float gaugeHeight)
        {
            var adjustY = 10.0f;
            var digitTotal = 10.0f;
            var width = (this.Numbers.Width / digitTotal);
            var height = this.Numbers.Height;
            var scaledWidth = width * this.Configuration.NumberPercentage.Scale;
            var scaledHeight = height * this.Configuration.NumberPercentage.Scale;
            var textureY = 0.0f;
            var textureH = 1.0f;
            int percentage = (int)(this.Progress * 100.0);
            var percentageText = percentage.ToString();
            var x = offsetX + this.Configuration.NumberPercentage.OffsetX + (gaugeWidth / 2.0f);
            var y = offsetY + this.Configuration.NumberPercentage.OffsetY + (gaugeHeight / 2.0f) + adjustY;
            var totalNumberWidth = scaledWidth * percentageText.Length;

            foreach (var digitText in percentageText)
            {
                var digit = char.GetNumericValue(digitText);
                var textureX = (width * digit) / this.Numbers.Width;
                var textureW = textureX + (width / this.Numbers.Width);
                ImGui.SetCursorPos(new(x - (totalNumberWidth / 2.0f), y - (scaledHeight / 2.0f)));
                ImGui.Image(this.Numbers.ImGuiHandle, new(scaledWidth, scaledHeight), new((float)textureX, textureY), new((float)textureW, textureH), this.Configuration.NumberPercentage.NumberPercentageColor);
                x += scaledWidth;
            }
        }

        private void AddVertexDataUpToThisPoint()
        {
            void AddVertexIndicesToRotate()
            {
                this.RotateVertexIndices.Clear();
                if (this.Configuration.ProgressBar.Rotate != 0)
                {
                    var vertexSize = ImGui.GetWindowDrawList().VtxBuffer.Size;
                    for (var i = 0; i < vertexSize; i++)
                        this.RotateVertexIndices.Add(i);
                }
            }

            void AddVertexIndicesToScale(List<(int, float)> scaleVertexIndices, float scale)
            {
                scaleVertexIndices.Clear();
                var vertexSize = ImGui.GetWindowDrawList().VtxBuffer.Size;
                for (var i = 0; i < vertexSize; i++)
                    scaleVertexIndices.Add((i, scale));
            }

            AddVertexIndicesToRotate();
            AddVertexIndicesToScale(this.ScaleHorizontalVertexIndices, this.Configuration.ProgressBar.ScaleHorizontal);
            AddVertexIndicesToScale(this.ScaleVerticalVertexIndices, this.Configuration.ProgressBar.ScaleVertical);
        }

        private void VertexDataUpdate(float offsetX, float offsetY, float gaugeWidth, float gaugeHeight)
        {
            void Rotate(ref float x, ref float y, float degrees)
            {
                var angle = Math.PI * degrees / 180.0;
                var cos = (float)Math.Cos(angle);
                var sin = (float)Math.Sin(angle);
                var newX = x * cos - y * sin;
                var newY = x * sin + y * cos;
                x = newX;
                y = newY;
            }

            float Scale(float scale, int index, List<(int, float)> scaleVertexIndices)
            {
                var result = scaleVertexIndices.Select(x => x).Where(x => x.Item1 == index);
                return result.Any() ? (scale * result.First().Item2) : scale;
            }

            var windowPos = ImGui.GetWindowPos();
            var vertexBuffer = ImGui.GetWindowDrawList().VtxBuffer;
            var scale = this.Configuration.General.Scale;
            var rotate = this.Configuration.ProgressBar.Rotate;
            var gaugeWidthCenter = gaugeWidth / 2.0f;
            var gaugeHeightCenter = gaugeHeight / 2.0f;

            for (var i = 0; i < vertexBuffer.Size; i++)
            {
                var scaleHorizontal = Scale(scale, i, this.ScaleHorizontalVertexIndices);
                var scaleVertical = Scale(scale, i, this.ScaleVerticalVertexIndices);

                var x = (vertexBuffer[i].pos.X - windowPos.X - offsetX - gaugeWidthCenter) * scaleHorizontal;
                var y = (vertexBuffer[i].pos.Y - windowPos.Y - offsetY - gaugeHeightCenter) * scaleVertical;

                if (rotate != 0)
                {
                    if (this.RotateVertexIndices.Contains(i))
                        Rotate(ref x, ref y, rotate);
                }

                vertexBuffer[i].pos.X = windowPos.X + offsetX + x + gaugeWidthCenter;
                vertexBuffer[i].pos.Y = windowPos.Y + offsetY + y + gaugeHeightCenter;
            }
        }

        private void DrawMPTickBar()
        {
            var mpTickBarUI = this.GetMPTickBarUI();
            var textureToElementScale = 0.5f;
            var gaugeWidth = mpTickBarUI.Gauge.Width * textureToElementScale;
            var gaugeHeight = (mpTickBarUI.Gauge.Height / 6.0f) * textureToElementScale;
            var offsetX = this.Configuration.General.OffsetX + 20.0f;
            var offsetY = this.Configuration.General.OffsetY + 20.0f;

            this.RenderBackgroundUIElement(mpTickBarUI, offsetX, offsetY, gaugeWidth, gaugeHeight, textureToElementScale, 5, this.Configuration.ProgressBar.BackgroundColor);

            if (this.Configuration.ProgressBar.IsRegressEffectEnabled && this.RegressEffect.IsRegressing)
                this.RenderBarUIElement(mpTickBarUI, offsetX, offsetY, gaugeWidth, gaugeHeight, textureToElementScale, this.RegressEffect.Regress, 4, false, this.Configuration.ProgressBar.RegressBarColor);

            var progressWidth = 296.0f * textureToElementScale;
            var fastFireIIIMarkerOffset = Math.Clamp((3.0f - this.PlayerState.GetFastFireIIICastTime() + this.Configuration.FastFireIIIMarker.TimeOffset) * (progressWidth / 3.0f), 0.0f, progressWidth);
            var isProgressAfterMarker = this.Progress > (fastFireIIIMarkerOffset / progressWidth);

            this.RenderBarUIElement(mpTickBarUI, offsetX, offsetY, gaugeWidth, gaugeHeight, textureToElementScale, this.Progress, 2, true, (isProgressAfterMarker) ? this.Configuration.ProgressBar.ProgressBarAfterMarkerColor : this.Configuration.ProgressBar.ProgressBarColor);
            this.RenderBackgroundUIElement(mpTickBarUI, offsetX, offsetY, gaugeWidth, gaugeHeight, textureToElementScale, 0, this.Configuration.ProgressBar.EdgeColor);

            if ((this.Configuration.FastFireIIIMarker.Visibility == FastFireIIIMarkerVisibility.Visible) ||
               ((this.Configuration.FastFireIIIMarker.Visibility == FastFireIIIMarkerVisibility.UnderUmbralIceIII) && this.PlayerState.IsUmbralIceIIIActivated))
            {
                this.RenderMarkerUIElement(mpTickBarUI, offsetX, offsetY, gaugeHeight, fastFireIIIMarkerOffset, 0, this.Configuration.FastFireIIIMarker.BackgroundColor);
                this.RenderMarkerUIElement(mpTickBarUI, offsetX, offsetY, gaugeHeight, fastFireIIIMarkerOffset, 1, this.Configuration.FastFireIIIMarker.MarkerColor);
            }

            this.AddVertexDataUpToThisPoint();

            if (((this.Configuration.FireIIICastIndicator.Visibility == FireIIICastIndicatorVisibility.Visible) ||
                ((this.Configuration.FireIIICastIndicator.Visibility == FireIIICastIndicatorVisibility.UnderUmbralIceIII) && this.PlayerState.IsUmbralIceIIIActivated)) && isProgressAfterMarker)
                this.RenderFireIIICastIndicator(offsetX, offsetY, gaugeWidth, gaugeHeight);

            if ((this.Configuration.NumberPercentage.Visibility == NumberPercentageVisibility.Visible) ||
               ((this.Configuration.NumberPercentage.Visibility == NumberPercentageVisibility.UnderUmbralIceIII) && this.PlayerState.IsUmbralIceIIIActivated))
                this.RenderNumbers(offsetX, offsetY, gaugeWidth, gaugeHeight);

            this.VertexDataUpdate(offsetX, offsetY, gaugeWidth, gaugeHeight);
        }

        private void DrawMPTickBarWindow()
        {
            var isMPTickBarVisible = (this.PlayerState != null) && this.PlayerState.IsPlayingAsBlackMage &&
               (!this.Configuration.General.IsLocked ||
               (this.Configuration.General.Visibility == MPTickBarVisibility.Visible) ||
               (this.Configuration.General.Visibility == MPTickBarVisibility.InCombat && this.PlayerState.IsInCombat));

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
                if (ImGui.BeginChild("MP Tick Bar (Child)", Vector2.Zero, true, windowFlags))
                    this.DrawMPTickBar();
                ImGui.EndChild();
            }
            ImGui.End();
        }

        private void DrawConfigurationWindow()
        {
            var isConfigurationWindowVisible = this.IsConfigurationWindowVisible;
            if (!isConfigurationWindowVisible)
                return;

            if (ImGui.Begin("MP Tick Bar configuration", ref isConfigurationWindowVisible, ImGuiWindowFlags.AlwaysAutoResize))
            {
                this.IsConfigurationWindowVisible = isConfigurationWindowVisible;

                this.Top();
                if (ImGui.BeginTabBar("Tab Bar"))
                {
                    ImGui.PushItemWidth(180.0f * ImGuiHelpers.GlobalScale);
                    if (ImGui.BeginTabItem("General"))
                    {
                        this.GeneralTab();
                        ImGui.EndTabItem();
                    }
                    if (ImGui.BeginTabItem("Progress Bar"))
                    {
                        this.ProgressBarTab();
                        ImGui.EndTabItem();
                    }
                    if (ImGui.BeginTabItem("Fast Fire III Marker"))
                    {
                        this.FastFireIIIMarkerTab();
                        ImGui.EndTabItem();
                    }
                    if (ImGui.BeginTabItem("Fire III Cast Indicator"))
                    {
                        this.FireIIICastIndicatorTab();
                        ImGui.EndTabItem();
                    }
                    if (ImGui.BeginTabItem("Number (%)"))
                    {
                        this.NumberPercentageTab();
                        ImGui.EndTabItem();
                    }
                    ImGui.PopItemWidth();
                    ImGui.EndTabBar();
                }
                this.Bottom();
            }
            ImGui.End();
        }

        private void GeneralTab()
        {
            var config = this.Configuration.General;
            PluginUI.CollapsingHeader("Location", () =>
            {
                this.CheckBox(config.IsLocked, x => config.IsLocked = x, "Locked");
                this.DragInt(config.OffsetX, config.OffsetY, x => config.OffsetX = x, x => config.OffsetY = x, "Offset", 1, -2000, 2000, "%i");
            });
            PluginUI.CollapsingHeader("Dimension", () =>
            {
                this.DragFloat(config.Scale, x => config.Scale = x, "Scale", 0.01f, 0.1f, 5.0f, "%.2f");
            });
            PluginUI.CollapsingHeader("Functional", () =>
            {
                this.CheckBox(config.IsAutostartEnabled, x => config.IsAutostartEnabled = x, "Autostart");
                PluginUI.Tooltip("Enable the progress bar to start automatically when changing zones or before combat starts (at full MP).\n\nAfter a while, the game stops sending the required data to trigger this functionality. Once you die, it'll work again.");
                this.Combo(config.Visibility, x => config.Visibility = x, "Visibility");
            });
        }

        private void ProgressBarTab()
        {
            var config = this.Configuration.ProgressBar;
            PluginUI.CollapsingHeader("Dimension", () =>
            {
                this.DragFloat(config.ScaleHorizontal, config.ScaleVertical, x => config.ScaleHorizontal = x, x => config.ScaleVertical = x, "Scale", 0.01f, 0.1f, 5.0f, "%.2f");
            });
            PluginUI.CollapsingHeader("Transform", () =>
            {
                this.DragInt(config.Rotate, x => config.Rotate = x, "Rotate (°)", 1, 0, 360, "%i");
            });
            PluginUI.CollapsingHeader("Visual", () =>
            {
                var spacing = ImGui.GetStyle().ItemSpacing.X * ImGuiHelpers.GlobalScale;
                this.ColorEdit4(config.ProgressBarColor, x => config.ProgressBarColor = x, "Progress Bar");
                this.ColorEdit4(config.ProgressBarAfterMarkerColor, x => config.ProgressBarAfterMarkerColor = x, "Progress Bar (After reaching the marker)", spacing);
                this.ColorEdit4(config.BackgroundColor, x => config.BackgroundColor = x, "Background");
                this.ColorEdit4(config.EdgeColor, x => config.EdgeColor = x, "Edge");
                this.CheckBox(config.IsRegressEffectEnabled, x => config.IsRegressEffectEnabled = x, "Regress Effect");
                PluginUI.Tooltip("Show the bar effect animation when it goes from full to an empty state.");
                this.ColorEdit4(config.RegressBarColor, x => config.RegressBarColor = x, "Regress Bar", spacing);
                this.Combo(config.UI, x => config.UI = x, "UI");
            });
        }

        private void FastFireIIIMarkerTab()
        {
            var config = this.Configuration.FastFireIIIMarker;
            PluginUI.CollapsingHeader("Dimension", () =>
            {
                this.DragFloat(config.ScaleHorizontal, config.ScaleVertical, x => config.ScaleHorizontal = x, x => config.ScaleVertical = x, "Scale", 0.01f, 0.1f, 5.0f, "%.2f");
            });
            PluginUI.CollapsingHeader("Visual", () =>
            {
                this.ColorEdit4(config.MarkerColor, x => config.MarkerColor = x, "Marker");
                this.ColorEdit4(config.BackgroundColor, x => config.BackgroundColor = x, "Background");
                this.Combo(config.UI, x => config.UI = x, "UI");
            });
            PluginUI.CollapsingHeader("Functional", () =>
            {
                this.DragFloat(config.TimeOffset, x => config.TimeOffset = x, "Time Offset (s)", 0.01f, 0.0f, 0.5f, "%.2f");
                this.Combo(config.Visibility, x => config.Visibility = x, "Visibility");
            });
        }

        private void FireIIICastIndicatorTab()
        {
            var config = this.Configuration.FireIIICastIndicator;
            PluginUI.CollapsingHeader("Location", () =>
            {
                this.DragInt(config.OffsetX, config.OffsetY, x => config.OffsetX = x, x => config.OffsetY = x, "Offset", 1, -2000, 2000, "%i");
            });
            PluginUI.CollapsingHeader("Dimension", () =>
            {
                this.DragFloat(config.Scale, x => config.Scale = x, "Scale", 0.01f, 0.1f, 5.0f, "%.2f");
            });
            PluginUI.CollapsingHeader("Visual", () =>
            {
                this.ColorEdit4(config.IndicatorColor, x => config.IndicatorColor = x, "Indicator");
            });
            PluginUI.CollapsingHeader("Functional", () =>
            {
                this.Combo(config.Visibility, x => config.Visibility = x, "Visibility");
            });
        }

        private void NumberPercentageTab()
        {
            var config = this.Configuration.NumberPercentage;
            PluginUI.CollapsingHeader("Location", () =>
            {
                this.DragInt(config.OffsetX, config.OffsetY, x => config.OffsetX = x, x => config.OffsetY = x, "Offset", 1, -2000, 2000, "%i");
            });
            PluginUI.CollapsingHeader("Dimension", () =>
            {
                this.DragFloat(config.Scale, x => config.Scale = x, "Scale", 0.01f, 0.1f, 5.0f, "%.2f");
            });
            PluginUI.CollapsingHeader("Visual", () =>
            {
                this.ColorEdit4(config.NumberPercentageColor, x => config.NumberPercentageColor = x, "Number (%)");
            });
            PluginUI.CollapsingHeader("Functional", () =>
            {
                this.Combo(config.Visibility, x => config.Visibility = x, "Visibility");
            });
        }

        private void Top()
        {
            var iconDimension = 23.0f * ImGuiHelpers.GlobalScale;
            ImGui.Image(this.FireIIIIcon.ImGuiHandle, new(iconDimension, iconDimension));
            var fastFireIIICastTime = (this.PlayerState != null) && this.PlayerState.IsPlayingAsBlackMage ? (((int)(this.PlayerState.GetFastFireIIICastTime() * 100)) / 100.0f).ToString("0.00s") : "N/A";
            PluginUI.Tooltip($"Current fast Fire III cast time: {fastFireIIICastTime}");
            ImGui.SameLine();
            ImGui.TextColored(new(0.0f, 1.0f, 0.0f, 1.0f), fastFireIIICastTime);
        }

        private void Bottom()
        {
            ImGui.Separator();
            if (ImGui.Button("Reset all settings to default"))
            {
                this.Configuration.Reset();
                this.Configuration.Save();
            }
        }
    }
}