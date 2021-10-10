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

        private double MPRegenSkipTime { get; set; }

        private bool IsProgressEnabled { get; set; }

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
            this.Progress = 0;
            this.IsProgressEnabled = false;
        }

        private bool OnMPRegenLucidDreaming(PlayerCharacter currentPlayer)
        {
            if (PlayerHelpers.IsLucidDreamingActivated(currentPlayer))
            {
                var lucidDreamingPotency = 50;
                var mpReturned = lucidDreamingPotency * 10000 / 1000;
                var mpRecovered = this.MP.Current - this.MP.Last;
                var iSrecoveringMPToFull = (this.MP.Current == currentPlayer.MaxMp) && (this.MP.Last > (currentPlayer.MaxMp - mpReturned));

                return (mpRecovered > 0) && (mpRecovered == mpReturned || iSrecoveringMPToFull);
            }
            return false;
        }

        private void OnManafontUsage()
        {
            var mpRegenSkipSecondsTotal = 3.0;
            if (!this.IsManafontOnCooldown.Last && this.IsManafontOnCooldown.Current)
                this.MPRegenSkipTime = mpRegenSkipSecondsTotal;
        }

        private bool OnMPRegen(PlayerCharacter currentPlayer, bool onMPRegenLucidDreaming, double interval)
        {
            this.MPRegenSkipTime -= interval;
            if (this.MPRegenSkipTime < 0)
                this.MPRegenSkipTime = 0;

            var mpReset = (this.MP.Last == 0) && (this.MP.Current == currentPlayer.MaxMp);
            var onMPRegen = (this.MP.Last < this.MP.Current) && !mpReset && !onMPRegenLucidDreaming && (this.MPRegenSkipTime == 0);
            if (onMPRegen)
            {
                if (this.Progress > 0.5)
                    this.Progress = 0;

                this.IsProgressEnabled = true;
                return true;
            }
            return false;
        }

        private void OnZoneChange()
        {
            if (this.Territory.Last != this.Territory.Current)
                this.ResetDisableProgress();
        }

        private void OnLeaveCombat()
        {
            if (this.IsInCombat.Last && !this.IsInCombat.Current)
                this.ResetDisableProgress();
        }

        private void OnDeath()
        {
            if (!this.IsDead.Last && this.IsDead.Current)
                this.ResetDisableProgress();
        }

        private void ProgressUpdate(double interval)
        {
            if (this.IsProgressEnabled)
            {
                this.Progress += interval;
                var mpTickSecondsTotal = 3.0;
                if (this.Progress >= mpTickSecondsTotal)
                    this.Progress -= mpTickSecondsTotal;
            }
        }

        public void Update(MPTickBarPluginUI mpTickBarPluginUI, PlayerCharacter currentPlayer, ushort territoryType, bool isInCombat)
        {
            this.Time.Current = ImGui.GetTime();
            this.MP.Current = currentPlayer.CurrentMp;
            this.Territory.Current = territoryType;
            this.IsInCombat.Current = isInCombat;
            this.IsDead.Current = (currentPlayer.CurrentHp == 0);
            this.IsManafontOnCooldown.Current = PlayerHelpers.IsManafontOnCooldown();

            var interval = (this.Time.Current - this.Time.Last);

            var onMPRegenLucidDreaming = this.OnMPRegenLucidDreaming(currentPlayer);
            this.OnManafontUsage();
            var onMPRegen = OnMPRegen(currentPlayer, onMPRegenLucidDreaming, interval);
            this.OnZoneChange();
            this.OnLeaveCombat();
            this.OnDeath();

            if (!onMPRegen)
                this.ProgressUpdate(interval);

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