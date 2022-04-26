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

        public void Log(params string[] messages)
        {
            string[] tmp = new string[logs.Length];
            Array.Copy(logs, 1, tmp, 0, this.logs.Length - 1);
            tmp[tmp.Length - 1] = String.Join(" ", messages);
            this.logs = tmp;

            this.gui.text = String.Join("\n", this.logs);
        }

        protected override void _Start() { this.Log(nameof(_Start)); }
        protected override void OnDebugLog(string message) { this.Log(message); }
        protected override void OnVideoPlayerInit() { this.Log(nameof(OnVideoPlayerInit)); }
        protected override void OnPlaybackEnd() { this.Log(nameof(OnPlaybackEnd)); }
        protected override void OnLoadReady(float duration) { this.Log(nameof(OnLoadReady), duration.ToString()); }
        protected override void OnLoadBegin() { this.Log(nameof(OnLoadBegin)); }
        protected override void OnLoadBegin(VRCUrl url) { this.Log(nameof(OnLoadBegin), "(with URL)"); }
        protected override void OnLoadError(VideoError videoError) { this.Log(nameof(OnLoadError), videoError.ToString()); }
        protected override void OnProgress(float timestamp, float duration) { this.Log(nameof(OnProgress), timestamp.ToString(), duration.ToString()); }
        protected override void OnUnload() { this.Log(nameof(OnUnload)); }
        protected override void OnPlaybackStart(float timestamp) { this.Log(nameof(OnPlaybackStart), timestamp.ToString()); }
        protected override void OnPlaybackStop(float timestamp) { this.Log(nameof(OnPlaybackStop), timestamp.ToString()); }
        protected override void OnAutoRetry(int attempt) { this.Log(nameof(OnAutoRetry), attempt.ToString()); }
        protected override void OnAutoRetryLoadTimeout() { this.Log(nameof(OnAutoRetryLoadTimeout)); }
        protected override void OnAutoRetrySwitchPlayer() { this.Log(nameof(OnAutoRetrySwitchPlayer)); }
        protected override void OnAutoRetryAbort() { this.Log(nameof(OnAutoRetryAbort)); }
        protected override void OnBrightnessChange(float alpha) { this.Log(nameof(OnBrightnessChange), alpha.ToString()); }
        protected override void OnVolumeChange(float volume, bool muted) { this.Log(nameof(OnVolumeChange), volume.ToString(), muted.ToString()); }
        protected override void OnMutedChange(bool muted, float volume) { this.Log(nameof(OnMutedChange), muted.ToString(), volume.ToString()); }
        protected override void OnFpsChange(int fps) { this.Log(nameof(OnFpsChange), fps.ToString()); }
        protected override void OnScreenResolutionChange(Renderer screen, float width, float height) { this.Log(nameof(OnScreenResolutionChange), screen.name, width.ToString(), height.ToString()); }
        protected override void OnLoadRequested(VRCUrl url) { this.Log(nameof(OnLoadRequested), "(with URL)"); }
        protected override void OnOwnershipChanged(int previousOwnerId, VRCPlayerApi nextOwner) { this.Log(nameof(OnOwnershipChanged), previousOwnerId.ToString(), nextOwner.playerId.ToString()); }
        protected override void OnOwnershipRequested() { this.Log(nameof(OnOwnershipRequested)); }
        protected override void OnOwnershipSecurityChanged(bool locked) { this.Log(nameof(OnOwnershipSecurityChanged), locked.ToString()); }
        protected override void OnScreenTextureChange() { this.Log(nameof(OnScreenTextureChange)); }
    }
}
