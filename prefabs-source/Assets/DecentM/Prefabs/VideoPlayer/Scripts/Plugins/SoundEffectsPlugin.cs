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

        protected override void OnAutoRetry(int attempt)
        {
            this.audioSource.PlayOneShot(this.autoRetry);
        }

        protected override void OnAutoRetryAbort()
        {
            this.audioSource.PlayOneShot(this.autoRetryAbort);
        }

        protected override void OnLoadReady(float duration)
        {
            this.audioSource.PlayOneShot(this.loadReady);
        }

        protected override void OnOwnershipChanged(int previousOwnerId, VRCPlayerApi nextOwner)
        {
            if (previousOwnerId == Networking.LocalPlayer.playerId && nextOwner != Networking.LocalPlayer)
            {
                this.audioSource.PlayOneShot(this.ownershipLost);
            }

            if (previousOwnerId != Networking.LocalPlayer.playerId && nextOwner == Networking.LocalPlayer)
            {
                this.audioSource.PlayOneShot(this.ownershipGained);
            }
        }

        protected override void OnOwnershipSecurityChanged(bool locked)
        {
            this.audioSource.PlayOneShot(locked ? this.ownershipLocked : this.ownershipUnlocked);
        }

        protected override void OnPlaybackEnd()
        {
            this.audioSource.PlayOneShot(this.playbackEnded);
        }

        protected override void OnRemotePlayerLoaded(int[] loadedPlayers)
        {
            this.audioSource.PlayOneShot(this.remotePlayerLoaded);
        }

        protected override void OnUnload()
        {
            this.audioSource.PlayOneShot(this.videoUnloaded);
        }
    }
}
