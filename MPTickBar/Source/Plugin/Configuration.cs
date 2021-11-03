using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Numerics;

namespace MPTickBar
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 5;

        public GeneralTab General { get; set; }

        public ProgressBarTab ProgressBar { get; set; }

        public FastFireIIIMarkerTab FastFireIIIMarker { get; set; }

        public FireIIICastIndicatorTab FireIIICastIndicator { get; set; }

        public NumberPercentageTab NumberPercentage { get; set; }

        public class GeneralTab
        {
            public bool IsLocked { get; set; } = true;

            public int OffsetX { get; set; } = 0;

            public int OffsetY { get; set; } = 0;

            public float Scale { get; set; } = 1.0f;

            public bool IsAutostartEnabled { get; set; } = true;

            public MPTickBarVisibility Visibility { get; set; } = MPTickBarVisibility.Visible;
        }

        public class ProgressBarTab
        {
            public float ScaleHorizontal { get; set; } = 1.0f;

            public float ScaleVertical { get; set; } = 1.0f;

            public int Rotate { get; set; } = 0;

            public Vector4 ProgressBarColor { get; set; } = new Vector4(0.0f, 1.0f, 1.0f, 1.0f);

            public Vector4 ProgressBarAfterMarkerColor { get; set; } = new Vector4(0.0f, 1.0f, 1.0f, 1.0f);

            public Vector4 BackgroundColor { get; set; } = Vector4.One;

            public Vector4 EdgeColor { get; set; } = Vector4.One;

            public bool IsRegressEffectEnabled { get; set; } = true;

            public Vector4 RegressBarColor { get; set; } = Vector4.One;

            public ProgressBarUI UI { get; set; } = ProgressBarUI.Default;
        }

        public class FastFireIIIMarkerTab
        {
            public Vector4 MarkerColor { get; set; } = new Vector4(1.0f, 0.25f, 0.25f, 1.0f);

            public Vector4 BackgroundColor { get; set; } = Vector4.One;

            public FastFireIIIMarkerUI UI { get; set; } = FastFireIIIMarkerUI.Default;

            public float TimeOffset { get; set; } = 0.0f;

            public FastFireIIIMarkerVisibility Visibility { get; set; } = FastFireIIIMarkerVisibility.Visible;
        }

        public class FireIIICastIndicatorTab
        {
            public int OffsetX { get; set; } = 0;

            public int OffsetY { get; set; } = 0;

            public float Scale { get; set; } = 1.0f;

            public Vector4 IndicatorColor { get; set; } = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);

            public FireIIICastIndicatorVisibility Visibility { get; set; } = FireIIICastIndicatorVisibility.Visible;
        }

        public class NumberPercentageTab
        {
            public int OffsetX { get; set; } = 0;

            public int OffsetY { get; set; } = 0;

            public float Scale { get; set; } = 1.0f;

            public Vector4 NumberPercentageColor { get; set; } = new Vector4(1.0f, 0.98f, 0.94f, 1.0f);

            public NumberPercentageVisibility Visibility { get; set; } = NumberPercentageVisibility.Hidden;
        }

        [NonSerialized]
        private DalamudPluginInterface pluginInterface;

        public Configuration() => this.Reset();

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface.SavePluginConfig(this);
        }

        public void Reset()
        {
            this.General = new GeneralTab();
            this.ProgressBar = new ProgressBarTab();
            this.FastFireIIIMarker = new FastFireIIIMarkerTab();
            this.FireIIICastIndicator = new FireIIICastIndicatorTab();
            this.NumberPercentage = new NumberPercentageTab();
        }
    }
}