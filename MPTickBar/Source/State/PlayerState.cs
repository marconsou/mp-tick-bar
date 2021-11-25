using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
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

        private Data<uint> MP { get; set; } = new();

        private Data<ushort> Territory { get; set; } = new();

        private Data<bool> IsDead { get; set; } = new();

        public byte UmbralIceRegenStack { get; private set; }

        public bool LucidDreamingRegenStack { get; private set; }

        public bool IsPlayingAsBlackMage => (this.ClientState != null) && (this.Player != null) && this.ClientState.IsLoggedIn && (this.Player.ClassJob?.Id == Global.BlackMageId);

        public bool IsInCombat => this.IsPlayingAsBlackMage && (this.Condition != null) && this.Condition[ConditionFlag.InCombat];

        public bool IsUmbralIceActivated => (this.UmbralIceStacks > 0);

        public bool IsUmbralIceIIIActivated => (this.UmbralIceStacks == 3);

        private bool IsLucidDreamingActivated => this.IsEffectActivated(Global.LucidDreamingId);

        private bool IsCircleOfPowerActivated => this.IsEffectActivated(Global.CircleOfPowerId);

        private byte UmbralIceStacks => (this.IsPlayingAsBlackMage && (this.JobGauges != null)) ? this.JobGauges.Get<BLMGauge>().UmbralIceStacks : (byte)0;

        private bool IsEffectActivated(uint statusId) => this.IsPlayingAsBlackMage && this.Player.StatusList.Any(x => x.StatusId == statusId);

        public bool CheckPlayerState(uint targetActorId) => !this.IsDead.Current && this.Player.IsValid() && (this.Player.ObjectId == targetActorId);

        private class Data<T> where T : struct
        {
            public T Current { get; private set; }

            public T Last { get; private set; }

            public void Update(T data) => this.Current = data;

            public void SaveData() => this.Last = this.Current;
        }

        public void ServicesUpdate(ClientState clientState, JobGauges jobGauges, Condition condition)
        {
            this.ClientState = clientState;
            this.JobGauges = jobGauges;
            this.Condition = condition;
        }

        public void StateUpdate()
        {
            this.MP.Update(this.IsPlayingAsBlackMage ? this.Player.CurrentMp : uint.MinValue);
            this.Territory.Update(this.IsPlayingAsBlackMage ? this.ClientState.TerritoryType : ushort.MinValue);
            this.IsDead.Update(this.IsPlayingAsBlackMage && (this.Player.CurrentHp == 0));
        }

        public void SaveData()
        {
            this.MP.SaveData();
            this.Territory.SaveData();
            this.IsDead.SaveData();
        }

        public void MPRegenStackUpdate()
        {
            var onlucidDreamingMPRegen = false;
            if (this.IsLucidDreamingActivated)
            {
                var lucidDreamingMPRegen = 500;
                var mpRecovered = (int)(this.MP.Current - this.MP.Last);
                var recoveringMPToFull = (this.MP.Current == this.Player.MaxMp) && (this.MP.Last > (this.Player.MaxMp - lucidDreamingMPRegen)) && (mpRecovered > 0);
                onlucidDreamingMPRegen = (mpRecovered == lucidDreamingMPRegen || recoveringMPToFull);
            }
            else
                this.LucidDreamingRegenStack = false;

            if (this.MP.Last > this.MP.Current)
                this.LucidDreamingRegenStack = false;
            else if (onlucidDreamingMPRegen)
                this.LucidDreamingRegenStack = true;

            this.UmbralIceRegenStack = (byte)((this.MP.Current + ((this.MP.Current / 6200) * 200)) / 3200);
        }

        public bool IsValidState()
        {
            var onZoneChange = (this.Territory.Last != this.Territory.Current);
            var OnDeath = (!this.IsDead.Last && this.IsDead.Current);
            return !onZoneChange && !OnDeath;
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
                var spellSpeed = UIState.pInstance->PlayerState.Attributes[Global.SpellSpeedIndex];
                return (float)Math.Floor(Math.Floor(Math.Ceiling(Math.Floor(100.0 - (this.IsCircleOfPowerActivated ? 15 : 0)) * 1) * Math.Floor((2000 - Math.Floor(130.0 * (spellSpeed - sub) / div + 1000)) * gcd35 / 1000) / 1000) * astralUmbral / 100) / 100;
            }
        }
    }
}