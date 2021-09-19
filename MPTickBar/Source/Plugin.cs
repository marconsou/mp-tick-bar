using Dalamud.Game.Command;
using Dalamud.Game.Internal;
using Dalamud.Plugin;
using ImGuiNET;
using ImGuiScene;
using MPTickBar.Properties;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace MPTickBar
{
    public class Plugin : IDalamudPlugin
    {
        public string Name => "MP Tick Bar";

        private string CommandName => "/mptb";

        private DalamudPluginInterface PluginInterface { get; set; }

        private PluginUI PluginUI { get; set; }

        private Configuration Configuration { get; set; }

        private double RealTime { get; set; }

        private double LastCurrentTime { get; set; }

        private int LastCurrentMp { get; set; } = int.MaxValue;

        private ushort LastTerritoryType { get; set; } = ushort.MaxValue;

        private bool LastIsInCombat { get; set; }

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;
            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            var gaugeDefault = this.LoadTexture(Resources.GaugeDefault);
            var gaugeMaterialUIBlack = this.LoadTexture(Resources.GaugeMaterialUIBlack);
            var GaugeMaterialUIDiscord = this.LoadTexture(Resources.GaugeMaterialUIDiscord);
            var jobStackDefault = this.LoadTexture(Resources.JobStackDefault);
            var jobStackMaterialUI = this.LoadTexture(Resources.JobStackMaterialUI);
            this.PluginUI = new PluginUI(this.Configuration, gaugeDefault, gaugeMaterialUIBlack, GaugeMaterialUIDiscord, jobStackDefault, jobStackMaterialUI);

            this.PluginInterface.CommandManager.AddHandler(this.CommandName, new CommandInfo(this.OnCommand)
            {
                HelpMessage = "Visually tracks MP regeneration tick (Black Mage only).",
                ShowInHelp = true
            });

            this.PluginInterface.UiBuilder.DisableAutomaticUiHide = false;
            this.PluginInterface.UiBuilder.DisableCutsceneUiHide = false;
            this.PluginInterface.UiBuilder.DisableGposeUiHide = false;
            this.PluginInterface.UiBuilder.DisableUserUiHide = false;
            this.PluginInterface.UiBuilder.OnBuildUi += this.OnBuildUi;
            this.PluginInterface.UiBuilder.OnOpenConfigUi += (sender, args) => this.OnOpenConfigUi();
            this.PluginInterface.Framework.OnUpdateEvent += this.OnUpdateEvent;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            this.PluginUI.Dispose();
            this.PluginInterface.UiBuilder.OnBuildUi -= this.OnBuildUi;
            this.PluginInterface.UiBuilder.OnOpenConfigUi -= (sender, args) => this.OnOpenConfigUi();
            this.PluginInterface.Framework.OnUpdateEvent -= this.OnUpdateEvent;
            this.PluginInterface.CommandManager.RemoveHandler(this.CommandName);
            this.PluginInterface.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private TextureWrap LoadTexture(Bitmap bitmap)
        {
            var bytesPerPixel = Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
            var imageData = this.FixColors(bitmap, bytesPerPixel);
            return this.PluginInterface.UiBuilder.LoadImageRaw(imageData, bitmap.Width, bitmap.Height, bytesPerPixel);
        }

        private byte[] FixColors(Bitmap bitmap, int bytesPerPixel)
        {
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            var byteCount = bitmapData.Stride * bitmap.Height;
            var pixels = new byte[byteCount];
            var ptrFirstPixel = bitmapData.Scan0;
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);
            var heightInPixels = bitmapData.Height;
            var widthInBytes = bitmapData.Width * bytesPerPixel;
            for (var y = 0; y < heightInPixels; y++)
            {
                var currentLine = y * bitmapData.Stride;
                for (var x = 0; x < widthInBytes; x += bytesPerPixel)
                {
                    var oldRed = pixels[currentLine + x];
                    var oldGreen = pixels[currentLine + x + 1];
                    var oldBlue = pixels[currentLine + x + 2];

                    pixels[currentLine + x + 2] = oldRed;
                    pixels[currentLine + x + 1] = oldGreen;
                    pixels[currentLine + x] = oldBlue;
                }
            }
            bitmap.UnlockBits(bitmapData);
            return pixels;
        }

        private void OnCommand(string command, string args)
        {
            this.OnOpenConfigUi();
        }

        private void OnBuildUi()
        {
            this.PluginUI.Draw();
        }

        private void OnOpenConfigUi()
        {
            this.PluginUI.IsConfigurationWindowVisible = !this.PluginUI.IsConfigurationWindowVisible;
        }

        private void OnUpdateEvent(Framework framework)
        {
            var clientState = this.PluginInterface.ClientState;
            var currentPlayer = clientState?.LocalPlayer;
            this.PluginUI.IsMPTickBarVisible = clientState.IsLoggedIn && PlayerHelpers.IsBlackMage(currentPlayer);

            if (!this.PluginUI.IsMPTickBarVisible)
                return;

            this.PluginUI.IsUmbralIceIIIActivated = PlayerHelpers.IsUmbralIceIIIActivated(clientState);
            this.PluginUI.FireIIICastTime = PlayerHelpers.CalculatedFireIIICastTime(this.Configuration.FireIIICastTime, this.PluginUI.IsUmbralIceIIIActivated, PlayerHelpers.IsCircleOfPowerActivated(currentPlayer));

            var currentMp = currentPlayer.CurrentMp;
            var territoryType = clientState.TerritoryType;
            var isInCombat = currentPlayer.StatusFlags.ToString().Contains("InCombat");

            if (!this.PluginUI.IsMpTickBarProgressResumed)
            {
                var skipSpecificEvents = (this.LastCurrentMp == 0) && (currentMp == 10000); //Death during battle / first loop on login
                var wasMPRegenerated = (this.LastCurrentMp < currentMp);

                this.PluginUI.IsMpTickBarProgressResumed = (!skipSpecificEvents) && (wasMPRegenerated) && (!PlayerHelpers.IsLucidDreamingActivated(currentPlayer));

                if (this.PluginUI.IsMpTickBarProgressResumed)
                {
                    this.RealTime = ImGui.GetTime();
                }
            }
            else
            {
                var currentTime = ImGui.GetTime() - this.RealTime;
                var incrementedTime = currentTime - this.LastCurrentTime;
                this.PluginUI.ProgressTime += (float)(incrementedTime / 3.0f);
                this.LastCurrentTime = currentTime;

                var changingZones = (this.LastTerritoryType != territoryType);
                var leavingBattle = (this.LastIsInCombat && !isInCombat);

                if ((changingZones) || (leavingBattle))
                {
                    this.PluginUI.IsMpTickBarProgressResumed = false;
                }
            }

            this.LastCurrentMp = currentMp;
            this.LastTerritoryType = territoryType;
            this.LastIsInCombat = isInCombat;
        }
    }
}