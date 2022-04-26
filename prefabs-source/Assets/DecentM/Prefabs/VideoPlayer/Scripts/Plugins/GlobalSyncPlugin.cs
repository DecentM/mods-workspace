using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using UNet;

namespace DecentM.VideoPlayer.Plugins
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class GlobalSyncPlugin : VideoPlayerPlugin
    {
        public float diffToleranceSeconds = 2f;
        public DebugPlugin debug;

        private float latency = 0.1f;

        [UdonSynced, FieldChangeCallback(nameof(progress))]
        private float _progress;

        public float progress
        {
            set
            {
                if (Networking.GetOwner(this.gameObject) == Networking.LocalPlayer) return;

                float desiredProgress = value + this.latency;

                _progress = value;
                float localProgress = this.system.GetTime();
                float diff = desiredProgress - localProgress;

                // If diff is positive, the local player is ahead of the remote one
                if (diff > diffToleranceSeconds || diff < diffToleranceSeconds * -1) this.system.Seek(desiredProgress);
            }
            get => _progress;
        }

        [UdonSynced, FieldChangeCallback(nameof(isPlaying))]
        private bool _isPlaying;

        public bool isPlaying
        {
            set
            {
                if (Networking.GetOwner(this.gameObject) == Networking.LocalPlayer) return;

                _isPlaying = value;
                if (value) this.system.StartPlayback();
                else this.system.PausePlayback();
            }
            get => _isPlaying;
        }

        [UdonSynced, FieldChangeCallback(nameof(url))]
        private VRCUrl _url;

        public VRCUrl url
        {
            set
            {
                if (Networking.GetOwner(this.gameObject) == Networking.LocalPlayer) return;

                _url = value;

                if (value != null) this.system.LoadVideo(value);
            }
            get => _url;
        }

        protected override void OnProgress(float timestamp, float duration)
        {
            if (Networking.GetOwner(this.gameObject) != Networking.LocalPlayer) return;

            this._progress = timestamp;
            this.RequestSerialization();
        }

        protected override void OnLoadRequested(VRCUrl url)
        {
            if (Networking.GetOwner(this.gameObject) != Networking.LocalPlayer) return;

            this._url = url;
            this.RequestSerialization();
        }

        public void SyncUnload()
        {
            if (Networking.GetOwner(this.gameObject) == Networking.LocalPlayer) return;

            this.system.UnloadVideo();
        }

        protected override void OnUnload()
        {
            if (Networking.GetOwner(this.gameObject) != Networking.LocalPlayer) return;

            this.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(this.SyncUnload));

            this._url = null;
            this._isPlaying = false;
            this.RequestSerialization();
        }

        protected override void OnPlaybackStart(float timestamp)
        {
            if (Networking.GetOwner(this.gameObject) != Networking.LocalPlayer) return;

            this._isPlaying = true;
            this.RequestSerialization();
        }

        private float CalculateServerPing()
        {
            DateTime localTime = Networking.GetNetworkDateTime();
            TimeSpan serverTimeSpan = TimeSpan.FromMilliseconds(Networking.GetServerTimeInMilliseconds());
            DateTime serverTime = new DateTime(1970, 1, 1) + serverTimeSpan;
            TimeSpan difference = localTime - serverTime;

            return difference.Milliseconds;
        }

        protected override void OnPlaybackStop(float timestamp)
        {
            if (Networking.GetOwner(this.gameObject) != Networking.LocalPlayer) return;

            this._progress = timestamp;
            this._isPlaying = false;
            this.RequestSerialization();

            // Send a network event to sync the paused time, but only after one network roundtrip
            // to make sure everyone's paused by now.
            this.SendCustomEventDelayedSeconds(nameof(AfterOnPlaybackStop), this.CalculateServerPing() / 1000f);
        }

        public void AfterOnPlaybackStop()
        {
            this.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(SyncPausedTime));
        }

        public void SyncPausedTime()
        {
            if (Networking.GetOwner(this.gameObject) == Networking.LocalPlayer) return;

            // Continuously refine the latency by calculating the diff between the synced progress and the local progress
            // Next time that "play" is pressed, the stream will be offset by this amount
            float currentTime = this.system.GetTime();
            float diff = this.progress - currentTime;
            this.latency += diff;

            this.system.PausePlayback(this.progress);
        }

        protected override void OnLoadReady(float duration)
        {
            if (Networking.GetOwner(this.gameObject) == Networking.LocalPlayer) return;

            if (this.isPlaying) this.system.StartPlayback();
        }

        private int currentOwnerId = 0;

        protected override void OnVideoPlayerInit()
        {
            VRCPlayerApi owner = Networking.GetOwner(this.gameObject);
            if (owner == null || !owner.IsValid()) return;

            this.currentOwnerId = owner.playerId;
            this.events.OnOwnershipChanged(this.currentOwnerId, owner);
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            if (player == null || !player.IsValid()) return;

            this.events.OnOwnershipChanged(this.currentOwnerId, player);
            this.currentOwnerId = player.playerId;
        }

        private bool ownershipLocked = false;

        public override bool OnOwnershipRequest(VRCPlayerApi requestingPlayer, VRCPlayerApi requestedOwner)
        {
            if (this.ownershipLocked) return false;

            if (requestingPlayer == null || !requestingPlayer.IsValid()) return false;
            if (requestedOwner == null || !requestedOwner.IsValid()) return false;

            this.events.OnDebugLog($"{requestingPlayer.playerId} is requesting {requestedOwner.playerId} to be owner");
            return requestingPlayer.playerId == requestedOwner.playerId;
        }

        protected override void OnOwnershipSecurityChanged(bool locked)
        {
            this.ownershipLocked = locked;
        }

        protected override void OnOwnershipRequested()
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        }

        /* 
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
        */
    }
}
