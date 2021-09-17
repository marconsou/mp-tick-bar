using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using ImGuiNET;
using MPTickBarApi4.Properties;
using System;

namespace MPTickBarApi4
{
    public class Plugin : IDalamudPlugin
    {
        public string Name => "MP Tick Bar (Api 4)";

        private static string CommandName => "/mptb4";

        [PluginService]
        [RequiredVersion("1.0")]
        private DalamudPluginInterface PluginInterface { get; set; }

        [PluginService]
        [RequiredVersion("1.0")]
        private CommandManager CommandManager { get; init; }

        [PluginService]
        [RequiredVersion("1.0")]
        private Framework Framework { get; set; }

        [PluginService]
        [RequiredVersion("1.0")]
        private ClientState ClientState { get; set; }

        [PluginService]
        [RequiredVersion("1.0")]
        private JobGauges JobGauges { get; set; }

        private PluginUI PluginUI { get; set; }

        private Configuration Configuration { get; set; }

        private double RealTime { get; set; }

        private double LastCurrentTime { get; set; }

        private uint LastCurrentMp { get; set; } = int.MaxValue;

        private ushort LastTerritoryType { get; set; } = ushort.MaxValue;

        private bool LastIsInCombat { get; set; }

        public Plugin()
        {
            this.Configuration = this.PluginInterface?.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            var gaugeDefault = this.PluginInterface.UiBuilder.LoadImage(Resources.GaugeDefault);
            var gaugeMaterialUIBlack = this.PluginInterface.UiBuilder.LoadImage(Resources.GaugeMaterialUIBlack);
            var GaugeMaterialUIDiscord = this.PluginInterface.UiBuilder.LoadImage(Resources.GaugeMaterialUIDiscord);
            var jobStackDefault = this.PluginInterface.UiBuilder.LoadImage(Resources.JobStackDefault);
            var jobStackMaterialUI = this.PluginInterface.UiBuilder.LoadImage(Resources.JobStackMaterialUI);
            this.PluginUI = new PluginUI(this.Configuration, gaugeDefault, gaugeMaterialUIBlack, GaugeMaterialUIDiscord, jobStackDefault, jobStackMaterialUI);

            this.CommandManager?.AddHandler(Plugin.CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Visually tracks MP regeneration tick (Black Mage only).",
            });

            this.PluginInterface.UiBuilder.Draw += this.Draw;
            this.PluginInterface.UiBuilder.OpenConfigUi += this.OpenConfigUi;
            this.Framework.Update += this.Update;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            this.PluginUI.Dispose();
            this.PluginInterface.UiBuilder.Draw -= this.Draw;
            this.PluginInterface.UiBuilder.OpenConfigUi -= this.OpenConfigUi;
            this.Framework.Update -= this.Update;
            this.CommandManager.RemoveHandler(Plugin.CommandName);
            this.ClientState.Dispose();
            this.Framework.Dispose();
            this.PluginInterface.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /* public byte[] ImageToByteArray(System.Drawing.Image imageIn)
         {
             using (var ms = new System.IO.MemoryStream())
             {
                 imageIn.Save(ms, imageIn.RawFormat);
                 return ms.ToArray();
             }
         }*/

        /*  private byte[] FixColors(System.Drawing.Bitmap bitmap, int bytesPerPixel)
          {
              var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bitmap.PixelFormat);
              var byteCount = bitmapData.Stride * bitmap.Height;
              var pixels = new byte[byteCount];
              var ptrFirstPixel = bitmapData.Scan0;
              System.Runtime.InteropServices.Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);
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
          }*/

        //  private static byte[] FixColors(byte[] imageData)
        //  {
        //  return imageData;
        /*   var fixedImageData = new byte[imageData.Length];
           for (int i = 0; i < imageData.Length; i += 2)
           {
               var red = imageData[i];
               var green = imageData[i + 1];
               var blue = imageData[i + 2];
               var alpha = imageData[i + 3];

               fixedImageData[i] = red;
               fixedImageData[i + 1] = green;
               fixedImageData[i + 2] = blue;
               fixedImageData[i + 3] = alpha;
           }
           return fixedImageData;*/
        //}

        private void OnCommand(string command, string args)
        {
            this.OpenConfigUi();
        }

        private void Draw()
        {
            this.PluginUI.Draw();
        }

        private void OpenConfigUi()
        {
            this.PluginUI.IsConfigurationWindowVisible = !this.PluginUI.IsConfigurationWindowVisible;
        }

        private void Update(Framework framework)
        {
            var currentPlayer = this.ClientState.LocalPlayer;
            this.PluginUI.IsMPTickBarVisible = this.ClientState.IsLoggedIn && PlayerHelpers.IsBlackMage(currentPlayer);

            if (!this.PluginUI.IsMPTickBarVisible)
                return;

            this.PluginUI.IsUmbralIceIIIActivated = PlayerHelpers.IsUmbralIceIIIActivated(this.JobGauges);
            this.PluginUI.FireIIICastTime = PlayerHelpers.CalculatedFireIIICastTime(this.Configuration.FireIIICastTime, this.PluginUI.IsUmbralIceIIIActivated, PlayerHelpers.IsCircleOfPowerActivated(currentPlayer));

            var currentMp = currentPlayer.CurrentMp;
            var territoryType = this.ClientState.TerritoryType;
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