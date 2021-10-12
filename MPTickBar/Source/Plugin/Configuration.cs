﻿using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Numerics;

namespace MPTickBar
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 3;

        public bool IsMPTickBarLocked { get; set; } = false;

        public bool IsRegressEffectVisible { get; set; } = true;

        public int MPTickBarOffsetX { get; set; } = 0;

        public int MPTickBarOffsetY { get; set; } = 0;

        public int FireIIICastIndicatorOffsetX { get; set; } = 0;

        public int FireIIICastIndicatorOffsetY { get; set; } = 0;

        public int NumberPercentageOffsetX { get; set; } = 0;

        public int NumberPercentageOffsetY { get; set; } = 0;

        public float MPTickBarScale { get; set; } = 1.0f;

        public float ProgressBarScaleHorizontal { get; set; } = 1.0f;

        public float ProgressBarScaleVertical { get; set; } = 1.0f;

        public float FireIIICastIndicatorScale { get; set; } = 1.0f;

        public float NumberPercentageScale { get; set; } = 1.0f;

        public float FastFireIIIMarkerTimeOffset { get; set; } = 0.10f;

        public Vector3 ProgressBarTintColor { get; set; } = new Vector3(0.0f, 1.0f, 1.0f);

        public Vector3 FastFireIIIMarkerTintColor { get; set; } = new Vector3(1.0f, 0.375f, 0.375f);

        public Vector3 FireIIICastIndicatorTintColor { get; set; } = new Vector3(1.0f, 0.375f, 0.375f);

        public Vector3 NumberPercentageTintColor { get; set; } = new Vector3(1.0f, 0.98f, 0.94f);

        public MPTickBarVisibility MPTickBarVisibility { get; set; } = MPTickBarVisibility.Visible;

        public UIType UIType { get; set; } = UIType.FinalFantasyXIVDefault;

        public FastFireIIIMarkerVisibility FastFireIIIMarkerVisibility { get; set; } = FastFireIIIMarkerVisibility.Visible;

        public FastFireIIIMarkerType FastFireIIIMarkerType { get; set; } = FastFireIIIMarkerType.Icon;

        public FireIIICastIndicatorVisibility FireIIICastIndicatorVisibility { get; set; } = FireIIICastIndicatorVisibility.UnderUmbralIceIII;

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