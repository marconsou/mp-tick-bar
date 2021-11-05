using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Linq;

namespace MPTickBar
{
    public class PlayerState
    {
        private ClientState ClientState { get; set; }

        private JobGauges JobGauges { get; set; }

        private Condition Condition { get; set; }

        private PlayerCharacter Player => this.ClientState?.LocalPlayer;

        public bool IsPlayingAsBlackMage => (this.ClientState != null) && (this.Player != null) && this.ClientState.IsLoggedIn && (this.Player.ClassJob?.Id == 25);

        private bool IsEffectActivated(uint statusId) => this.IsPlayingAsBlackMage && this.Player.StatusList.Any(x => x.StatusId == statusId);

        public bool IsLucidDreamingActivated => this.IsEffectActivated(1204);

        public bool IsCircleOfPowerActivated => this.IsEffectActivated(738);

        public bool IsUmbralIceIIIActivated => this.IsPlayingAsBlackMage && (this.JobGauges != null) && (this.JobGauges.Get<BLMGauge>().UmbralIceStacks == 3);

        public bool IsInCombat => this.IsPlayingAsBlackMage && (this.Condition != null) && this.Condition[ConditionFlag.InCombat];

        public bool IsDead => this.IsPlayingAsBlackMage && (this.Player.CurrentHp == 0);

        public ushort TerritoryType => this.IsPlayingAsBlackMage ? this.ClientState.TerritoryType : (ushort)0;

        public uint HP => this.IsPlayingAsBlackMage ? this.Player.CurrentHp : 0;

        public uint MP => this.IsPlayingAsBlackMage ? this.Player.CurrentMp : 0;

        public uint MPMax => this.IsPlayingAsBlackMage ? this.Player.MaxMp : 0;

        public uint Id => this.IsPlayingAsBlackMage ? this.Player.ObjectId : uint.MaxValue;

        public void ServicesUpdate(ClientState clientState, JobGauges jobGauges, Condition condition)
        {
            this.ClientState = clientState;
            this.JobGauges = jobGauges;
            this.Condition = condition;
        }

        public bool IsManafontOnCooldown()
        {
            unsafe
            {
                return this.IsPlayingAsBlackMage && ActionManager.Instance()->IsRecastTimerActive(ActionType.Spell, 158);
            }
        }

        public float GetFastFireIIICastTime()
        {
            unsafe
            {
                var level = this.IsPlayingAsBlackMage ? this.Player.Level : 0;
                if (level == 0)
                    return 0.0f;

                var gcd35 = 3500;
                var astralUmbral = 50;
                var sub = LevelModifier.GetLevelModifierSub(level);
                var div = LevelModifier.GetLevelModifierDiv(level);
                var spellSpeed = UIState.pInstance->PlayerState.Attributes[46];
                return (float)Math.Floor(Math.Floor(Math.Ceiling(Math.Floor(100.0 - (this.IsCircleOfPowerActivated ? 15 : 0)) * 1) * Math.Floor((2000 - Math.Floor(130.0 * (spellSpeed - sub) / div + 1000)) * gcd35 / 1000) / 1000) * astralUmbral / 100) / 100;
            }
        }
    }
}