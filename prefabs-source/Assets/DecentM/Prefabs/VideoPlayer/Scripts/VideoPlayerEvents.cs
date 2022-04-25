
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components.Video;
using DecentM.Pubsub;

namespace DecentM.VideoPlayer
{
    public enum VideoPlayerEvent
    {
        OnVideoPlayerInit,
        OnBrightnessChange,
        OnVolumeChange,
        OnMutedChange,
        OnFpsChange,
        OnScreenResolutionChange,

        OnPlaybackStart,
        OnPlaybackStop,
        OnPlaybackEnd,
        OnProgress,

        OnLoadBegin,
        OnLoadReady,
        OnLoadError,
        OnUnload,

        OnAutoRetry,
        OnAutoRetrySwitchPlayer,
        OnAutoRetryLoadTimeout,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public sealed class VideoPlayerEvents : PubsubHost
    {
        #region Core

        public void OnVideoPlayerInit()
        {
            this.BroadcastEvent(VideoPlayerEvent.OnVideoPlayerInit);
        }

        public void OnBrightnessChange(float alpha)
        {
            this.BroadcastEvent(VideoPlayerEvent.OnBrightnessChange, alpha);
        }

        public void OnVolumeChange(float volume)
        {
            this.BroadcastEvent(VideoPlayerEvent.OnVolumeChange, volume);
        }

        public void OnMutedChange(bool muted)
        {
            this.BroadcastEvent(VideoPlayerEvent.OnMutedChange, muted);
        }

        public void OnFpsChange(int fps)
        {
            this.BroadcastEvent(VideoPlayerEvent.OnFpsChange, fps);
        }

        public void OnScreenResolutionChange(Renderer screen, float width, float height)
        {
            this.BroadcastEvent(VideoPlayerEvent.OnScreenResolutionChange, screen, width, height);
        }

        public void OnPlaybackStart(float timestamp)
        {
            this.BroadcastEvent(VideoPlayerEvent.OnPlaybackStart, timestamp);
        }

        public void OnPlaybackStop(float timestamp)
        {
            this.BroadcastEvent(VideoPlayerEvent.OnPlaybackStop, timestamp);
        }

        public void OnProgress(float timestamp, float duration)
        {
            this.BroadcastEvent(VideoPlayerEvent.OnProgress, timestamp, duration);
        }

        public void OnPlaybackEnd()
        {
            this.BroadcastEvent(VideoPlayerEvent.OnPlaybackEnd);
        }

        public void OnLoadBegin(VRCUrl url)
        {
            this.BroadcastEvent(VideoPlayerEvent.OnLoadBegin, url);
        }

        public void OnLoadReady(float duration)
        {
            this.BroadcastEvent(VideoPlayerEvent.OnLoadReady, duration);
        }

        public void OnUnload()
        {
            this.BroadcastEvent(VideoPlayerEvent.OnUnload);
        }

        public void OnLoadError(VideoError videoError)
        {
            this.BroadcastEvent(VideoPlayerEvent.OnLoadError, videoError);
        }

        #endregion

        #region Auto Retry plugin

        public void OnAutoRetry(int attempt)
        {
            this.BroadcastEvent(VideoPlayerEvent.OnAutoRetry, attempt);
        }

        public void OnAutoRetrySwitchPlayer()
        {
            this.BroadcastEvent(VideoPlayerEvent.OnAutoRetrySwitchPlayer);
        }

        public void OnAutoRetryLoadTimeout()
        {
            this.BroadcastEvent(VideoPlayerEvent.OnAutoRetryLoadTimeout);
        }

        #endregion
    }
}
