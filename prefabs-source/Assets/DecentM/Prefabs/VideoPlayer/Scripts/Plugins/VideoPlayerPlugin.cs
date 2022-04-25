using UdonSharp;
using DecentM.Pubsub;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Components.Video;

namespace DecentM.VideoPlayer.Plugins
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class VideoPlayerPlugin : PubsubSubscriber
    {
        public VideoPlayerSystem system;
        public VideoPlayerEvents events;

        protected virtual void OnVideoPlayerInit() { }
        protected virtual void OnBrightnessChange(float alpha) { }
        protected virtual void OnVolumeChange(float volume, bool muted) { }
        protected virtual void OnMutedChange(bool muted, float volume) { }
        protected virtual void OnFpsChange(int fps) { }
        protected virtual void OnScreenResolutionChange(Renderer screen, float width, float height) { }

        protected virtual void OnPlaybackStart(float timestamp) { }
        protected virtual void OnPlaybackStop(float timestamp) { }
        protected virtual void OnPlaybackEnd() { }
        protected virtual void OnProgress(float timestamp, float duration) { }

        protected virtual void OnLoadBegin(VRCUrl url) { }
        protected virtual void OnLoadBegin() { }
        protected virtual void OnLoadReady(float duration) { }
        protected virtual void OnLoadError(VideoError videoError) { }
        protected virtual void OnUnload() { }

        protected virtual void OnAutoRetry(int attempt) { }
        protected virtual void OnAutoRetrySwitchPlayer() { }
        protected virtual void OnAutoRetryLoadTimeout() { }

        protected sealed override void OnPubsubEvent(object name, object[] data)
        {
            switch (name)
            {
                #region Core

                case VideoPlayerEvent.OnLoadBegin:
                    {
                        VRCUrl url = (VRCUrl)data[0];
                        if (url == null) this.OnLoadBegin();
                        else this.OnLoadBegin(url);
                        return;
                    }

                case VideoPlayerEvent.OnBrightnessChange:
                    {
                        float alpha = (float)data[0];
                        this.OnBrightnessChange(alpha);
                        return;
                    }

                case VideoPlayerEvent.OnVolumeChange:
                    {
                        float volume = (float)data[0];
                        bool muted = (bool)data[1];
                        this.OnVolumeChange(volume, muted);
                        return;
                    }

                case VideoPlayerEvent.OnMutedChange:
                    {
                        bool muted = (bool)data[0];
                        float volume = (float)data[1];
                        this.OnMutedChange(muted, volume);
                        return;
                    }

                case VideoPlayerEvent.OnFpsChange:
                    {
                        int fps = (int)data[0];
                        this.OnFpsChange(fps);
                        return;
                    }

                case VideoPlayerEvent.OnScreenResolutionChange:
                    {
                        Renderer screen = (Renderer)data[0];
                        float width = (float)data[1];
                        float height = (float)data[2];
                        this.OnScreenResolutionChange(screen, width, height);
                        return;
                    }

                case VideoPlayerEvent.OnLoadReady:
                    {
                        float duration = (float)data[0];
                        this.OnLoadReady(duration);
                        return;
                    }

                case VideoPlayerEvent.OnLoadError:
                    {
                        VideoError videoError = (VideoError)data[0];
                        this.OnLoadError(videoError);
                        return;
                    }

                case VideoPlayerEvent.OnUnload:
                    {
                        this.OnUnload();
                        return;
                    }

                case VideoPlayerEvent.OnPlaybackStart:
                    {
                        float timestamp = (float)data[0];
                        this.OnPlaybackStart(timestamp);
                        return;
                    }

                case VideoPlayerEvent.OnPlaybackStop:
                    {
                        float timestamp = (float)data[0];
                        this.OnPlaybackStop(timestamp);
                        return;
                    }

                case VideoPlayerEvent.OnProgress:
                    {
                        float timestamp = (float)data[0];
                        float duration = (float)data[1];
                        this.OnProgress(timestamp, duration);
                        return;
                    }

                case VideoPlayerEvent.OnPlaybackEnd:
                    {
                        this.OnPlaybackEnd();
                        return;
                    }

                case VideoPlayerEvent.OnVideoPlayerInit:
                    {
                        this.OnVideoPlayerInit();
                        return;
                    }

                #endregion

                #region Plugins

                case VideoPlayerEvent.OnAutoRetry:
                    {
                        int attempt = (int)data[0];
                        this.OnAutoRetry(attempt);
                        return;
                    }

                case VideoPlayerEvent.OnAutoRetrySwitchPlayer:
                    {
                        this.OnAutoRetrySwitchPlayer();
                        return;
                    }

                case VideoPlayerEvent.OnAutoRetryLoadTimeout:
                    {
                        this.OnAutoRetryLoadTimeout();
                        return;
                    }

                #endregion
            }
        }
    }
}
