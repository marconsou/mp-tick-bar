using Dalamud.Game.Command;
using Dalamud.Game.Internal;
using Dalamud.Plugin;
using ImGuiScene;
using MPTickBar.Properties;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace MPTickBar
{
    public class MPTickBarPlugin : IDalamudPlugin
    {
        public string Name => "MP Tick Bar";

        private string CommandName => "/mptb";

        private DalamudPluginInterface PluginInterface { get; set; }

        private MPTickBarPluginUI MPTickBarPluginUI { get; set; }

        private Configuration Configuration { get; set; }

        private ActionManager ActionManager { get; set; }

        private UpdateEventState UpdateEventState { get; set; }

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;
            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);
            this.ActionManager = new ActionManager(this.PluginInterface.TargetModuleScanner);
            this.UpdateEventState = new UpdateEventState();

            var gaugeDefault = this.LoadTexture(Resources.GaugeDefault);
            var gaugeMaterialUIBlack = this.LoadTexture(Resources.GaugeMaterialUIBlack);
            var GaugeMaterialUIDiscord = this.LoadTexture(Resources.GaugeMaterialUIDiscord);
            var jobStackDefault = this.LoadTexture(Resources.JobStackDefault);
            var jobStackMaterialUI = this.LoadTexture(Resources.JobStackMaterialUI);
            var numberPercentage = this.LoadTexture(Resources.NumberPercentage);
            this.MPTickBarPluginUI = new MPTickBarPluginUI(this.Configuration, gaugeDefault, gaugeMaterialUIBlack, GaugeMaterialUIDiscord, jobStackDefault, jobStackMaterialUI, numberPercentage);

            this.PluginInterface.CommandManager.AddHandler(this.CommandName, new CommandInfo(this.OnCommand)
            {
                HelpMessage = "Open MP Tick Bar configuration menu.",
                ShowInHelp = true
            });

            this.PluginInterface.UiBuilder.DisableAutomaticUiHide = false;
            this.PluginInterface.UiBuilder.DisableCutsceneUiHide = false;
            this.PluginInterface.UiBuilder.DisableGposeUiHide = false;
            this.PluginInterface.UiBuilder.DisableUserUiHide = false;

            this.PluginInterface.ClientState.OnLogin += this.UpdateEventState.OnLogin;
            this.PluginInterface.UiBuilder.OnBuildUi += this.OnBuildUi;
            this.PluginInterface.UiBuilder.OnOpenConfigUi += (sender, args) => this.OnOpenConfigUi();
            this.PluginInterface.Framework.OnUpdateEvent += this.OnUpdateEvent;
        }

        public void Dispose()
        {
            this.MPTickBarPluginUI.Dispose();
            this.PluginInterface.ClientState.OnLogin -= this.UpdateEventState.OnLogin;
            this.PluginInterface.UiBuilder.OnBuildUi -= this.OnBuildUi;
            this.PluginInterface.UiBuilder.OnOpenConfigUi -= (sender, args) => this.OnOpenConfigUi();
            this.PluginInterface.Framework.OnUpdateEvent -= this.OnUpdateEvent;
            this.PluginInterface.CommandManager.RemoveHandler(this.CommandName);
            this.PluginInterface.Dispose();
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
            this.MPTickBarPluginUI.Draw();
        }

        private void OnOpenConfigUi()
        {
            this.MPTickBarPluginUI.IsConfigurationWindowVisible = !this.MPTickBarPluginUI.IsConfigurationWindowVisible;
        }

        private void OnUpdateEvent(Framework framework)
        {
            var clientState = this.PluginInterface.ClientState;
            var currentPlayer = clientState?.LocalPlayer;
            this.MPTickBarPluginUI.IsMPTickBarVisible = clientState.IsLoggedIn && PlayerHelpers.IsBlackMage(currentPlayer);

            if (!this.MPTickBarPluginUI.IsMPTickBarVisible)
                return;

            this.UpdateEventState.OnUpdateEvent(currentPlayer, clientState, this.ActionManager, this.MPTickBarPluginUI, this.Configuration);
        }
    }
}