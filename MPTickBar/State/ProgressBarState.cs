using ImGuiNET;
using System;

namespace MPTickBar
{
    public class ProgressBarState
    {
        public PlayerState PlayerState { get; set; }

        private double ProgressTime { get; set; }

        private double NetworkTime { get; set; }

        public void Login(object sender, EventArgs e) => this.ResetProgress();

        public void RestartProgress() => this.NetworkTime = ImGui.GetTime();

        private void ResetProgress() => this.NetworkTime = 0.0;

        private void ProgressUpdate() => this.ProgressTime = (this.NetworkTime != 0.0) ? ((ImGui.GetTime() - this.NetworkTime) % 3.0) : 0.0;

        public double Update()
        {
            this.PlayerState.StateUpdate();
            this.PlayerState.MPRegenStackUpdate();
            if (!this.PlayerState.IsValidState())
                this.ResetProgress();
            this.ProgressUpdate();
            this.PlayerState.SaveData();
            return this.ProgressTime / 3.0;
        }
    }
}