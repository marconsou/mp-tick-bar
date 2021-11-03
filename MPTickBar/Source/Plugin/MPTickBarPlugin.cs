using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Command;
using Dalamud.Game.Network;
using Dalamud.IoC;
using Dalamud.Plugin;
using System;

namespace MPTickBar
{
    public sealed class MPTickBarPlugin : IDalamudPlugin
    {
        public string Name => "MP Tick Bar";

        private static string CommandName => "/mptb";

        [PluginService]
        private DalamudPluginInterface PluginInterface { get; init; }

        [PluginService]
        private CommandManager CommandManager { get; init; }

        [PluginService]
        private Framework Framework { get; init; }

        [PluginService]
        private ClientState ClientState { get; init; }

        [PluginService]
        private JobGauges JobGauges { get; init; }

        [PluginService]
        private Condition Condition { get; init; }

        [PluginService]
        private GameNetwork GameNetwork { get; init; }

        private MPTickBarPluginUI MPTickBarPluginUI { get; }

        private Configuration Configuration { get; }

        private UpdateEventState UpdateEventState { get; } = new();

        public MPTickBarPlugin()
        {
            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);
            this.MPTickBarPluginUI = new MPTickBarPluginUI(this.Configuration, this.PluginInterface.UiBuilder);

            this.CommandManager.AddHandler(MPTickBarPlugin.CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Opens MP Tick Bar configuration menu.",
                ShowInHelp = true
            });

            this.PluginInterface.UiBuilder.DisableAutomaticUiHide = false;
            this.PluginInterface.UiBuilder.DisableCutsceneUiHide = false;
            this.PluginInterface.UiBuilder.DisableGposeUiHide = false;
            this.PluginInterface.UiBuilder.DisableUserUiHide = false;

            this.ClientState.Login += this.UpdateEventState.Login;
            this.PluginInterface.UiBuilder.Draw += this.Draw;
            this.PluginInterface.UiBuilder.OpenConfigUi += this.OpenConfigUi;
            this.Framework.Update += this.Update;
            this.GameNetwork.NetworkMessage += this.NetworkMessage;
        }

        public void Dispose()
        {
            this.MPTickBarPluginUI.Dispose();
            this.ClientState.Login -= this.UpdateEventState.Login;
            this.PluginInterface.UiBuilder.Draw -= this.Draw;
            this.PluginInterface.UiBuilder.OpenConfigUi -= this.OpenConfigUi;
            this.Framework.Update -= this.Update;
            this.GameNetwork.NetworkMessage -= this.NetworkMessage;
            this.CommandManager.RemoveHandler(MPTickBarPlugin.CommandName);
            this.PluginInterface.Dispose();
        }

        private PlayerCharacter GetCurrentPlayer() => this.ClientState.LocalPlayer;

        private bool IsPlayingAsBLM()
        {
            var currentPlayer = this.GetCurrentPlayer();
            return (currentPlayer != null) && this.ClientState.IsLoggedIn && PlayerHelpers.IsBlackMage(currentPlayer);
        }

        private bool IsInCombat() => this.Condition[ConditionFlag.InCombat];

        private void OnCommand(string command, string args)
        {
            this.OpenConfigUi();
        }

        private void Draw()
        {
            this.MPTickBarPluginUI.Draw();
        }

        private void OpenConfigUi()
        {
            this.MPTickBarPluginUI.IsConfigurationWindowVisible = !this.MPTickBarPluginUI.IsConfigurationWindowVisible;
        }

        private void Update(Framework framework)
        {
            var currentPlayer = this.GetCurrentPlayer();
            var isPlayingAsBLM = this.IsPlayingAsBLM();
            var isInCombat = this.IsInCombat();

            this.MPTickBarPluginUI.IsMPTickBarVisible = isPlayingAsBLM &&
                (!this.Configuration.General.IsLocked ||
                (this.Configuration.General.Visibility == MPTickBarVisibility.Visible) ||
                (this.Configuration.General.Visibility == MPTickBarVisibility.InCombat && isInCombat));

            this.MPTickBarPluginUI.Level = isPlayingAsBLM ? currentPlayer.Level : 0;
            this.MPTickBarPluginUI.IsPlayingAsBLM = isPlayingAsBLM;
            this.MPTickBarPluginUI.IsCircleOfPowerActivated = isPlayingAsBLM && PlayerHelpers.IsCircleOfPowerActivated(currentPlayer);
            this.MPTickBarPluginUI.IsUmbralIceIIIActivated = isPlayingAsBLM && PlayerHelpers.IsUmbralIceIIIActivated(this.JobGauges);

            var progress = this.UpdateEventState.Update(currentPlayer, this.ClientState.TerritoryType, isInCombat);
            this.MPTickBarPluginUI.Update(progress);
        }

        private void NetworkMessage(IntPtr dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction)
        {
            if (this.Configuration.General.IsAutostartEnabled && (opCode == 423) && (direction == NetworkMessageDirection.ZoneDown) && this.IsPlayingAsBLM())
                this.UpdateEventState.NetworkMessage(dataPtr, this.GetCurrentPlayer(), targetActorId);
        }
    }
}