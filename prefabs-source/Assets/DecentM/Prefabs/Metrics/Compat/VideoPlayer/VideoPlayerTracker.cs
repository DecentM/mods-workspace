using System;
using JetBrains.Annotations;
using VRC.SDKBase;

using DecentM.VideoPlayer.Plugins;

namespace DecentM.Metrics.Plugins
{
    public enum TrackedVideoPlayerEvent
    {
        Play,
        Stop,
        VodLoaded,
        LiveLoaded,
        AllPlayersFailed,
    }

    public class VideoPlayerTracker : IndividualTrackingPlugin
    {
        private void TrackEnum(TrackedVideoPlayerEvent enumType)
        {
            VRCUrl url = this.urlStore.GetVideoPlayerUrl(nameof(enumType));

            this.system.RecordMetric(url, Metric.VideoPlayer);
        }

        public void OnPlay()
        {
            this.TrackEnum(TrackedVideoPlayerEvent.Play);
        }

        public void OnStop()
        {
            this.TrackEnum(TrackedVideoPlayerEvent.Stop);
        }

        public void OnVodLoaded()
        {
            this.TrackEnum(TrackedVideoPlayerEvent.VodLoaded);
        }

        public void OnLiveLoaded()
        {
            this.TrackEnum(TrackedVideoPlayerEvent.LiveLoaded);
        }

        public void OnAllPlayersFailed()
        {
            this.TrackEnum(TrackedVideoPlayerEvent.AllPlayersFailed);
        }
    }
}
