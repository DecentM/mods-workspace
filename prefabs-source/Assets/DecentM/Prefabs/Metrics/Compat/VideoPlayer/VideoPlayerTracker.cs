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
        Retry,
        RetryAbort,
    }

    public class VideoPlayerTracker : IndividualTrackingPlugin
    {
        private void TrackEvent(string eventName)
        {
            VRCUrl url = this.urlStore.GetVideoPlayerUrl(this.metricName, eventName);

            if (url == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"Didn't find a URL for {this.metricName} {eventName}"
                );
                return;
            }

            this.system.RecordMetric(url, Metric.VideoPlayer);
        }

        public void OnPlay()
        {
            this.TrackEvent(nameof(TrackedVideoPlayerEvent.Play));
        }

        public void OnStop()
        {
            this.TrackEvent(nameof(TrackedVideoPlayerEvent.Stop));
        }

        public void OnVodLoaded()
        {
            this.TrackEvent(nameof(TrackedVideoPlayerEvent.VodLoaded));
        }

        public void OnLiveLoaded()
        {
            this.TrackEvent(nameof(TrackedVideoPlayerEvent.LiveLoaded));
        }

        public void OnAllPlayersFailed()
        {
            this.TrackEvent(nameof(TrackedVideoPlayerEvent.AllPlayersFailed));
        }

        public void OnAutoRetry()
        {
            this.TrackEvent(nameof(TrackedVideoPlayerEvent.Retry));
        }

        public void OnAutoRetryAbort()
        {
            this.TrackEvent(nameof(TrackedVideoPlayerEvent.RetryAbort));
        }
    }
}
