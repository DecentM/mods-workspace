
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using DecentM.Pubsub;

namespace DecentM.Metrics
{
    enum MetricsEvent
    {
        OnMetricsSystemInit,
        OnDebugLog,

        OnMetricSubmitted,
        OnMetricDiscarded,
        OnMetricRequeued,
        OnMetricDeliveryRetry,
        OnMetricDeliveryAttempt,
        OnMetricQueued,
    }

    public class MetricsEvents : PubsubHost
    {
        public void OnMetricsSystemInit()
        {
            this.BroadcastEvent(MetricsEvent.OnMetricsSystemInit);
        }

        public void OnDebugLog(string message)
        {
            this.BroadcastEvent(MetricsEvent.OnDebugLog, message);
        }

        public void OnMetricSubmitted(Metric metric, int attempts)
        {
            this.BroadcastEvent(MetricsEvent.OnMetricSubmitted, metric, attempts);
        }

        public void OnMetricDiscarded(Metric metric, int attempts)
        {
            this.BroadcastEvent(MetricsEvent.OnMetricDiscarded, metric, attempts);
        }

        public void OnMetricDeliveryAttempt(Metric metric, int attempts)
        {
            this.BroadcastEvent(MetricsEvent.OnMetricDeliveryAttempt, metric, attempts);
        }

        public void OnMetricDeliveryRetry(Metric metric, int attempts)
        {
            this.BroadcastEvent(MetricsEvent.OnMetricDeliveryAttempt, metric, attempts);
        }

        public void OnMetricQueued(Metric metric)
        {
            this.BroadcastEvent(MetricsEvent.OnMetricQueued, metric);
        }

        public void OnMetricRequeued(Metric metric, int attempts)
        {
            this.BroadcastEvent(MetricsEvent.OnMetricQueued, metric, attempts);
        }
    }
}
