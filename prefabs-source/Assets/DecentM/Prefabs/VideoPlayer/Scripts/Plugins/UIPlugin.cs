using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using UnityEngine.UI;

namespace DecentM.VideoPlayer.Plugins
{
    public sealed class UIPlugin : VideoPlayerPlugin
    {
        public Slider progress;

        public Button playButton;
        public Button pauseButton;
        public Button stopButton;

        public TextMeshProUGUI status;

        public Image brightnessImage;
        public Slider brightnessSlider;
        public Image volumeImage;
        public Slider volumeSlider;

        [Space]
        public Sprite volumeMutedIcon;
        public Sprite volumeLowIcon;
        public Sprite volumeMediumIcon;
        public Sprite volumeHighIcon;
        public Sprite brightnessLowIcon;
        public Sprite brightnessMediumIcon;
        public Sprite brightnessHighIcon;

        public TextMeshProUGUI info;

        private string HumanReadableTimestamp(float timestamp)
        {
            TimeSpan t = TimeSpan.FromSeconds(timestamp);

            if (t.Hours > 0) return string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);

            return string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
        }

        protected override void OnProgress(float timestamp, float duration)
        {
            this.status.text = $"{this.HumanReadableTimestamp(timestamp)} / {this.HumanReadableTimestamp(duration)}";
            this.progress.SetValueWithoutNotify(timestamp / duration);
        }

        protected override void OnAutoRetry(int attempt)
        {
            this.status.text = $"Retrying (attempt {attempt + 1})...";
        }

        protected override void OnAutoRetrySwitchPlayer()
        {
            this.status.text = "Trying with a different player...";
        }

        protected override void OnAutoRetryLoadTimeout()
        {
            this.status.text = "Load timeout, retrying in 10 seconds...";
        }

        protected override void OnLoadBegin()
        {
            this.status.text = "Loading...";
        }

        protected override void OnLoadBegin(VRCUrl url)
        {
            this.status.text = "Loading...";
        }

        protected override void OnLoadReady(float duration)
        {
            this.status.text = "Loaded";
        }

        protected override void OnLoadError(VideoError videoError)
        {
            switch (videoError)
            {
                case VideoError.InvalidURL:
                    this.status.text = "Invalid URL";
                    break;

                case VideoError.AccessDenied:
                    this.status.text = "Access denied";
                    break;

                // We don't care about the rest of the errors as they're handled by the AutoRetry plugin
            }
        }

        private Sprite GetBrightnessSprite(float alpha)
        {
            if (alpha < 1f / 3f) return this.brightnessLowIcon;
            if (alpha < 2f / 3f) return this.brightnessMediumIcon;

            return this.brightnessHighIcon;
        }

        protected override void OnBrightnessChange(float alpha)
        {
            this.brightnessSlider.SetValueWithoutNotify(alpha);
            this.brightnessImage.sprite = this.GetBrightnessSprite(alpha);
        }

        private Sprite GetVolumeSprite(float volume, bool muted)
        {
            if (muted || volume == 0) return this.volumeMutedIcon;

            if (volume < 1f / 3f) return this.volumeLowIcon;
            if (volume < 2f / 3f) return this.volumeMediumIcon;

            return this.volumeHighIcon;
        }

        protected override void OnVolumeChange(float volume, bool muted)
        {
            this.volumeSlider.SetValueWithoutNotify(volume);
            this.volumeImage.sprite = this.GetVolumeSprite(volume, muted);
        }

        protected override void OnMutedChange(bool muted, float volume)
        {
            this.volumeImage.sprite = this.GetVolumeSprite(volume, muted);
        }

        protected override void OnPlaybackEnd()
        {
            this.status.text = "Playback ended";

            this.playButton.gameObject.SetActive(true);
            this.pauseButton.gameObject.SetActive(false);
            this.stopButton.gameObject.SetActive(false);
        }

        protected override void OnPlaybackStart(float timestamp)
        {
            this.status.text = "Playing...";

            this.playButton.gameObject.SetActive(false);
            this.pauseButton.gameObject.SetActive(true);
            this.stopButton.gameObject.SetActive(true);
        }

        protected override void OnPlaybackStop(float timestamp)
        {
            this.status.text = "Paused";

            this.playButton.gameObject.SetActive(true);
            this.pauseButton.gameObject.SetActive(false);
            this.stopButton.gameObject.SetActive(true);
        }

        protected override void OnUnload()
        {
            this.status.text = "Stopped";

            this.playButton.gameObject.SetActive(false);
            this.pauseButton.gameObject.SetActive(false);
            this.stopButton.gameObject.SetActive(false);
        }

        protected override void OnScreenResolutionChange(Renderer screen, float width, float height)
        {
            this.info.text = $"{height}p";
        }
    }
}
