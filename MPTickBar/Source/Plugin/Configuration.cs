using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Numerics;

namespace MPTickBar
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 1;

        public bool IsMPTickBarLocked { get; set; } = false;

        public bool IsRegressEffectVisible { get; set; } = false;

        public int NumberPercentageOffsetX { get; set; } = 0;

        public int NumberPercentageOffsetY { get; set; } = 0;

        public float UIScale { get; set; } = 1.0f;

        public float FastFireIIIMarkerTimeOffset { get; set; } = 0.20f;

        public Vector3 ProgressBarTintColor { get; set; } = new Vector3(0.0f, 1.0f, 1.0f);

        public Vector3 FastFireIIIMarkerTintColor { get; set; } = new Vector3(1.0f, 0.375f, 0.375f);

        public Vector3 NumberPercentageTintColor { get; set; } = new Vector3(1.0f, 0.98f, 0.94f);

        public MPTickBarVisibility MPTickBarVisibility { get; set; } = MPTickBarVisibility.Visible;

        public UIType UIType { get; set; } = UIType.FinalFantasyXIVDefault;

        public FastFireIIIMarkerVisibility FastFireIIIMarkerVisibility { get; set; } = FastFireIIIMarkerVisibility.Visible;

        public FastFireIIIMarkerType FastFireIIIMarkerType { get; set; } = FastFireIIIMarkerType.Icon;

        public NumberPercentageVisibility NumberPercentageVisibility { get; set; } = NumberPercentageVisibility.Hidden;

        [NonSerialized]
        private DalamudPluginInterface pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface.SavePluginConfig(this);
        }
    }
}