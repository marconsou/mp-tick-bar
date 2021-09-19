﻿using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using ImGuiNET;

namespace MPTickBar
{
    public class MPTickBarPlugin : IDalamudPlugin
    {
        public string Name => "MP Tick Bar";

        private static string CommandName => "/mptb";

        [PluginService]
        public static DalamudPluginInterface PluginInterface { get; private set; }

        [PluginService]
        public static CommandManager CommandManager { get; private set; }

        [PluginService]
        public static Framework Framework { get; private set; }

        [PluginService]
        public static ClientState ClientState { get; private set; }

        [PluginService]
        public static JobGauges JobGauges { get; private set; }

        private MPTickBarPluginUI MPTickBarPluginUI { get; set; }

        private Configuration Configuration { get; set; }

        private double RealTime { get; set; }

        private double LastCurrentTime { get; set; }

        private uint LastCurrentMp { get; set; } = int.MaxValue;

        private ushort LastTerritoryType { get; set; } = ushort.MaxValue;

        private bool LastIsInCombat { get; set; }

        public MPTickBarPlugin()
        {
            this.Configuration = MPTickBarPlugin.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(MPTickBarPlugin.PluginInterface);

            var gaugeDefault = MPTickBarPlugin.PluginInterface.UiBuilder.LoadImage(Resources.GaugeDefault);
            var gaugeMaterialUIBlack = MPTickBarPlugin.PluginInterface.UiBuilder.LoadImage(Resources.GaugeMaterialUIBlack);
            var GaugeMaterialUIDiscord = MPTickBarPlugin.PluginInterface.UiBuilder.LoadImage(Resources.GaugeMaterialUIDiscord);
            var jobStackDefault = MPTickBarPlugin.PluginInterface.UiBuilder.LoadImage(Resources.JobStackDefault);
            var jobStackMaterialUI = MPTickBarPlugin.PluginInterface.UiBuilder.LoadImage(Resources.JobStackMaterialUI);
            this.MPTickBarPluginUI = new MPTickBarPluginUI(this.Configuration, gaugeDefault, gaugeMaterialUIBlack, GaugeMaterialUIDiscord, jobStackDefault, jobStackMaterialUI);

            MPTickBarPlugin.CommandManager?.AddHandler(MPTickBarPlugin.CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open configuration menu.",
                ShowInHelp = true
            });

            MPTickBarPlugin.PluginInterface.UiBuilder.Draw += this.Draw;
            MPTickBarPlugin.PluginInterface.UiBuilder.OpenConfigUi += this.OpenConfigUi;
            MPTickBarPlugin.Framework.Update += this.Update;
        }

        public void Dispose()
        {
            this.MPTickBarPluginUI.Dispose();
            MPTickBarPlugin.PluginInterface.UiBuilder.Draw -= this.Draw;
            MPTickBarPlugin.PluginInterface.UiBuilder.OpenConfigUi -= this.OpenConfigUi;
            MPTickBarPlugin.Framework.Update -= this.Update;
            MPTickBarPlugin.CommandManager.RemoveHandler(MPTickBarPlugin.CommandName);
            MPTickBarPlugin.Framework.Dispose();
            MPTickBarPlugin.ClientState.Dispose();
            MPTickBarPlugin.PluginInterface.Dispose();
        }

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
            var currentPlayer = MPTickBarPlugin.ClientState.LocalPlayer;
            this.MPTickBarPluginUI.IsMPTickBarVisible = MPTickBarPlugin.ClientState.IsLoggedIn && PlayerHelpers.IsBlackMage(currentPlayer);

            if (!this.MPTickBarPluginUI.IsMPTickBarVisible)
                return;

            this.MPTickBarPluginUI.IsUmbralIceIIIActivated = PlayerHelpers.IsUmbralIceIIIActivated(MPTickBarPlugin.JobGauges);
            this.MPTickBarPluginUI.FireIIICastTime = PlayerHelpers.CalculatedFireIIICastTime(this.Configuration.FireIIICastTime, this.MPTickBarPluginUI.IsUmbralIceIIIActivated, PlayerHelpers.IsCircleOfPowerActivated(currentPlayer));

            var currentMp = currentPlayer.CurrentMp;
            var territoryType = MPTickBarPlugin.ClientState.TerritoryType;
            var isInCombat = currentPlayer.StatusFlags.ToString().Contains("InCombat");

            if (!this.MPTickBarPluginUI.IsMpTickBarProgressResumed)
            {
                var skipSpecificEvents = (this.LastCurrentMp == 0) && (currentMp == 10000); //Death during battle / first loop on login
                var wasMPRegenerated = (this.LastCurrentMp < currentMp);

                this.MPTickBarPluginUI.IsMpTickBarProgressResumed = (!skipSpecificEvents) && (wasMPRegenerated) && (!PlayerHelpers.IsLucidDreamingActivated(currentPlayer));

                if (this.MPTickBarPluginUI.IsMpTickBarProgressResumed)
                {
                    this.RealTime = ImGui.GetTime();
                }
            }
            else
            {
                var currentTime = ImGui.GetTime() - this.RealTime;
                var incrementedTime = currentTime - this.LastCurrentTime;
                this.MPTickBarPluginUI.ProgressTime += (float)(incrementedTime / 3.0f);
                this.LastCurrentTime = currentTime;

                var changingZones = (this.LastTerritoryType != territoryType);
                var leavingBattle = (this.LastIsInCombat && !isInCombat);

                if ((changingZones) || (leavingBattle))
                {
                    this.MPTickBarPluginUI.IsMpTickBarProgressResumed = false;
                }
            }

            this.LastCurrentMp = currentMp;
            this.LastTerritoryType = territoryType;
            this.LastIsInCombat = isInCombat;
        }
    }
}