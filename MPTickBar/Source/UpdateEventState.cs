using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Actors.Types;
using ImGuiNET;
using System;

namespace MPTickBar
{
    public class UpdateEventState
    {
        private UpdateEventData<double> Time { get; set; } = new UpdateEventData<double>();

        private UpdateEventData<int> MP { get; set; } = new UpdateEventData<int>();

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

        public void OnLogin(object sender, EventArgs e)
        {
            this.IsFirstTimeReset = true;
        }

        public void OnUpdateEvent(PlayerCharacter currentPlayer, ClientState clientState, ActionManager actionManager, MPTickBarPluginUI mpTickBarPluginUI, Configuration configuration)
        {
            this.Time.Current = ImGui.GetTime();
            this.MP.Current = currentPlayer.CurrentMp;
            this.Territory.Current = clientState.TerritoryType;
            this.IsInCombat.Current = currentPlayer.StatusFlags.ToString().Contains("InCombat");
            this.IsDead.Current = (currentPlayer.CurrentHp == 0);
            this.IsManafontOnCooldown.Current = PlayerHelpers.IsManafontOnCooldown(actionManager);

            if (!this.WasDead)
                this.WasDead = this.IsDead.Current;

            if (!this.WasInCombat)
                this.WasInCombat = this.IsInCombat.Current;

            if (!this.WasTerritoryChanged)
                this.WasTerritoryChanged = this.Territory.Last != this.Territory.Current;

            if (!this.WasManafontUsed)
                this.WasManafontUsed = !this.IsManafontOnCooldown.Last && this.IsManafontOnCooldown.Current;

            if (!this.EnteringCombatWithoutProgress)
                this.EnteringCombatWithoutProgress = !this.IsInCombat.Last && this.IsInCombat.Current && (mpTickBarPluginUI.ProgressTime == 0);

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
                        mpTickBarPluginUI.ProgressTime = 0;
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
                    mpTickBarPluginUI.ProgressTime = 0;
                    incrementedTime = 0;
                }
            }

            this.Time.SaveData();
            this.MP.SaveData();
            this.Territory.SaveData();
            this.IsInCombat.SaveData();
            this.IsDead.SaveData();
            this.IsManafontOnCooldown.SaveData();

            mpTickBarPluginUI.FireIIICastTime = PlayerHelpers.CalculatedFireIIICastTime(configuration.FireIIICastTime, PlayerHelpers.IsCircleOfPowerActivated(currentPlayer));
            mpTickBarPluginUI.ProgressTime += incrementedTime / 3.0;
        }

        private class UpdateEventData<T> where T : struct
        {
            public T Current { get; set; }

            public T Last { get; private set; }

            public void SaveData()
            {
                this.Last = this.Current;
            }
        }
    }
}