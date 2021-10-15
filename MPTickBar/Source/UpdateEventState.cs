using Dalamud.Game.ClientState.Objects.SubKinds;
using ImGuiNET;
using System;
using System.Runtime.InteropServices;

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

        private UpdateEventData<double> Progress { get; set; } = new UpdateEventData<double>();

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

        public void NetworkMessage(IntPtr dataPtr, PlayerCharacter currentPlayer)
        {
            if ((currentPlayer.CurrentHp > 0) && (currentPlayer.CurrentMp == currentPlayer.MaxMp) && (this.Progress.Current == 0) && (this.Progress.Last == 0))
            {
                var data = Marshal.ReadInt32(dataPtr, 0);
                if (data == currentPlayer.CurrentHp)
                {
                    Dalamud.Logging.PluginLog.Information($"{data}");
                    this.RestartProgress();
                }
            }
        }

        private void ResetDisableProgress()
        {
            this.Progress.Current = 0;
            this.IsProgressEnabled = false;
        }

        private void RestartProgress()
        {
            if (this.Progress.Current > 0.5)
                this.Progress.Current = 0;

            this.IsProgressEnabled = true;
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
                this.RestartProgress();

            return onMPRegen;
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
                var mpTickSecondsTotal = 3.0;
                this.Progress.Current += interval;
                if (this.Progress.Current >= mpTickSecondsTotal)
                    this.Progress.Current -= mpTickSecondsTotal;
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

            mpTickBarPluginUI.Update(this.Progress.Current);

            this.Time.SaveData();
            this.MP.SaveData();
            this.Territory.SaveData();
            this.IsInCombat.SaveData();
            this.IsDead.SaveData();
            this.IsManafontOnCooldown.SaveData();
            this.Progress.SaveData();
        }
    }
}