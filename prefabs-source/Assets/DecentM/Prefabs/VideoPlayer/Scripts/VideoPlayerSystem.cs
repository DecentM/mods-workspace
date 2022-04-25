using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.VideoPlayer
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VideoPlayerSystem : UdonSharpBehaviour
    {
        public VideoPlayerEvents events;
        public BasePlayerHandler[] playerHandlers;
        public AudioSource[] speakers;
        public Renderer[] screens;

        public int fps = 30;
        public int minFps = 1;
        public int maxFps = 144;

        private int currentPlayerHandlerIndex = 0;

        public BasePlayerHandler currentPlayerHandler
        {
            get { return this.playerHandlers[currentPlayerHandlerIndex]; }
        }

        private void Start()
        {
            this.SendCustomEventDelayedSeconds(nameof(BroadcastInit), 0.1f);
        }

        public void BroadcastInit()
        {
            this.DisableAllPlayers();
            this.EnablePlayer(0);
            this.events.OnVideoPlayerInit();
        }

        [PublicAPI]
        public int GetFps()
        {
            return this.fps;
        }

        [PublicAPI]
        public void SetFps(int fps)
        {
            this.fps = Mathf.Clamp(fps, this.minFps, this.maxFps);
            this.events.OnFpsChange(fps);
        }

        [PublicAPI]
        public Texture GetVideoTexture()
        {
            return this.currentPlayerHandler.GetScreenTexture();
        }

        private void DisablePlayer(BasePlayerHandler player)
        {
            player.UnloadVideo();
            player.gameObject.SetActive(false);
        }

        private void DisablePlayer(int index)
        {
            BasePlayerHandler basePlayerHandler = this.playerHandlers[index];

            if (basePlayerHandler == null) return;

            this.DisablePlayer(basePlayerHandler);
        }

        private void EnablePlayer(BasePlayerHandler player)
        {
            player.gameObject.SetActive(true);
            player.UnloadVideo();
        }

        private void EnablePlayer(int index)
        {
            BasePlayerHandler basePlayerHandler = this.playerHandlers[index];

            if (basePlayerHandler == null) return;

            this.EnablePlayer(basePlayerHandler);
        }

        private void DisableAllPlayers()
        {
            foreach (BasePlayerHandler player in playerHandlers)
            {
                this.DisablePlayer(player);
            }
        }

        [PublicAPI]
        public bool NextPlayerHandler()
        {
            if (this.playerHandlers.Length == 0) return false;

            int newIndex = this.currentPlayerHandlerIndex + 1;

            if (
                newIndex >= this.playerHandlers.Length
                || newIndex < 0
                || this.currentPlayerHandlerIndex == newIndex
                || this.playerHandlers[newIndex] == null
            ) {
                newIndex = 0;
            }

            this.DisablePlayer(this.currentPlayerHandler);
            this.currentPlayerHandlerIndex = newIndex;
            this.EnablePlayer(this.currentPlayerHandler);

            return true;
        }

        [PublicAPI]
        public bool IsPlaying()
        {
            return this.currentPlayerHandler.IsPlaying();
        }

        [PublicAPI]
        public void StartPlayback()
        {
            this.currentPlayerHandler.StartPlayback();
        }

        [PublicAPI]
        public void StartPlayback(float timestamp)
        {
            this.currentPlayerHandler.StartPlayback(timestamp);
        }

        [PublicAPI]
        public void Seek(float timestamp)
        {
            this.currentPlayerHandler.Seek(timestamp);
        }

        [PublicAPI]
        public void PausePlayback(float timestamp)
        {
            this.currentPlayerHandler.PausePlayback();
        }

        [PublicAPI]
        public void PausePlayback()
        {
            this.currentPlayerHandler.PausePlayback();
        }

        private VRCUrl currentUrl;

        [PublicAPI]
        public void LoadVideo(VRCUrl url)
        {
            this.currentUrl = url;
            this.currentPlayerHandler.LoadVideo(url);
        }

        [PublicAPI]
        public void UnloadVideo()
        {
            this.currentUrl = null;
            this.currentPlayerHandler.UnloadVideo();
        }

        [PublicAPI]
        public VRCUrl GetCurrentUrl()
        {
            return this.currentUrl;
        }

        [PublicAPI]
        public bool SetBrightness(float alpha)
        {
            if (alpha < 0 || alpha > 1) return false;

            foreach (Renderer screen in this.screens)
            {
                screen.material.SetColor("_Color", new Color(1, 1, 1, alpha));
            }

            this.events.OnBrightnessChange(alpha);

            return true;
        }

        private float currentVolume = 1;

        [PublicAPI]
        public bool SetVolume(float volume)
        {
            if (volume < 0 || volume > 1) return false;

            foreach (AudioSource speaker in this.speakers)
            {
                speaker.volume = volume;
            }

            this.events.OnVolumeChange(volume);

            return true;
        }

        [PublicAPI]
        public void SetMuted(bool muted)
        {
            foreach (AudioSource speaker in this.speakers)
            {
                speaker.volume = muted ? 0 : this.currentVolume;
            }

            this.events.OnMutedChange(muted);
        }
    }
}
