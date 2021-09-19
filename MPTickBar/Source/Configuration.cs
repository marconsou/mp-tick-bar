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

        public bool IsMPTickBarVisible { get; set; } = true;

        public bool IsMPTickBarLocked { get; set; } = false;

        public bool IsFastFireIIIMarkerVisible { get; set; } = true;

        public int FastFireIIIMarkerOffset { get; set; } = 10;

        public float FireIIICastTime { get; set; } = 3.2f;

        public float UIScale { get; set; } = 1.0f;

        public Vector3 ProgressBarTintColor { get; set; } = new Vector3(0.0f, 1.0f, 1.0f);

        public Vector3 FastFireIIIMarkerTintColor { get; set; } = new Vector3(1.0f, 0.375f, 0.375f);

        public FastFireIIIMarkerType FastFireIIIMarkerType { get; set; } = FastFireIIIMarkerType.Icon;

        public UIType UIType { get; set; } = UIType.FinalFantasyXIVDefault;

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