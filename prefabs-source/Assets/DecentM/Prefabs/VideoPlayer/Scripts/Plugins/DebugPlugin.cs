using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace DecentM.VideoPlayer.Plugins
{
    public class DebugPlugin : VideoPlayerPlugin
    {
        public TextMeshProUGUI gui;

        string[] logs = new string[40];

        private void Log(params string[] messages)
        {
            string[] tmp = new string[logs.Length];
            Array.Copy(logs, 1, tmp, 0, this.logs.Length - 1);
            tmp[tmp.Length - 1] = String.Join(" ", messages);
            this.logs = tmp;

            this.gui.text = String.Join("\n", this.logs);
        }

        protected override void OnVideoPlayerInit() { this.Log(nameof(OnVideoPlayerInit)); }
        protected override void OnPlaybackEnd() { this.Log(nameof(OnPlaybackEnd)); }
        protected override void OnLoadReady(float duration) { this.Log(nameof(OnLoadReady), duration.ToString()); }
        protected override void OnLoadBegin() { this.Log(nameof(OnLoadBegin)); }
        protected override void OnLoadBegin(VRCUrl url) { this.Log(nameof(OnLoadBegin), url.ToString()); }
        protected override void OnLoadError(VideoError videoError) { this.Log(nameof(OnLoadError), videoError.ToString()); }
        protected override void OnProgress(float timestamp, float duration) { this.Log(nameof(OnProgress), timestamp.ToString(), duration.ToString()); }
        protected override void OnUnload() { this.Log(nameof(OnUnload)); }
        protected override void OnPlaybackStart(float timestamp) { this.Log(nameof(OnPlaybackStart), timestamp.ToString()); }
        protected override void OnPlaybackStop(float timestamp) { this.Log(nameof(OnPlaybackStop), timestamp.ToString()); }
        protected override void OnAutoRetry(int attempt) { this.Log(nameof(OnAutoRetry), attempt.ToString()); }
        protected override void OnAutoRetryLoadTimeout() { this.Log(nameof(OnAutoRetryLoadTimeout)); }
        protected override void OnAutoRetrySwitchPlayer() { this.Log(nameof(OnAutoRetrySwitchPlayer)); }
        protected override void OnBrightnessChange(float alpha) { this.Log(nameof(OnBrightnessChange), alpha.ToString()); }
        protected override void OnVolumeChange(float volume) { this.Log(nameof(OnVolumeChange), volume.ToString()); }
        protected override void OnMutedChange(bool muted) { this.Log(nameof(OnMutedChange), muted.ToString()); }
    }
}
