using ImGuiNET;
using System.Linq;

namespace MPTickBar
{
    public class CountdownState
    {
        private int StartingSeconds { get; set; }

        private double ExecuteCommandTime { get; set; }

        private double CancelCommandTime { get; set; }

        private double LastProgress { get; set; }

        private bool CheckForCommand => this.StartingSeconds != 0;

        private void Reset()
        {
            this.StartingSeconds = 0;
            this.ExecuteCommandTime = 0.0;
            this.CancelCommandTime = 0.0;
        }

        public void Start(string args, int startingSeconds, float timeOffset)
        {
            this.StartingSeconds = (int.TryParse(args, out int paramValue) && Enumerable.Range(5, 26).Contains(paramValue)) ? paramValue : startingSeconds;
            this.ExecuteCommandTime = 0.0;
            this.CancelCommandTime = ImGui.GetTime() + this.StartingSeconds + timeOffset + 3.5;
        }

        public void Update(Chat chat, double progress, float timeOffset)
        {
            if (this.CheckForCommand)
            {
                var time = ImGui.GetTime();

                if ((this.LastProgress > progress) && (this.ExecuteCommandTime == 0.0))
                    this.ExecuteCommandTime = time + timeOffset;

                if ((time > this.ExecuteCommandTime) && (this.ExecuteCommandTime != 0.0))
                {
                    chat.ExecuteCommand($"/cd {this.StartingSeconds}");
                    this.Reset();
                }

                if ((time > this.CancelCommandTime) && (this.CancelCommandTime != 0.0))
                    this.Reset();
            }
            this.LastProgress = progress;
        }
    }
}