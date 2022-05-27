using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

using DecentM.Metrics.Plugins;

namespace DecentM.VideoPlayer.Plugins
{
    public class VideoPlayerMetrics : VideoPlayerPlugin
    {
        public VideoPlayerTracker tracker;

        protected override void OnPlaybackStart(float timestamp)
        {
            // Only track the first play event, not after pausing
            if (timestamp > 0.1f)
                return;

            this.tracker.OnPlay();
        }

        protected override void OnPlaybackStop(float timestamp)
        {
            this.tracker.OnStop();
        }

        protected override void OnLoadReady(float duration)
        {
            if (float.IsInfinity(duration))
            {
                this.tracker.OnLiveLoaded();
            }
            else
            {
                this.tracker.OnVodLoaded();
            }
        }

        protected override void OnAutoRetryAllPlayersFailed()
        {
            this.tracker.OnAllPlayersFailed();
        }
    }
}
