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

        protected virtual void OnDebugLog(string message) { }

        protected virtual void OnVideoPlayerInit() { }

        protected virtual void OnBrightnessChange(float alpha) { }

        protected virtual void OnVolumeChange(float volume, bool muted) { }

        protected virtual void OnMutedChange(bool muted, float volume) { }

        protected virtual void OnFpsChange(int fps) { }

        protected virtual void OnScreenResolutionChange(
            ScreenHandler screen,
            float width,
            float height
        ) { }

        protected virtual void OnScreenTextureChange() { }

        protected virtual void OnPlayerSwitch(VideoPlayerHandlerType type) { }

        protected virtual void OnPlaybackStart(float timestamp) { }

        protected virtual void OnPlaybackStop(float timestamp) { }

        protected virtual void OnPlaybackEnd() { }

        protected virtual void OnProgress(float timestamp, float duration) { }

        protected virtual void OnLoadBegin(VRCUrl url) { }

        protected virtual void OnLoadBegin() { }

        protected virtual void OnLoadReady(float duration) { }

        protected virtual void OnLoadError(VideoError videoError) { }

        protected virtual void OnUnload() { }

        protected virtual void OnLoadRequested(VRCUrl url) { }

        protected virtual void OnLoadApproved(VRCUrl url) { }

        protected virtual void OnLoadDenied(VRCUrl url, string reason) { }

        protected virtual void OnLoadRatelimitWaiting() { }

        protected virtual void OnAutoRetry(int attempt) { }

        protected virtual void OnAutoRetryLoadTimeout(int timeout) { }

        protected virtual void OnAutoRetryAbort() { }

        protected virtual void OnAutoRetryAllPlayersFailed() { }

        protected virtual void OnOwnershipChanged(int previousOwnerId, VRCPlayerApi nextOwner) { }

        protected virtual void OnOwnershipSecurityChanged(bool locked) { }

        protected virtual void OnOwnershipRequested() { }

        protected virtual void OnRemotePlayerLoaded(int loadedPlayers) { }

        protected virtual void OnSubtitleRender(string text) { }

        protected virtual void OnSubtitleClear() { }

        protected virtual void OnSubtitleLanguageOptionsChange(string[][] newOptions) { }

        protected virtual void OnSubtitleLanguageRequested(string language) { }

        protected virtual void OnMetadataChange(
            string title,
            string uploader,
            string siteName,
            int viewCount,
            int likeCount,
            string resolution,
            int fps,
            string description,
            string duration,
            TextAsset[] subtitles
        ) { }

        public sealed override void OnPubsubEvent(object name, object[] data)
        {
            switch (name)
            {
                #region Core

                case VideoPlayerEvent.OnDebugLog:
                {
                    string message = (string)data[0];
                    this.OnDebugLog(message);
                    return;
                }

                case VideoPlayerEvent.OnLoadRequested:
                {
                    VRCUrl url = (VRCUrl)data[0];
                    this.OnLoadRequested(url);
                    return;
                }

                case VideoPlayerEvent.OnLoadApproved:
                {
                    VRCUrl url = (VRCUrl)data[0];
                    this.OnLoadApproved(url);
                    return;
                }

                case VideoPlayerEvent.OnLoadDenied:
                {
                    VRCUrl url = (VRCUrl)data[0];
                    string reason = (string)data[1];
                    this.OnLoadDenied(url, reason);
                    return;
                }

                case VideoPlayerEvent.OnLoadBegin:
                {
                    VRCUrl url = (VRCUrl)data[0];
                    if (url == null)
                        this.OnLoadBegin();
                    else
                        this.OnLoadBegin(url);
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
                    ScreenHandler screen = (ScreenHandler)data[0];
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

                case VideoPlayerEvent.OnLoadRatelimitWaiting:
                {
                    this.OnLoadRatelimitWaiting();
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

                case VideoPlayerEvent.OnPlayerSwitch:
                {
                    VideoPlayerHandlerType type = (VideoPlayerHandlerType)data[0];
                    this.OnPlayerSwitch(type);
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

                case VideoPlayerEvent.OnAutoRetryLoadTimeout:
                {
                    int timeout = (int)data[0];
                    this.OnAutoRetryLoadTimeout(timeout);
                    return;
                }

                case VideoPlayerEvent.OnAutoRetryAbort:
                {
                    this.OnAutoRetryAbort();
                    return;
                }

                case VideoPlayerEvent.OnAutoRetryAllPlayersFailed:
                {
                    this.OnAutoRetryAllPlayersFailed();
                    return;
                }

                case VideoPlayerEvent.OnOwnershipChanged:
                {
                    int previousOwnerId = (int)data[0];
                    VRCPlayerApi nextOwner = (VRCPlayerApi)data[1];
                    this.OnOwnershipChanged(previousOwnerId, nextOwner);
                    return;
                }

                case VideoPlayerEvent.OnOwnershipSecurityChanged:
                {
                    bool locked = (bool)data[0];
                    this.OnOwnershipSecurityChanged(locked);
                    return;
                }

                case VideoPlayerEvent.OnOwnershipRequested:
                {
                    this.OnOwnershipRequested();
                    return;
                }

                case VideoPlayerEvent.OnScreenTextureChange:
                {
                    this.OnScreenTextureChange();
                    return;
                }

                case VideoPlayerEvent.OnRemotePlayerLoaded:
                {
                    int loadedPlayers = (int)data[0];
                    this.OnRemotePlayerLoaded(loadedPlayers);
                    return;
                }

                case VideoPlayerEvent.OnMetadataChange:
                {
                    string title = (string)data[0];
                    string uploader = (string)data[1];
                    string siteName = (string)data[2];
                    int viewCount = (int)data[3];
                    int likeCount = (int)data[4];
                    string resolution = (string)data[5];
                    int fps = (int)data[6];
                    string description = (string)data[7];
                    string duration = (string)data[8];
                    TextAsset[] subtitles = (TextAsset[])data[9];

                    this.OnMetadataChange(
                        title,
                        uploader,
                        siteName,
                        viewCount,
                        likeCount,
                        resolution,
                        fps,
                        description,
                        duration,
                        subtitles
                    );
                    return;
                }

                case VideoPlayerEvent.OnSubtitleRender:
                {
                    string text = (string)data[0];
                    this.OnSubtitleRender(text);
                    return;
                }

                case VideoPlayerEvent.OnSubtitleClear:
                {
                    this.OnSubtitleClear();
                    return;
                }

                case VideoPlayerEvent.OnSubtitleLanguageOptionsChange:
                {
                    string[][] newOptions = (string[][])data;
                    this.OnSubtitleLanguageOptionsChange(newOptions);
                    return;
                }

                case VideoPlayerEvent.OnSubtitleLanguageRequested:
                {
                    string language = (string)data[0];
                    this.OnSubtitleLanguageRequested(language);
                    return;
                }

                default:
                {
                    break;
                }

                    #endregion
            }
        }
    }
}
