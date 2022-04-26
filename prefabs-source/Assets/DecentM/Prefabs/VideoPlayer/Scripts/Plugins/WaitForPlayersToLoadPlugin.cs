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
    public class WaitForPlayersToLoadPlugin : VideoPlayerPlugin
    {
        public NetworkInterface unet;
        public ByteBufferReader reader;
        public ByteBufferWriter writer;

        protected override void _Start()
        {
            this.unet.AddEventsListener(this);
        }

        private const string VideoLoadedCommand = "VL";

        private int OnUNetReceived_sender;
        private byte[] OnUNetReceived_dataBuffer;
        private int OnUNetReceived_dataIndex;
        private int OnUNetReceived_dataLength;
        private int OnUNetReceived_id;
        public void OnUNetReceived()
        {
            if (OnUNetReceived_sender == Networking.LocalPlayer.playerId) return;

            string value = this.reader.ReadUTF8String(OnUNetReceived_dataLength, OnUNetReceived_dataBuffer, OnUNetReceived_dataIndex);
            string command = value.Split(null, 2)[0];
            string arguments = value.Split(null, 2)[1];

            switch (command)
            {
                case VideoLoadedCommand:
                    this.HandleVideoLoadedReceived(OnUNetReceived_sender);
                    break;

                default:
                    break;
            }
        }

        private int SendCommandAll(string command, string arguments)
        {
            string message = $"{command} {arguments}";
            int length = this.writer.GetUTF8StringSize(message);
            byte[] buffer = new byte[length + 1];
            this.writer.WriteUTF8String(message, buffer, 0);
            return this.unet.SendAll(false, buffer, length);
        }

        private int SendCommandTarget(bool sequenced, string command, int player, string arguments)
        {
            string message = $"{command} {arguments}";
            int length = this.writer.GetUTF8StringSize(message);
            byte[] buffer = new byte[length + 1];
            this.writer.WriteUTF8String(message, buffer, 0);
            return this.unet.SendTarget(sequenced, buffer, length, player);
        }

        private int SendCommandMaster(string command, string arguments)
        {
            string message = $"{command} {arguments}";
            int length = this.writer.GetUTF8StringSize(message);
            byte[] buffer = new byte[length + 1];
            this.writer.WriteUTF8String(message, buffer, 0);
            return this.unet.SendMaster(false, buffer, length);
        }

        // Everyone except the owner does this, because then owner already knows when it finishes loading
        protected override void OnLoadReady(float duration)
        {
            if (Networking.LocalPlayer.playerId == this.ownerId) return;

            this.SendCommandTarget(true, VideoLoadedCommand, this.ownerId, "");
        }

        private int ownerId = 0;

        protected override void OnOwnershipChanged(int previousOwnerId, VRCPlayerApi nextOwner)
        {
            if (nextOwner == null || !nextOwner.IsValid()) return;

            this.ownerId = nextOwner.playerId;
        }

        private int[] loadedPlayers = new int[0];

        protected override void OnLoadRequested(VRCUrl url)
        {
            this.loadedPlayers = new int[0];
        }

        // Only the current owner runs this, because the others are sending info to the owner
        // Wait for at least one ack
        private void HandleVideoLoadedReceived(int senderId)
        {
            if (Networking.LocalPlayer.playerId != this.ownerId) return;

            int[] tmp = new int[this.loadedPlayers.Length + 1];
            Array.Copy(this.loadedPlayers, tmp, this.loadedPlayers.Length);
            tmp[tmp.Length - 1] = senderId;
            this.loadedPlayers = tmp;

            this.events.OnRemotePlayerLoaded(this.loadedPlayers);

            if (this.loadedPlayers.Length >= VRCPlayerApi.GetPlayerCount() - 1)
            {
                this.system.StartPlayback();
            }
        }

        /*
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
        */
    }
}
