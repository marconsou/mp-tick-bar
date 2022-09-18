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

        public GeneralTab General { get; set; }

        public ProgressBarTab ProgressBar { get; set; }

        public FastFireIIIMarkerTab FastFireIIIMarker { get; set; }

        public TimeSplitMarkerTab TimeSplitMarker { get; set; }

        public FireIIICastIndicatorTab FireIIICastIndicator { get; set; }

        public MPRegenStackTab MPRegenStack { get; set; }

        public NumberTab Number { get; set; }

        public CountdownTab Countdown { get; set; }

        public class GeneralTab
        {
            public bool IsLocked { get; set; } = false;

            public int OffsetX { get; set; } = 0;

            public int OffsetY { get; set; } = 0;

            public float Scale { get; set; } = 1.0f;

            public bool IsDarkKnightEnabled { get; set; } = false;

            public bool IsAlwaysEnabled { get; set; } = true;

            public bool IsWhileAliveEnabled { get; set; } = false;

            public bool IsWhileInCombatEnabled { get; set; } = false;

            public bool IsWhileWeaponUnsheathedEnabled { get; set; } = false;

            public bool IsWhileUnderUmbralIceEnabled { get; set; } = false;

            public bool IsAlwaysOtherJobsEnabled { get; set; } = true;

            public bool IsWhileAliveOtherJobsEnabled { get; set; } = false;

            public bool IsWhileInCombatOtherJobsEnabled { get; set; } = false;

            public bool IsWhileWeaponUnsheathedOtherJobsEnabled { get; set; } = false;
        }

        public class ProgressBarTab
        {
            public float ScaleHorizontal { get; set; } = 1.0f;

            public float ScaleVertical { get; set; } = 1.0f;

            public int Rotate { get; set; } = 0;

            public Vector4 ProgressBarColor { get; set; } = new(0.0f, 1.0f, 1.0f, 1.0f);

            public Vector4 BackgroundColor { get; set; } = Vector4.One;

            public Vector4 EdgeColor { get; set; } = Vector4.One;

            public bool IsProgressBarAfterMarkerEnabled { get; set; } = true;

            public Vector4 ProgressBarAfterMarkerColor { get; set; } = new(0.0f, 0.65f, 1.0f, 1.0f);

            public bool IsRegressEffectEnabled { get; set; } = true;

            public Vector4 RegressBarColor { get; set; } = Vector4.One;

            public ProgressBarUI UI { get; set; } = ProgressBarUI.Default;
        }

        public class FastFireIIIMarkerTab
        {
            public float ScaleHorizontal { get; set; } = 1.0f;

            public float ScaleVertical { get; set; } = 1.0f;

            public Vector4 MarkerColor { get; set; } = new(1.0f, 0.25f, 0.25f, 1.0f);

            public Vector4 BackgroundColor { get; set; } = Vector4.One;

            public FastFireIIIMarkerUI UI { get; set; } = FastFireIIIMarkerUI.Default;

            public float TimeOffset { get; set; } = 0.25f;

            public bool IsAlwaysEnabled { get; set; } = true;
        }

        public class TimeSplitMarkerTab
        {
            public float ScaleHorizontal { get; set; } = 1.0f;

            public float ScaleVertical { get; set; } = 1.0f;

            public Vector4 MarkerColor { get; set; } = new(0.25f, 1.0f, 0.25f, 1.0f);

            public Vector4 BackgroundColor { get; set; } = Vector4.One;

            public TimeSplitMarkerUI UI { get; set; } = TimeSplitMarkerUI.Default;

            public bool IsSingleMarkerEnabled { get; set; } = false;

            public float SingleMarkerTimeOffset { get; set; } = 1.5f;

            public bool IsMultipleMarkersEnabled { get; set; } = false;

            public int MultipleMarkersAmount { get; set; } = 2;
        }

        public class FireIIICastIndicatorTab
        {
            public int OffsetX { get; set; } = 0;

            public int OffsetY { get; set; } = 0;

            public float Scale { get; set; } = 1.0f;

            public Vector4 IndicatorColor { get; set; } = new(1.0f, 0.0f, 0.0f, 1.0f);

            public bool IsAlwaysEnabled { get; set; } = false;
        }

        public class MPRegenStackTab
        {
            public int OffsetX { get; set; } = 0;

            public int OffsetY { get; set; } = 0;

            public float Scale { get; set; } = 1.0f;

            public bool IsUmbralIceStackEnabled { get; set; } = false;

            public Vector4 UmbralIceStackColor { get; set; } = new(0.0f, 1.0f, 1.0f, 1.0f);

            public Vector4 UmbralIceStackBackgroundColor { get; set; } = Vector4.One;

            public bool IsLucidDreamingStackEnabled { get; set; } = false;

            public Vector4 LucidDreamingStackColor { get; set; } = new(0.86f, 0.435f, 1.0f, 1.0f);

            public Vector4 LucidDreamingStackBackgroundColor { get; set; } = Vector4.One;

            public MPRegenStackUI UI { get; set; } = MPRegenStackUI.Default;
        }

        public class NumberTab
        {
            public int OffsetX { get; set; } = 0;

            public int OffsetY { get; set; } = 0;

            public float Scale { get; set; } = 1.0f;

            public Vector4 NumberColor { get; set; } = new(1.0f, 0.98f, 0.94f, 1.0f);

            public NumberType Type { get; set; } = NumberType.RemainingTime;

            public bool Reverse { get; set; } = false;

            public bool IsAlwaysEnabled { get; set; } = false;
        }

        public class CountdownTab
        {
            public int StartingSeconds { get; set; } = 12;

            public float TimeOffset { get; set; } = 0.0f;
        }

        [NonSerialized]
        private DalamudPluginInterface pluginInterface;

        public Configuration() => this.Reset();

        public void Initialize(DalamudPluginInterface pluginInterface) => this.pluginInterface = pluginInterface;

        public void Save() => this.pluginInterface.SavePluginConfig(this);

        public void Reset()
        {
            this.General = new();
            this.ProgressBar = new();
            this.FastFireIIIMarker = new();
            this.TimeSplitMarker = new();
            this.FireIIICastIndicator = new();
            this.MPRegenStack = new();
            this.Number = new();
            this.Countdown = new();
        }
    }
}