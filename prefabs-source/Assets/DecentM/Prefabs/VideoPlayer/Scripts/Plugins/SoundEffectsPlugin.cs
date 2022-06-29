using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace DecentM.VideoPlayer.Plugins
{
    public sealed class SoundEffectsPlugin : VideoPlayerPlugin
    {
        [Space]
        public AudioSource audioSource;

        [Space]
        public AudioClip autoRetry;
        public AudioClip autoRetryAbort;
        public AudioClip loadReady;
        public AudioClip ownershipLocked;
        public AudioClip ownershipUnlocked;
        public AudioClip ownershipGained;
        public AudioClip ownershipLost;
        public AudioClip playbackEnded;
        public AudioClip remotePlayerLoaded;
        public AudioClip videoUnloaded;

        private void PlaySound(AudioClip clip)
        {
            if (this.system.IsPlaying())
                return;

            this.audioSource.PlayOneShot(clip);
        }

        protected override void OnAutoRetry(int attempt)
        {
            this.PlaySound(this.autoRetry);
        }

        protected override void OnAutoRetryAbort()
        {
            this.PlaySound(this.autoRetryAbort);
        }

        protected override void OnLoadReady(float duration)
        {
            this.PlaySound(this.loadReady);
        }

        protected override void OnOwnershipChanged(int previousOwnerId, VRCPlayerApi nextOwner)
        {
            if (
                previousOwnerId == Networking.LocalPlayer.playerId
                && nextOwner != Networking.LocalPlayer
            )
            {
                this.PlaySound(this.ownershipLost);
            }

            if (
                previousOwnerId != Networking.LocalPlayer.playerId
                && nextOwner == Networking.LocalPlayer
            )
            {
                this.PlaySound(this.ownershipGained);
            }
        }

        protected override void OnOwnershipSecurityChanged(bool locked)
        {
            this.PlaySound(locked ? this.ownershipLocked : this.ownershipUnlocked);
        }

        protected override void OnPlaybackEnd()
        {
            this.PlaySound(this.playbackEnded);
        }

        protected override void OnRemotePlayerLoaded(int loadedPlayers)
        {
            this.PlaySound(this.remotePlayerLoaded);
        }

        protected override void OnUnload()
        {
            this.PlaySound(this.videoUnloaded);
        }
    }
}
