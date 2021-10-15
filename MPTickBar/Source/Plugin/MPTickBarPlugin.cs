using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Command;
using Dalamud.Game.Network;
using Dalamud.IoC;
using Dalamud.Plugin;
using MPTickBar.Properties;
using System;

namespace MPTickBar
{
    public sealed class MPTickBarPlugin : IDalamudPlugin
    {
        public string Name => "MP Tick Bar";

        private static string CommandName => "/mptb";

        [PluginService]
        private static DalamudPluginInterface PluginInterface { get; set; }

        [PluginService]
        private static CommandManager CommandManager { get; set; }

        [PluginService]
        private static Framework Framework { get; set; }

        [PluginService]
        private static ClientState ClientState { get; set; }

        [PluginService]
        private static JobGauges JobGauges { get; set; }

        [PluginService]
        private static Condition Condition { get; set; }

        [PluginService]
        private static GameNetwork GameNetwork { get; set; }

        private MPTickBarPluginUI MPTickBarPluginUI { get; set; }

        private Configuration Configuration { get; set; }

        private UpdateEventState UpdateEventState { get; set; }

        public MPTickBarPlugin()
        {
            this.Configuration = MPTickBarPlugin.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(MPTickBarPlugin.PluginInterface);
            this.UpdateEventState = new UpdateEventState();

            var gaugeDefault = MPTickBarPlugin.PluginInterface.UiBuilder.LoadImage(Resources.GaugeDefault);
            var gaugeMaterialUIBlack = MPTickBarPlugin.PluginInterface.UiBuilder.LoadImage(Resources.GaugeMaterialUIBlack);
            var GaugeMaterialUIDiscord = MPTickBarPlugin.PluginInterface.UiBuilder.LoadImage(Resources.GaugeMaterialUIDiscord);
            var jobStackDefault = MPTickBarPlugin.PluginInterface.UiBuilder.LoadImage(Resources.JobStackDefault);
            var jobStackMaterialUI = MPTickBarPlugin.PluginInterface.UiBuilder.LoadImage(Resources.JobStackMaterialUI);
            var fireIIICastIndicator = MPTickBarPlugin.PluginInterface.UiBuilder.LoadImage(Resources.FireIIICastIndicator);
            var numberPercentage = MPTickBarPlugin.PluginInterface.UiBuilder.LoadImage(Resources.NumberPercentage);
            this.MPTickBarPluginUI = new MPTickBarPluginUI(this.Configuration, gaugeDefault, gaugeMaterialUIBlack, GaugeMaterialUIDiscord, jobStackDefault, jobStackMaterialUI, fireIIICastIndicator, numberPercentage);

            MPTickBarPlugin.CommandManager.AddHandler(MPTickBarPlugin.CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Opens MP Tick Bar configuration menu.",
                ShowInHelp = true
            });

            MPTickBarPlugin.PluginInterface.UiBuilder.DisableAutomaticUiHide = false;
            MPTickBarPlugin.PluginInterface.UiBuilder.DisableCutsceneUiHide = false;
            MPTickBarPlugin.PluginInterface.UiBuilder.DisableGposeUiHide = false;
            MPTickBarPlugin.PluginInterface.UiBuilder.DisableUserUiHide = false;

            MPTickBarPlugin.ClientState.Login += this.UpdateEventState.Login;
            MPTickBarPlugin.PluginInterface.UiBuilder.Draw += this.Draw;
            MPTickBarPlugin.PluginInterface.UiBuilder.OpenConfigUi += this.OpenConfigUi;
            MPTickBarPlugin.Framework.Update += this.Update;
            MPTickBarPlugin.GameNetwork.NetworkMessage += this.NetworkMessage;
        }

        public void Dispose()
        {
            this.MPTickBarPluginUI.Dispose();
            MPTickBarPlugin.ClientState.Login -= this.UpdateEventState.Login;
            MPTickBarPlugin.PluginInterface.UiBuilder.Draw -= this.Draw;
            MPTickBarPlugin.PluginInterface.UiBuilder.OpenConfigUi -= this.OpenConfigUi;
            MPTickBarPlugin.Framework.Update -= this.Update;
            MPTickBarPlugin.GameNetwork.NetworkMessage -= this.NetworkMessage;
            MPTickBarPlugin.CommandManager.RemoveHandler(MPTickBarPlugin.CommandName);
            MPTickBarPlugin.PluginInterface.Dispose();
        }

        private static PlayerCharacter GetCurrentPlayer() => MPTickBarPlugin.ClientState.LocalPlayer;

        private static bool IsPlayingAsBLM()
        {
            var currentPlayer = MPTickBarPlugin.GetCurrentPlayer();
            return (currentPlayer != null) && MPTickBarPlugin.ClientState.IsLoggedIn && PlayerHelpers.IsBlackMage(currentPlayer);
        }

        private static bool IsInCombat() => MPTickBarPlugin.Condition[ConditionFlag.InCombat];

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
            var currentPlayer = MPTickBarPlugin.GetCurrentPlayer();
            var isPlayingAsBLM = MPTickBarPlugin.IsPlayingAsBLM();
            var isInCombat = MPTickBarPlugin.IsInCombat();

            this.MPTickBarPluginUI.IsMPTickBarVisible = isPlayingAsBLM &&
                (!this.Configuration.IsMPTickBarLocked ||
                (this.Configuration.MPTickBarVisibility == MPTickBarVisibility.Visible) ||
                (this.Configuration.MPTickBarVisibility == MPTickBarVisibility.InCombat && isInCombat));

            this.MPTickBarPluginUI.Level = isPlayingAsBLM ? currentPlayer.Level : 0;
            this.MPTickBarPluginUI.IsPlayingAsBLM = isPlayingAsBLM;
            this.MPTickBarPluginUI.IsCircleOfPowerActivated = isPlayingAsBLM && PlayerHelpers.IsCircleOfPowerActivated(currentPlayer);
            this.MPTickBarPluginUI.IsUmbralIceIIIActivated = isPlayingAsBLM && PlayerHelpers.IsUmbralIceIIIActivated(MPTickBarPlugin.JobGauges);

            if (isPlayingAsBLM)
                this.UpdateEventState.Update(this.MPTickBarPluginUI, currentPlayer, MPTickBarPlugin.ClientState.TerritoryType, isInCombat);
        }

        private void NetworkMessage(IntPtr dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction)
        {
            if (this.Configuration.IsAutostartEnabled && (opCode == 423) && (sourceActorId == 0) && (direction == NetworkMessageDirection.ZoneDown))
            {
                var currentPlayer = MPTickBarPlugin.GetCurrentPlayer();
                var isPlayingAsBLM = MPTickBarPlugin.IsPlayingAsBLM();
                var isInCombat = MPTickBarPlugin.IsInCombat();

                if (isPlayingAsBLM && (!isInCombat) && (currentPlayer != null) && (currentPlayer.ObjectId == targetActorId))
                    this.UpdateEventState.NetworkMessage(dataPtr, currentPlayer);
            }
        }
    }
}