using UdonSharp;
using DecentM.Pubsub;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Components.Video;

namespace DecentM.Metrics.Plugins
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class MetricsPlugin : PubsubSubscriber
    {
        public MetricsSystem system;
        public MetricsEvents events;
        public URLStore urlStore;

        protected virtual void OnMetricsSystemInit() { }

        protected virtual void OnDebugLog(string message) { }

        protected virtual void OnMetricSubmitted(Metric metric, int attempts) { }

        protected virtual void OnMetricDiscarded(Metric metric, int attempts) { }

        protected virtual void OnMetricDeliveryAttempt(Metric metric, int attempts) { }

        protected virtual void OnMetricDeliveryRetry(Metric metric, int attempts) { }

        protected virtual void OnMetricQueued(Metric metric) { }

        protected virtual void OnMetricRequeued(Metric metric, int attempts) { }

        public sealed override void OnPubsubEvent(object name, object[] data)
        {
            switch (name)
            {
                #region Core

                case MetricsEvent.OnDebugLog:
                {
                    string message = (string)data[0];
                    this.OnDebugLog(message);
                    return;
                }

                case MetricsEvent.OnMetricsSystemInit:
                {
                    this.OnMetricsSystemInit();
                    return;
                }

                case MetricsEvent.OnMetricSubmitted:
                {
                    Metric metric = (Metric)data[0];
                    int attempts = (int)data[0];
                    this.OnMetricSubmitted(metric, attempts);
                    return;
                }

                case MetricsEvent.OnMetricDiscarded:
                {
                    Metric metric = (Metric)data[0];
                    int attempts = (int)data[0];
                    this.OnMetricDiscarded(metric, attempts);
                    return;
                }

                case MetricsEvent.OnMetricDeliveryAttempt:
                {
                    Metric metric = (Metric)data[0];
                    int attempts = (int)data[0];
                    this.OnMetricDeliveryAttempt(metric, attempts);
                    return;
                }

                case MetricsEvent.OnMetricDeliveryRetry:
                {
                    Metric metric = (Metric)data[0];
                    int attempts = (int)data[0];
                    this.OnMetricDeliveryRetry(metric, attempts);
                    return;
                }

                case MetricsEvent.OnMetricQueued:
                {
                    Metric metric = (Metric)data[0];
                    this.OnMetricQueued(metric);
                    return;
                }

                case MetricsEvent.OnMetricRequeued:
                {
                    Metric metric = (Metric)data[0];
                    int attempts = (int)data[0];
                    this.OnMetricRequeued(metric, attempts);
                    return;
                }

                    #endregion
            }
        }
    }
}
