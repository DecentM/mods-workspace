using System;
using UnityEngine;
using TMPro;

namespace DecentM.Prefabs.VideoPlayer.Plugins
{
    public sealed class SoundEffectsPlugin : VideoPlayerPlugin
    {
        [Space]
        public AudioSource audioSource;

        [Space]
        public AudioClip autoRetry;
        public AudioClip autoRetryAbort;
        public AudioClip loadReady;
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
