using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace DecentM.VideoPlayer.Plugins
{
    public class DebugPlugin : VideoPlayerPlugin
    {
        public TextMeshProUGUI gui;

        string[] logs = new string[40];

        public void Log(params string[] messages)
        {
            string[] tmp = new string[logs.Length];
            Array.Copy(logs, 1, tmp, 0, this.logs.Length - 1);
            tmp[tmp.Length - 1] = String.Join(" ", messages);
            this.logs = tmp;

            if (this.gui != null)
                this.gui.text = String.Join("\n", this.logs);
        }

        protected override void _Start()
        {
            if (this.gui == null)
            {
                this.enabled = false;
                return;
            }

            this.Log(nameof(_Start));
        }

        protected override void OnDebugLog(string message)
        {
            this.Log(message);
        }

        protected override void OnVideoPlayerInit()
        {
            this.Log(nameof(OnVideoPlayerInit));
        }

        protected override void OnPlayerSwitch(VideoPlayerHandlerType type)
        {
            this.Log(nameof(OnPlayerSwitch), type.ToString());
        }

        protected override void OnPlaybackEnd()
        {
            this.Log(nameof(OnPlaybackEnd));
        }

        protected override void OnLoadReady(float duration)
        {
            this.Log(nameof(OnLoadReady), duration.ToString());
        }

        protected override void OnLoadBegin()
        {
            this.Log(nameof(OnLoadBegin));
        }

        protected override void OnLoadBegin(VRCUrl url)
        {
            this.Log(nameof(OnLoadBegin), "(with URL)");
        }

        protected override void OnLoadError(VideoError videoError)
        {
            this.Log(nameof(OnLoadError), videoError.ToString());
        }

        protected override void OnProgress(float timestamp, float duration)
        {
            this.Log(nameof(OnProgress), timestamp.ToString(), duration.ToString());
        }

        protected override void OnUnload()
        {
            this.Log(nameof(OnUnload));
        }

        protected override void OnPlaybackStart(float timestamp)
        {
            this.Log(nameof(OnPlaybackStart), timestamp.ToString());
        }

        protected override void OnPlaybackStop(float timestamp)
        {
            this.Log(nameof(OnPlaybackStop), timestamp.ToString());
        }

        protected override void OnAutoRetry(int attempt)
        {
            this.Log(nameof(OnAutoRetry), attempt.ToString());
        }

        protected override void OnAutoRetryLoadTimeout(int failures)
        {
            this.Log(nameof(OnAutoRetryLoadTimeout), failures.ToString());
        }

        protected override void OnAutoRetryAbort()
        {
            this.Log(nameof(OnAutoRetryAbort));
        }

        protected override void OnBrightnessChange(float alpha)
        {
            this.Log(nameof(OnBrightnessChange), alpha.ToString());
        }

        protected override void OnVolumeChange(float volume, bool muted)
        {
            this.Log(nameof(OnVolumeChange), volume.ToString(), muted.ToString());
        }

        protected override void OnMutedChange(bool muted, float volume)
        {
            this.Log(nameof(OnMutedChange), muted.ToString(), volume.ToString());
        }

        protected override void OnFpsChange(int fps)
        {
            this.Log(nameof(OnFpsChange), fps.ToString());
        }

        protected override void OnScreenResolutionChange(
            ScreenHandler screen,
            float width,
            float height
        )
        {
            this.Log(
                nameof(OnScreenResolutionChange),
                screen.name,
                width.ToString(),
                height.ToString()
            );
        }

        protected override void OnLoadRequested(VRCUrl url)
        {
            this.Log(nameof(OnLoadRequested), "(with URL)");
        }

        protected override void OnLoadApproved(VRCUrl url)
        {
            this.Log(nameof(OnLoadApproved), "(with URL)");
        }

        protected override void OnLoadDenied(VRCUrl url, string reason)
        {
            this.Log(nameof(OnLoadDenied), $"(with URL) {reason}");
        }

        protected override void OnOwnershipChanged(int previousOwnerId, VRCPlayerApi nextOwner)
        {
            this.Log(
                nameof(OnOwnershipChanged),
                previousOwnerId.ToString(),
                nextOwner.playerId.ToString()
            );
        }

        protected override void OnOwnershipRequested()
        {
            this.Log(nameof(OnOwnershipRequested));
        }

        protected override void OnOwnershipSecurityChanged(bool locked)
        {
            this.Log(nameof(OnOwnershipSecurityChanged), locked.ToString());
        }

        protected override void OnScreenTextureChange()
        {
            this.Log(nameof(OnScreenTextureChange));
        }

        protected override void OnRemotePlayerLoaded(int[] loadedPlayers)
        {
            this.Log(nameof(OnRemotePlayerLoaded), loadedPlayers.Length.ToString());
        }

        protected override void OnAutoRetryAllPlayersFailed()
        {
            this.Log(nameof(OnAutoRetryAllPlayersFailed));
        }

        protected override void OnMetadataChange(
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
        )
        {
            this.Log(
                nameof(OnMetadataChange),
                viewCount.ToString(),
                likeCount.ToString(),
                fps.ToString(),
                duration,
                subtitles.Length.ToString()
            );
        }

        protected override void OnSubtitleRender(string text)
        {
            this.Log(nameof(OnSubtitleRender), $"({text.Length} long string)");
        }

        protected override void OnSubtitleClear()
        {
            this.Log(nameof(OnSubtitleClear));
        }

        protected override void OnSubtitleLanguageOptionsChange(string[][] newOptions)
        {
            this.Log(nameof(OnSubtitleLanguageOptionsChange), newOptions.Length.ToString());
        }

        protected override void OnSubtitleLanguageRequested(string language)
        {
            this.Log(nameof(OnSubtitleLanguageRequested), language);
        }
    }
}
