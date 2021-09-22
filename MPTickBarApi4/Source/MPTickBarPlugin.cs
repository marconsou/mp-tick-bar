using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using ImGuiNET;
using MPTickBar.Properties;

namespace MPTickBar
{
    public class MPTickBarPlugin : IDalamudPlugin
    {
        public string Name => "MP Tick Bar";

        private string CommandName => "/mptb";

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

        private MPTickBarPluginUI MPTickBarPluginUI { get; set; }

        private Configuration Configuration { get; set; }

        private double RealTime { get; set; }

        private double LastCurrentTime { get; set; }

        private uint LastCurrentMp { get; set; }

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
            var numberPercentage = MPTickBarPlugin.PluginInterface.UiBuilder.LoadImage(Resources.NumberPercentage);
            this.MPTickBarPluginUI = new MPTickBarPluginUI(this.Configuration, gaugeDefault, gaugeMaterialUIBlack, GaugeMaterialUIDiscord, jobStackDefault, jobStackMaterialUI, numberPercentage);

            MPTickBarPlugin.CommandManager.AddHandler(this.CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open MP Tick Bar configuration menu.",
                ShowInHelp = true
            });

            MPTickBarPlugin.PluginInterface.UiBuilder.DisableAutomaticUiHide = false;
            MPTickBarPlugin.PluginInterface.UiBuilder.DisableCutsceneUiHide = false;
            MPTickBarPlugin.PluginInterface.UiBuilder.DisableGposeUiHide = false;
            MPTickBarPlugin.PluginInterface.UiBuilder.DisableUserUiHide = false;

            MPTickBarPlugin.ClientState.Login += this.Login;
            MPTickBarPlugin.PluginInterface.UiBuilder.Draw += this.Draw;
            MPTickBarPlugin.PluginInterface.UiBuilder.OpenConfigUi += this.OpenConfigUi;
            MPTickBarPlugin.Framework.Update += this.Update;
        }

        public void Dispose()
        {
            this.MPTickBarPluginUI.Dispose();
            MPTickBarPlugin.ClientState.Login -= this.Login;
            MPTickBarPlugin.PluginInterface.UiBuilder.Draw -= this.Draw;
            MPTickBarPlugin.PluginInterface.UiBuilder.OpenConfigUi -= this.OpenConfigUi;
            MPTickBarPlugin.Framework.Update -= this.Update;
            MPTickBarPlugin.CommandManager.RemoveHandler(this.CommandName);
            MPTickBarPlugin.Framework.Dispose();
            MPTickBarPlugin.ClientState.Dispose();
            MPTickBarPlugin.PluginInterface.Dispose();
        }

        private void OnCommand(string command, string args)
        {
            this.OpenConfigUi();
        }

        private void Login(object sender, System.EventArgs e)
        {
            if (this.MPTickBarPluginUI != null)
                this.MPTickBarPluginUI.IsMpTickBarProgressResumed = false;
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
            var isDead = (currentPlayer.CurrentHp == 0);

            if (!this.MPTickBarPluginUI.IsMpTickBarProgressResumed)
            {
                var wasMPreset = (this.LastCurrentMp == 0) && (currentMp == currentPlayer.MaxMp);
                var wasMPRegenerated = (this.LastCurrentMp < currentMp);
                var combatConditions = !isInCombat || (isInCombat && PlayerHelpers.GetUmbralIceStacks(MPTickBarPlugin.JobGauges) > 0);
                //Manafont/Ethers conditions not covered. Do NOT start syncing with them.

                this.MPTickBarPluginUI.IsMpTickBarProgressResumed = wasMPRegenerated && combatConditions && !isDead && !wasMPreset && !PlayerHelpers.IsLucidDreamingActivated(currentPlayer);
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

                if (isDead || changingZones || leavingBattle)
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