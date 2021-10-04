using Dalamud.Game.ClientState.Objects.SubKinds;
using ImGuiNET;
using System;

namespace MPTickBar
{
    public class UpdateEventState
    {
        private UpdateEventData<double> Time { get; set; } = new UpdateEventData<double>();

        private UpdateEventData<uint> MP { get; set; } = new UpdateEventData<uint>();

        private UpdateEventData<ushort> Territory { get; set; } = new UpdateEventData<ushort>();

        private UpdateEventData<bool> IsInCombat { get; set; } = new UpdateEventData<bool>();

        private UpdateEventData<bool> IsDead { get; set; } = new UpdateEventData<bool>();

        private UpdateEventData<bool> IsManafontOnCooldown { get; set; } = new UpdateEventData<bool>();

        private double Progress { get; set; }

        private bool IsProgressEnabled { get; set; }

        private bool WasManafontUsed { get; set; }

        private class UpdateEventData<T> where T : struct
        {
            public T Current { get; set; }

            public T Last { get; private set; }

            public void SaveData()
            {
                this.Last = this.Current;
            }
        }

        public void Login(object sender, EventArgs e)
        {
            this.ResetDisableProgress();
        }

        private void ResetDisableProgress()
        {
            this.IsProgressEnabled = false;
            this.Progress = 0;
        }

        public void Update(MPTickBarPluginUI mpTickBarPluginUI, PlayerCharacter currentPlayer, ushort territoryType, bool isInCombat)
        {
            this.Time.Current = ImGui.GetTime();
            this.MP.Current = currentPlayer.CurrentMp;
            this.Territory.Current = territoryType;
            this.IsInCombat.Current = isInCombat;
            this.IsDead.Current = (currentPlayer.CurrentHp == 0);
            this.IsManafontOnCooldown.Current = PlayerHelpers.IsManafontOnCooldown();

            var incrementedTime = (this.Time.Current - this.Time.Last);

            var onMPRegenLucidDreaming = false;
            if (PlayerHelpers.IsLucidDreamingActivated(currentPlayer))
            {
                var lucidDreamingPotency = 50;
                var mpReturned = lucidDreamingPotency * 10000 / 1000;
                var mpRecovered = this.MP.Current - this.MP.Last;
                var iSrecoveringLastMPTick = (this.MP.Current == currentPlayer.MaxMp) && (this.MP.Last > (currentPlayer.MaxMp - mpReturned));

                onMPRegenLucidDreaming = (mpRecovered > 0) && (mpRecovered == mpReturned || iSrecoveringLastMPTick);
            }

            if (!this.WasManafontUsed)
                this.WasManafontUsed = !this.IsManafontOnCooldown.Last && this.IsManafontOnCooldown.Current;

            var mpReset = (this.MP.Last == 0) && (this.MP.Current == currentPlayer.MaxMp);
            var onMPRegen = (this.MP.Last < this.MP.Current) && !mpReset && !onMPRegenLucidDreaming;
            if (onMPRegen)
            {
                if (!this.WasManafontUsed)
                {
                    this.ResetDisableProgress();
                    this.IsProgressEnabled = true;
                }
                else
                {
                    if (this.IsManafontOnCooldown.Current)
                        this.WasManafontUsed = false;
                }
            }

            var onZoneChange = this.Territory.Last != this.Territory.Current;
            if (onZoneChange)
                this.ResetDisableProgress();

            var onLeaveCombat = this.IsInCombat.Last && !this.IsInCombat.Current;
            if (onLeaveCombat)
                this.ResetDisableProgress();

            var onDeath = !this.IsDead.Last && this.IsDead.Current;
            if (onDeath)
                this.ResetDisableProgress();

            if (this.IsProgressEnabled)
            {
                this.Progress += incrementedTime;
                if (this.Progress >= 3)
                    this.Progress -= 3;
            }
            else
                this.WasManafontUsed = false;

            mpTickBarPluginUI.Update(this.Progress);

            this.Time.SaveData();
            this.MP.SaveData();
            this.Territory.SaveData();
            this.IsInCombat.SaveData();
            this.IsDead.SaveData();
            this.IsManafontOnCooldown.SaveData();
        }
    }
}