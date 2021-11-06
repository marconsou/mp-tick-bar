using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.JobGauge;
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

        private PlayerState PlayerState { get; } = new();

        public MPTickBarPlugin()
        {
            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new();
            this.Configuration.Initialize(this.PluginInterface);
            this.MPTickBarPluginUI = new(this.Configuration, this.PluginInterface.UiBuilder);

            this.CommandManager.AddHandler(MPTickBarPlugin.CommandName, new(this.OnCommand)
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

        private void PlayerStateUpdate()
        {
            this.PlayerState.ServicesUpdate(this.ClientState, this.JobGauges, this.Condition);
            this.UpdateEventState.PlayerState = this.PlayerState;
            this.MPTickBarPluginUI.PlayerState = this.PlayerState;
        }

        private void OnCommand(string command, string args) => this.OpenConfigUi();

        private void Draw() => this.MPTickBarPluginUI.Draw();

        private void OpenConfigUi() => this.MPTickBarPluginUI.IsConfigurationWindowVisible = !this.MPTickBarPluginUI.IsConfigurationWindowVisible;

        private void Update(Framework framework)
        {
            this.PlayerStateUpdate();

            var progress = this.UpdateEventState.Update();
            this.MPTickBarPluginUI.Update(progress);
        }

        private void NetworkMessage(IntPtr dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction)
        {
            if (this.Configuration.General.IsAutostartEnabled && (opCode == 423) && (direction == NetworkMessageDirection.ZoneDown) && (this.PlayerState != null) && this.PlayerState.IsPlayingAsBlackMage)
            {
                this.PlayerStateUpdate();
                this.UpdateEventState.NetworkMessage(dataPtr, targetActorId);
            }
        }
    }
}