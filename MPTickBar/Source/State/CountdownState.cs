using ImGuiNET;
using System.Linq;

namespace MPTickBar
{
    public class CountdownState
    {
        private int StartingSeconds { get; set; }

        private double TimeExecuteCommand { get; set; }

        private double TimeCancelCommand { get; set; }

        private double LastProgress { get; set; }

        private bool CheckForCommand => this.StartingSeconds != 0;

        private void Reset()
        {
            this.StartingSeconds = 0;
            this.TimeExecuteCommand = 0.0;
            this.TimeCancelCommand = 0.0;
        }

        public void Start(string args, int startingSeconds, float timeOffset)
        {
            this.StartingSeconds = (int.TryParse(args, out int paramValue) && Enumerable.Range(5, 26).Contains(paramValue)) ? paramValue : startingSeconds;
            this.TimeExecuteCommand = 0.0;
            this.TimeCancelCommand = ImGui.GetTime() + this.StartingSeconds + timeOffset + 3.5;
        }

        public void Update(Chat chat, double progress, float timeOffset)
        {
            if (this.CheckForCommand)
            {
                var time = ImGui.GetTime();

                if ((this.LastProgress > progress) && (this.TimeExecuteCommand == 0.0))
                    this.TimeExecuteCommand = time + timeOffset;

                if ((time > this.TimeExecuteCommand) && (this.TimeExecuteCommand != 0.0))
                {
                    chat.ExecuteCommand($"/cd {this.StartingSeconds}");
                    this.Reset();
                }

                if ((time > this.TimeCancelCommand) && (this.TimeCancelCommand != 0.0))
                    this.Reset();
            }
            this.LastProgress = progress;
        }
    }
}