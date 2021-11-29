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

        private TextureWrap JobStackADefault { get; }

        private TextureWrap JobStackAMaterialUI { get; }

        private TextureWrap JobStackAMaterialUISilver { get; }

        private TextureWrap JobStackBDefault { get; }

        private TextureWrap JobStackBMaterialUI { get; }

        private TextureWrap JobStackBMaterialUISilver { get; }

        private TextureWrap MarkerLine { get; }

        private TextureWrap FireIIICastIndicator { get; }

        private TextureWrap Numbers { get; }

        private TextureWrap FireIIIIcon { get; }

        private bool IsConfigurationWindowVisible { get; set; }

        private double Progress { get; set; }

        private RegressEffectData RegressEffect { get; } = new();

        private List<int> RotateVertexIndices { get; } = new();

        private List<(int, float)> ScaleHorizontalVertexIndices { get; } = new();

        private List<(int, float)> ScaleVerticalVertexIndices { get; } = new();

        private class MPTickBarUI
        {
            public TextureWrap Bar { get; init; }

            public TextureWrap Marker { get; init; }

            public TextureWrap UmbralIceRegenStack { get; init; }

            public TextureWrap LucidDreamingRegenStack { get; init; }
        }

        private class RegressEffectData
        {
            public bool IsRegressing => this.Regress > 0;

            public double Regress { get; private set; }

            private double LastProgress { get; set; }

            private double TimeStayingPaused { get; set; }

            private double TimeSpeedInterval { get; set; }

            private double Speed { get; set; }

            public void Update(double progress)
            {
                var time = ImGui.GetTime();

                if ((this.LastProgress > progress) && this.TimeStayingPaused == 0.0)
                {
                    this.TimeStayingPaused = time + 0.200;
                    this.Regress = this.LastProgress;
                    this.Speed = this.LastProgress / 20.0;
                }

                if ((time > this.TimeStayingPaused) && (time > this.TimeSpeedInterval))
                {
                    this.TimeSpeedInterval = time + 0.008;
                    this.Regress = Math.Clamp(this.Regress - this.Speed, 0.0, 1.0);

                    if (this.Regress == 0.0)
                        this.TimeStayingPaused = 0.0;
                }

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
            this.JobStackADefault = uiBuilder.LoadImage(Resources.JobStackADefault);
            this.JobStackAMaterialUI = uiBuilder.LoadImage(Resources.JobStackAMaterialUI);
            this.JobStackAMaterialUISilver = uiBuilder.LoadImage(Resources.JobStackAMaterialUISilver);
            this.JobStackBDefault = uiBuilder.LoadImage(Resources.JobStackBDefault);
            this.JobStackBMaterialUI = uiBuilder.LoadImage(Resources.JobStackBMaterialUI);
            this.JobStackBMaterialUISilver = uiBuilder.LoadImage(Resources.JobStackBMaterialUISilver);
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
            this.JobStackADefault.Dispose();
            this.JobStackAMaterialUI.Dispose();
            this.JobStackAMaterialUISilver.Dispose();
            this.JobStackBDefault.Dispose();
            this.JobStackBMaterialUI.Dispose();
            this.JobStackBMaterialUISilver.Dispose();
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

        public void OpenConfigUi() => this.IsConfigurationWindowVisible = !this.IsConfigurationWindowVisible;

        public void Update(double progress)
        {
            this.Progress = progress;
            this.RegressEffect.Update(progress);
        }

        private MPTickBarUI GetMPTickBarUI()
        {
            var bar = this.Configuration.ProgressBar.UI switch
            {
                ProgressBarUI.Default => this.GaugeDefault,
                ProgressBarUI.MaterialUIDiscord => this.GaugeMaterialUIDiscord,
                ProgressBarUI.MaterialUIBlack => this.GaugeMaterialUIBlack,
                ProgressBarUI.MaterialUISilver => this.GaugeMaterialUISilver,
                ProgressBarUI.SolidBar => this.GaugeSolidBar,
                _ => this.GaugeDefault,
            };

            var marker = this.Configuration.FastFireIIIMarker.UI switch
            {
                FastFireIIIMarkerUI.Default => this.JobStackADefault,
                FastFireIIIMarkerUI.MaterialUI => this.JobStackAMaterialUI,
                FastFireIIIMarkerUI.MaterialUISilver => this.JobStackAMaterialUISilver,
                FastFireIIIMarkerUI.Line => this.MarkerLine,
                _ => this.JobStackADefault,
            };

            var umbralIceRegenStack = this.Configuration.MPRegenStack.UI switch
            {
                MPRegenStackUI.Default => this.JobStackBDefault,
                MPRegenStackUI.MaterialUI => this.JobStackBMaterialUI,
                MPRegenStackUI.MaterialUISilver => this.JobStackBMaterialUISilver,
                _ => this.JobStackBDefault,
            };

            var lucidDreamingRegenStack = this.Configuration.MPRegenStack.UI switch
            {
                MPRegenStackUI.Default => this.JobStackADefault,
                MPRegenStackUI.MaterialUI => this.JobStackAMaterialUI,
                MPRegenStackUI.MaterialUISilver => this.JobStackAMaterialUISilver,
                _ => this.JobStackADefault,
            };

            return new() { Bar = bar, Marker = marker, UmbralIceRegenStack = umbralIceRegenStack, LucidDreamingRegenStack = lucidDreamingRegenStack };
        }

        private void RenderBackgroundUIElement(MPTickBarUI mpTickBarUI, float offsetX, float offsetY, float gaugeWidth, float gaugeHeight, float textureToElementScale, bool isBackground)
        {
            var x = offsetX;
            var y = offsetY;
            var width = gaugeWidth;
            var height = gaugeHeight;
            var textureElementHeight = gaugeHeight / textureToElementScale;
            var textureX = 0.0f;
            var textureY = (textureElementHeight * (!isBackground ? 0 : 5)) / mpTickBarUI.Bar.Height;
            var textureW = 1.0f;
            var textureH = textureY + (textureElementHeight / mpTickBarUI.Bar.Height);
            ImGui.SetCursorPos(new(x, y));
            ImGui.Image(mpTickBarUI.Bar.ImGuiHandle, new(width, height), new(textureX, textureY), new(textureW, textureH), isBackground ? this.Configuration.ProgressBar.BackgroundColor : this.Configuration.ProgressBar.EdgeColor);
        }

        private void RenderBarUIElement(MPTickBarUI mpTickBarUI, float offsetX, float offsetY, float gaugeWidth, float gaugeHeight, float textureToElementScale, double progress, bool isProgress, bool isProgressAfterMarker)
        {
            var barTextureOffsetX = 12.0f * textureToElementScale;
            var x = offsetX + barTextureOffsetX;
            var y = offsetY;
            var width = (float)((gaugeWidth - (barTextureOffsetX * 2.0f)) * progress);
            var height = gaugeHeight;
            var textureElementX = barTextureOffsetX / textureToElementScale;
            var textureElementHeight = gaugeHeight / textureToElementScale;
            var textureX = textureElementX / mpTickBarUI.Bar.Width;
            var textureY = (textureElementHeight * (isProgress ? 2 : 4)) / mpTickBarUI.Bar.Height;
            var textureW = textureX + (float)((1.0f - (textureX * 2.0f)) * (isProgress ? progress : 1.0f));
            var textureH = textureY + (textureElementHeight / mpTickBarUI.Bar.Height);
            var color = !isProgress ? this.Configuration.ProgressBar.RegressBarColor : (isProgressAfterMarker) ? this.Configuration.ProgressBar.ProgressBarAfterMarkerColor : this.Configuration.ProgressBar.ProgressBarColor;
            ImGui.SetCursorPos(new(x, y));
            ImGui.Image(mpTickBarUI.Bar.ImGuiHandle, new(width, height), new(textureX, textureY), new(textureW, textureH), color);
        }

        private void RenderMarkerUIElement(MPTickBarUI mpTickBarUI, float offsetX, float offsetY, float dimension, float fastFireIIIMarkerOffset, bool isBackground)
        {
            var adjustX = -4.0f;
            var x = offsetX + adjustX + fastFireIIIMarkerOffset;
            var y = offsetY;
            var width = dimension * this.Configuration.FastFireIIIMarker.ScaleHorizontal;
            var height = dimension * this.Configuration.FastFireIIIMarker.ScaleVertical;
            var textureX = (!isBackground ? 1 : 0) * 0.5f;
            var textureY = 0.0f;
            var textureW = textureX + 0.5f;
            var textureH = 1.0f;
            ImGui.SetCursorPos(new(x - ((width - dimension) / 2.0f), y - ((height - dimension) / 2.0f)));
            ImGui.Image(mpTickBarUI.Marker.ImGuiHandle, new(width, height), new(textureX, textureY), new(textureW, textureH), isBackground ? this.Configuration.FastFireIIIMarker.BackgroundColor : this.Configuration.FastFireIIIMarker.MarkerColor);
        }

        private void RenderIndicatorUIElement(float offsetX, float offsetY, float gaugeWidth, float gaugeHeight)
        {
            var adjustX = 20.0f;
            var x = offsetX + this.Configuration.FireIIICastIndicator.OffsetX + gaugeWidth + adjustX;
            var y = offsetY + this.Configuration.FireIIICastIndicator.OffsetY + (gaugeHeight / 2.0f);
            var width = this.FireIIICastIndicator.Width * this.Configuration.FireIIICastIndicator.Scale;
            var height = this.FireIIICastIndicator.Height * this.Configuration.FireIIICastIndicator.Scale;
            var textureX = 0.0f;
            var textureY = 0.0f;
            var textureW = 1.0f;
            var textureH = 1.0f;
            ImGui.SetCursorPos(new(x - (width / 2.0f), y - (height / 2.0f)));
            ImGui.Image(this.FireIIICastIndicator.ImGuiHandle, new(width, height), new(textureX, textureY), new(textureW, textureH), this.Configuration.FireIIICastIndicator.IndicatorColor);
        }

        private void RenderStacksUIElement(MPTickBarUI mpTickBarUI, float offsetX, float offsetY, float gaugeWidth, float dimension, bool isBackground)
        {
            var x = offsetX + this.Configuration.MPRegenStack.OffsetX + (gaugeWidth / 4.0f);
            var y = offsetY + this.Configuration.MPRegenStack.OffsetY - 7.0f;
            var width = dimension * this.Configuration.MPRegenStack.Scale;
            var height = dimension * this.Configuration.MPRegenStack.Scale;
            var textureX = (!isBackground ? 1 : 0) * 0.5f;
            var textureY = 0.0f;
            var textureW = textureX + 0.5f;
            var textureH = 1.0f;
            var stacks = 4;
            var widthAdjust = 0.65f;
            var totalWidth = (width * (1.0f - widthAdjust)) + (width * widthAdjust * stacks);

            void RenderImage(IntPtr imGuiHandle, Vector4 color)
            {
                ImGui.SetCursorPos(new(x - (totalWidth / 2.0f), y - (height / 2.0f)));
                ImGui.Image(imGuiHandle, new(width, height), new(textureX, textureY), new(textureW, textureH), color);
            }

            for (var i = 0; i < stacks - 1; i++)
            {
                if (isBackground || (i < this.PlayerState.UmbralIceRegenStack))
                    RenderImage(mpTickBarUI.UmbralIceRegenStack.ImGuiHandle, isBackground ? this.Configuration.MPRegenStack.UmbralIceStackBackgroundColor : this.Configuration.MPRegenStack.UmbralIceStackColor);
                x += width * widthAdjust;
            }

            x += 2.0f * this.Configuration.MPRegenStack.Scale;

            if (isBackground || this.PlayerState.LucidDreamingRegenStack)
                RenderImage(mpTickBarUI.LucidDreamingRegenStack.ImGuiHandle, isBackground ? this.Configuration.MPRegenStack.LucidDreamingStackBackgroundColor : this.Configuration.MPRegenStack.LucidDreamingStackColor);
        }

        private void RenderNumbersUIElement(float offsetX, float offsetY, float gaugeWidth, float gaugeHeight)
        {
            var adjustY = 10.0f;
            var digitTotal = 10.0f;
            var width = (this.Numbers.Width / digitTotal);
            var height = this.Numbers.Height;
            var scaledWidth = width * this.Configuration.Number.Scale;
            var scaledHeight = height * this.Configuration.Number.Scale;
            var textureY = 0.0f;
            var textureH = 1.0f;
            var number = (this.Configuration.Number.Type == NumberType.RemainingTime) ? (int)Math.Abs(((!this.Configuration.Number.Reverse ? 0.0 : 3.0) - (this.Progress * 3.0)) * 10.0) :
                         (this.Configuration.Number.Type == NumberType.Percentage) ? (int)Math.Abs((!this.Configuration.Number.Reverse ? 0.0 : 100.0) - (this.Progress * 100.0)) : 0;
            var numberText = (this.Configuration.Number.Type == NumberType.RemainingTime) ? number.ToString("00") : number.ToString();
            if (numberText.All(x => x == '0'))
                numberText = "0";

            var x = offsetX + this.Configuration.Number.OffsetX + (gaugeWidth / 2.0f);
            var y = offsetY + this.Configuration.Number.OffsetY + (gaugeHeight / 2.0f) + adjustY;
            var totalNumberWidth = scaledWidth * numberText.Length;

            if ((this.Configuration.Number.Type == NumberType.RemainingTime) && (numberText.Length > 1))
            {
                var pos = ImGui.GetWindowPos() + new Vector2(x, y + (scaledHeight * 0.75f / 2.0f));
                ImGui.GetWindowDrawList().AddCircleFilled(new Vector2(pos.X, pos.Y), 1.1f * this.Configuration.Number.Scale, ImGui.ColorConvertFloat4ToU32(this.Configuration.Number.NumberColor));
            }

            foreach (var item in numberText)
            {
                var digit = char.GetNumericValue(item);
                var textureX = (width * digit) / this.Numbers.Width;
                var textureW = textureX + (width / this.Numbers.Width);
                ImGui.SetCursorPos(new(x - (totalNumberWidth / 2.0f), y - (scaledHeight / 2.0f)));
                ImGui.Image(this.Numbers.ImGuiHandle, new(scaledWidth, scaledHeight), new((float)textureX, textureY), new((float)textureW, textureH), this.Configuration.Number.NumberColor);
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
            var gaugeWidth = mpTickBarUI.Bar.Width * textureToElementScale;
            var gaugeHeight = (mpTickBarUI.Bar.Height / 6.0f) * textureToElementScale;
            var offsetX = this.Configuration.General.OffsetX + 20.0f;
            var offsetY = this.Configuration.General.OffsetY + 20.0f;

            this.RenderBackgroundUIElement(mpTickBarUI, offsetX, offsetY, gaugeWidth, gaugeHeight, textureToElementScale, true);

            if (this.Configuration.ProgressBar.IsRegressEffectEnabled && this.RegressEffect.IsRegressing)
                this.RenderBarUIElement(mpTickBarUI, offsetX, offsetY, gaugeWidth, gaugeHeight, textureToElementScale, this.RegressEffect.Regress, false, false);

            var progressWidth = 296.0f * textureToElementScale;
            var fastFireIIIMarkerOffset = Math.Clamp((3.0f - this.PlayerState.GetFastFireIIICastTime() + this.Configuration.FastFireIIIMarker.TimeOffset) * (progressWidth / 3.0f), 0.0f, progressWidth);
            var isProgressAfterMarker = this.Progress > (fastFireIIIMarkerOffset / progressWidth);

            this.RenderBarUIElement(mpTickBarUI, offsetX, offsetY, gaugeWidth, gaugeHeight, textureToElementScale, this.Progress, true, isProgressAfterMarker);
            this.RenderBackgroundUIElement(mpTickBarUI, offsetX, offsetY, gaugeWidth, gaugeHeight, textureToElementScale, false);

            if ((this.Configuration.FastFireIIIMarker.Visibility == FastFireIIIMarkerVisibility.Visible) ||
               ((this.Configuration.FastFireIIIMarker.Visibility == FastFireIIIMarkerVisibility.UnderUmbralIceIII) && this.PlayerState.IsUmbralIceIIIActivated))
            {
                this.RenderMarkerUIElement(mpTickBarUI, offsetX, offsetY, gaugeHeight, fastFireIIIMarkerOffset, true);
                this.RenderMarkerUIElement(mpTickBarUI, offsetX, offsetY, gaugeHeight, fastFireIIIMarkerOffset, false);
            }

            this.AddVertexDataUpToThisPoint();

            if (((this.Configuration.FireIIICastIndicator.Visibility == FireIIICastIndicatorVisibility.Visible) ||
                ((this.Configuration.FireIIICastIndicator.Visibility == FireIIICastIndicatorVisibility.UnderUmbralIceIII) && this.PlayerState.IsUmbralIceIIIActivated)) && isProgressAfterMarker)
                this.RenderIndicatorUIElement(offsetX, offsetY, gaugeWidth, gaugeHeight);

            if ((this.Configuration.MPRegenStack.Visibility == MPRegenStackVisibility.Visible) ||
               ((this.Configuration.MPRegenStack.Visibility == MPRegenStackVisibility.UnderUmbralIce) && this.PlayerState.IsUmbralIceActivated))
            {
                this.RenderStacksUIElement(mpTickBarUI, offsetX, offsetY, gaugeWidth, gaugeHeight, true);
                this.RenderStacksUIElement(mpTickBarUI, offsetX, offsetY, gaugeWidth, gaugeHeight, false);
            }

            if ((this.Configuration.Number.Visibility == NumberVisibility.Visible) ||
               ((this.Configuration.Number.Visibility == NumberVisibility.WhileInProgress) && (this.Progress != 0.0)))
                this.RenderNumbersUIElement(offsetX, offsetY, gaugeWidth, gaugeHeight);

            this.VertexDataUpdate(offsetX, offsetY, gaugeWidth, gaugeHeight);
        }

        private void DrawMPTickBarWindow()
        {
            var isMPTickBarWindowVisible = (this.PlayerState != null) && this.PlayerState.IsPlayingAsBlackMage && !this.PlayerState.IsBetweenAreas && !this.PlayerState.IsOccupied &&
               (!this.Configuration.General.IsLocked ||
               (this.Configuration.General.Visibility == MPTickBarVisibility.Visible) ||
               (this.Configuration.General.Visibility == MPTickBarVisibility.InCombat && this.PlayerState.IsInCombat));

            if (!isMPTickBarWindowVisible)
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

            ImGui.SetNextWindowPos(new(700.0f, 450.0f), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(new(250.0f, 70.0f), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("MP Tick Bar", ref isMPTickBarWindowVisible, windowFlags))
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
                    if (ImGui.BeginTabItem("MP Regen Stack"))
                    {
                        this.MPRegenTab();
                        ImGui.EndTabItem();
                    }
                    if (ImGui.BeginTabItem("Number"))
                    {
                        this.NumberTab();
                        ImGui.EndTabItem();
                    }
                    if (ImGui.BeginTabItem("Countdown"))
                    {
                        this.CountdownTab();
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
            PluginUI.CollapsingHeader("Information", () =>
            {
                ImGui.Text("All options in this tab will apply to all UI elements at once." +
                    "\nUse the [Offset] option if an UI element is clipping.");
            });
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
                this.Combo(config.Visibility, x => config.Visibility = x, "Visibility");
            });
        }

        private void ProgressBarTab()
        {
            var config = this.Configuration.ProgressBar;
            PluginUI.CollapsingHeader("Information", () =>
            {
                ImGui.Text("The progress bar will start working based on:" +
                    "\n-Umbral Ice MP regen." +
                    "\n-Natural MP regen." +
                    "\n-At the beginning of the instanced duty or zone, under certain conditions." +
                    "\n-Changing your HP: re-quipping gear, using food, taking damage, etc.");
            });
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
            PluginUI.CollapsingHeader("Information", () =>
            {
                ImGui.Text("The marker indicates when you can cast a fast Fire III safely without losing the next regen tick." +
                    "\nAdjust the [Time Offset] option if needed." +
                    "\nThe marker will adjust automatically based on:" +
                    "\n-Spell Speed." +
                    "\n-Ley Lines.");
            });
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
            PluginUI.CollapsingHeader("Information", () =>
            {
                ImGui.Text("The indicator will show up when the progress reaches the marker.");
            });
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

        private void MPRegenTab()
        {
            var config = this.Configuration.MPRegenStack;
            PluginUI.CollapsingHeader("Information", () =>
            {
                ImGui.Text("The MP Regen Stack contains 4 stacks:" +
                    "\n-The first three stacks represent the state of the current MP." +
                    "\n (e.g Almost empty MP = 0 stack. Almost full MP = 3 stacks)" +
                    "\n-The last one represents the Lucid Dreaming regen." +
                    "\n-Umbral Ice I regen grants 1 stack." +
                    "\n-Umbral Ice III regen grants 2 stacks.");
            });
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
                var spacing = ImGui.GetStyle().ItemSpacing.X * ImGuiHelpers.GlobalScale;
                this.ColorEdit4(config.UmbralIceStackColor, x => config.UmbralIceStackColor = x, "Umbral Ice Stack");
                this.ColorEdit4(config.UmbralIceStackBackgroundColor, x => config.UmbralIceStackBackgroundColor = x, "Background", spacing);
                this.ColorEdit4(config.LucidDreamingStackColor, x => config.LucidDreamingStackColor = x, "Lucid Dreaming Stack");
                this.ColorEdit4(config.LucidDreamingStackBackgroundColor, x => config.LucidDreamingStackBackgroundColor = x, "Background ", spacing);
                this.Combo(config.UI, x => config.UI = x, "UI");
            });
            PluginUI.CollapsingHeader("Functional", () =>
            {
                this.Combo(config.Visibility, x => config.Visibility = x, "Visibility");
            });
        }

        private void NumberTab()
        {
            var config = this.Configuration.Number;
            PluginUI.CollapsingHeader("Information", () =>
            {
                ImGui.Text("The number represents the remaining time or percentage of the progress.");
            });
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
                this.ColorEdit4(config.NumberColor, x => config.NumberColor = x, "Number");
            });
            PluginUI.CollapsingHeader("Functional", () =>
            {
                var spacing = ImGui.GetStyle().ItemSpacing.X * ImGuiHelpers.GlobalScale;
                this.Combo(config.Type, x => config.Type = x, "Type");
                this.CheckBox(config.Reverse, x => config.Reverse = x, "Reverse", spacing);
                this.Combo(config.Visibility, x => config.Visibility = x, "Visibility");
            });
        }

        private void CountdownTab()
        {
            var config = this.Configuration.Countdown;
            PluginUI.CollapsingHeader("Information", () =>
            {
                ImGui.Text($"Type {MPTickBarPlugin.CountdownCommand} X to start the countdown with X seconds after next tick and time offset. (e.g. {MPTickBarPlugin.CountdownCommand} 12)" +
                    "\nAdjust the [Time Offset] option based on your opener/encounter requirements.");
            });
            PluginUI.CollapsingHeader("Functional", () =>
            {
                this.DragInt(config.StartingSeconds, x => config.StartingSeconds = x, "Starting Seconds (Default)", 1, 5, 30, "%i");
                PluginUI.Tooltip($"The default value used when {MPTickBarPlugin.CountdownCommand} command has invalid or no value.");
                this.DragFloat(config.TimeOffset, x => config.TimeOffset = x, "Time Offset (s)", 0.01f, 0.0f, 3.0f, "%.2f");
            });
        }

        private void Bottom()
        {
            ImGui.Separator();
            if (ImGui.Button("Reset all settings to default"))
            {
                this.Configuration.Reset();
                this.Configuration.Save();
            }

            var iconDimension = 23.0f * ImGuiHelpers.GlobalScale;
            var fastFireIIICastTime = (this.PlayerState != null) && this.PlayerState.IsPlayingAsBlackMage ? (((int)(this.PlayerState.GetFastFireIIICastTime() * 100)) / 100.0f).ToString("0.00s") : "N/A";
            var textWidth = ImGui.CalcTextSize(fastFireIIICastTime).X;
            ImGui.SameLine(ImGui.GetWindowWidth() - textWidth - iconDimension - 32.0f);
            ImGui.TextColored(new(0.0f, 1.0f, 0.0f, 1.0f), fastFireIIICastTime);
            ImGui.SameLine();
            ImGui.Image(this.FireIIIIcon.ImGuiHandle, new(iconDimension, iconDimension));
            PluginUI.Tooltip($"Current fast Fire III cast time: {fastFireIIICastTime}");
        }
    }
}