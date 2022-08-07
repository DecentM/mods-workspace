using JetBrains.Annotations;
using UnityEngine;

using DecentM.Prefabs.VideoPlayer.Handlers;

namespace DecentM.Prefabs.VideoPlayer
{
    public enum VideoErrorType
    {
        Unknown,
        HttpError,
        Timeout,
    }

    public struct VideoError
    {
        public VideoError(VideoErrorType type, string message = null)
        {
            this.type = type;
            this.message = message;
        }

        public VideoErrorType type;
        public string message;
    }

    public class VideoPlayerSystem : MonoBehaviour
    {
        public VideoPlayerEvents events;
        public PlayerHandler[] playerHandlers;
        public AudioSource[] speakers;
        public ScreenHandler[] screens;

        public bool muted = false;
        public float volume = 1.0f;
        public float brightness = 1.0f;
        public int minFps = 1;
        public int maxFps = 144;

        private int currentPlayerHandlerIndex = 0;

        public PlayerHandler currentPlayerHandler
        {
            get { return this.playerHandlers[currentPlayerHandlerIndex]; }
        }

        private void Start()
        {
            Invoke(nameof(BroadcastInit), 0.1f);
        }

        public void BroadcastInit()
        {
            this.DisableAllPlayers();
            this.EnablePlayer(0);
            this.SetMuted(this.muted);
            this.SetVolume(this.volume);
            this.SetBrightness(this.brightness);
            this.PausePlayback();
            this.Seek(0);
            this.events.OnVideoPlayerInit();
        }

        [PublicAPI]
        public int ScreenCount
        {
            get { return this.screens.Length; }
        }

        [PublicAPI]
        public void RenderCurrentFrame(RenderTexture texture, int screenIndex)
        {
            if (this.screens == null || this.screens.Length == 0)
                return;
            if (screenIndex >= this.screens.Length || screenIndex < 0)
                screenIndex = 0;

            ScreenHandler screen = this.screens[screenIndex];
            screen.RenderToRenderTexture(texture);
        }

        [PublicAPI]
        public void RenderCurrentFrame(RenderTexture texture)
        {
            this.RenderCurrentFrame(texture, 0);
        }

        [PublicAPI]
        public Texture GetVideoTexture()
        {
            return this.currentPlayerHandler.GetScreenTexture();
        }

        [PublicAPI]
        public void SetScreenTexture(Texture texture)
        {
            foreach (ScreenHandler screen in this.screens)
            {
                screen.SetTexture(texture);
            }
        }

        private void DisablePlayer(PlayerHandler player)
        {
            player.Unload();
            player.gameObject.SetActive(false);
        }

        private void DisablePlayer(int index)
        {
            PlayerHandler PlayerHandler = this.playerHandlers[index];

            if (PlayerHandler == null)
                return;

            this.DisablePlayer(PlayerHandler);
        }

        private void EnablePlayer(PlayerHandler player)
        {
            player.gameObject.SetActive(true);
            player.Unload();
        }

        private void EnablePlayer(int index)
        {
            PlayerHandler PlayerHandler = this.playerHandlers[index];

            if (PlayerHandler == null)
                return;

            this.EnablePlayer(PlayerHandler);
        }

        private void DisableAllPlayers()
        {
            foreach (PlayerHandler player in playerHandlers)
            {
                this.DisablePlayer(player);
            }
        }

        [PublicAPI]
        public int NextPlayerHandler()
        {
            if (this.playerHandlers.Length == 0)
                return -1;

            int newIndex = this.currentPlayerHandlerIndex + 1;

            if (
                newIndex >= this.playerHandlers.Length
                || newIndex < 0
                || this.currentPlayerHandlerIndex == newIndex
                || this.playerHandlers[newIndex] == null
            )
            {
                newIndex = 0;
            }

            this.DisablePlayer(this.currentPlayerHandler);
            this.currentPlayerHandlerIndex = newIndex;
            this.EnablePlayer(this.currentPlayerHandler);
            this.events.OnPlayerSwitch(this.currentPlayerHandler.type);

            return newIndex;
        }

        [PublicAPI]
        public bool IsPlaying()
        {
            return this.currentPlayerHandler.IsPlaying;
        }

        [PublicAPI]
        public void StartPlayback()
        {
            this.currentPlayerHandler.Play();
        }

        [PublicAPI]
        public void StartPlayback(float timestamp)
        {
            this.currentPlayerHandler.Play(timestamp);
        }

        [PublicAPI]
        public void Seek(float timestamp)
        {
            this.currentPlayerHandler.SetTime(timestamp);
            this.events.OnProgress(timestamp, this.GetDuration());
        }

        [PublicAPI]
        public void PausePlayback(float timestamp)
        {
            this.currentPlayerHandler.Pause();
            this.currentPlayerHandler.SetTime(timestamp);
            this.events.OnPlaybackStop(timestamp);
        }

        [PublicAPI]
        public void PausePlayback()
        {
            this.currentPlayerHandler.Pause();
            this.events.OnPlaybackStop(this.currentPlayerHandler.GetTime());
        }

        private string currentUrl;

        [PublicAPI]
        public void RequestVideo(string url)
        {
            this.events.OnLoadRequested(url);
        }

        public void LoadVideo(string url)
        {
            this.currentUrl = url;
            this.currentPlayerHandler.LoadURL(url);
        }

        [PublicAPI]
        public void Unload()
        {
            this.currentUrl = null;
            this.currentPlayerHandler.Unload();
            this.events.OnUnload();
            this.Seek(0);
        }

        [PublicAPI]
        public string GetCurrentUrl()
        {
            return this.currentUrl;
        }

        [PublicAPI]
        public float GetBrightness()
        {
            if (this.screens.Length == 0)
                return 1f;

            ScreenHandler screen = this.screens[0];
            return screen.GetBrightness();
        }

        [PublicAPI]
        public bool SetBrightness(float alpha)
        {
            if (alpha < 0 || alpha > 1)
                return false;

            foreach (ScreenHandler screen in this.screens)
            {
                screen.SetBrightness(alpha);
            }

            this.events.OnBrightnessChange(alpha);

            return true;
        }

        [PublicAPI]
        public bool SetVolume(float volume)
        {
            if (volume < 0 || volume > 1)
                return false;

            foreach (AudioSource speaker in this.speakers)
            {
                speaker.volume = volume;
            }

            this.volume = volume;
            this.events.OnVolumeChange(volume, this.muted);

            return true;
        }

        [PublicAPI]
        public float GetVolume()
        {
            return this.volume;
        }

        [PublicAPI]
        public void SetMuted(bool muted)
        {
            foreach (AudioSource speaker in this.speakers)
            {
                speaker.volume = muted ? 0 : this.volume;
            }

            this.muted = muted;
            this.events.OnMutedChange(muted, this.volume);
        }

        [PublicAPI]
        public bool GetMuted()
        {
            return this.muted;
        }

        [PublicAPI]
        public float GetDuration()
        {
            return this.currentPlayerHandler.GetDuration();
        }

        [PublicAPI]
        public float GetTime()
        {
            return this.currentPlayerHandler.GetTime();
        }
    }
}
