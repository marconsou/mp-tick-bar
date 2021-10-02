using Dalamud.Game.ClientState;
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

        private bool WasDead { get; set; }

        private bool WasInCombat { get; set; }

        private bool WasTerritoryChanged { get; set; }

        private bool WasManafontUsed { get; set; }

        private bool EnteringCombatWithoutProgress { get; set; }

        private bool IsFirstTimeReset { get; set; } = true;

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
            this.IsFirstTimeReset = true;
        }

        public void Update(MPTickBarPluginUI mpTickBarPluginUI, PlayerCharacter currentPlayer, ushort territoryType, bool isInCombat)
        {
            this.Time.Current = ImGui.GetTime();
            this.MP.Current = currentPlayer.CurrentMp;
            this.Territory.Current = territoryType;
            this.IsInCombat.Current = isInCombat;
            this.IsDead.Current = (currentPlayer.CurrentHp == 0);
            this.IsManafontOnCooldown.Current = PlayerHelpers.IsManafontOnCooldown();

            if (!this.WasDead)
                this.WasDead = this.IsDead.Current;

            if (!this.WasInCombat)
                this.WasInCombat = this.IsInCombat.Current;

            if (!this.WasTerritoryChanged)
                this.WasTerritoryChanged = this.Territory.Last != this.Territory.Current;

            if (!this.WasManafontUsed)
                this.WasManafontUsed = !this.IsManafontOnCooldown.Last && this.IsManafontOnCooldown.Current;

            if (!this.EnteringCombatWithoutProgress)
                this.EnteringCombatWithoutProgress = !this.IsInCombat.Last && this.IsInCombat.Current && (mpTickBarPluginUI.GetProgressTime(false) == 0.0);

            var incrementedTime = this.Time.Current - this.Time.Last;

            if (!PlayerHelpers.IsLucidDreamingActivated(currentPlayer))
            {
                var wasMPReset = (this.MP.Last == 0) && (this.MP.Current == currentPlayer.MaxMp);
                var wasMPRegenerated = (this.MP.Last < this.MP.Current) && !wasMPReset;
                var resetHoldProgress = this.WasDead || (this.WasInCombat && !this.IsInCombat.Current) || this.WasTerritoryChanged || this.EnteringCombatWithoutProgress || this.IsFirstTimeReset;

                if (wasMPRegenerated)
                {
                    if (!this.WasManafontUsed)
                    {
                        mpTickBarPluginUI.ResetProgressTime();
                        this.WasDead = false;
                        this.WasInCombat = false;
                        this.WasTerritoryChanged = false;
                        this.EnteringCombatWithoutProgress = false;
                        this.IsFirstTimeReset = false;
                    }
                    else
                    {
                        if (this.IsManafontOnCooldown.Current)
                            this.WasManafontUsed = false;
                    }
                }
                else if (resetHoldProgress)
                {
                    mpTickBarPluginUI.ResetProgressTime();
                    incrementedTime = 0;
                }
            }

            this.Time.SaveData();
            this.MP.SaveData();
            this.Territory.SaveData();
            this.IsInCombat.SaveData();
            this.IsDead.SaveData();
            this.IsManafontOnCooldown.SaveData();

            mpTickBarPluginUI.UpdateProgressTime(incrementedTime);
        }
    }
}