using System;
using JetBrains.Annotations;
using UnityEngine;
using UdonSharp;

using VRC.SDKBase;
using VRC.SDK3.Video.Components;
using VRC.SDK3.Components.Video;

using DecentM.VideoRatelimit;
using DecentM.Collections;

namespace DecentM.Metrics
{
    public enum Metric
    {
        Heartbeat,
        Respawn,
        Instance,

        Trigger,
        Station,
        Interaction,
        Pickup,
        Custom,

        VideoPlayer,
        PlayerList,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MetricsSystem : UdonSharpBehaviour
    {
        public int requeueAttemptsLimit = 2;
        public int discardAttemptsLimit = 6;

        public Queue queue;
        public MetricsEvents events;
        public VideoRatelimitSystem ratelimit;

        [Space]
        public VRCUnityVideoPlayer player;

        // Queue item structure:
        // 0 - VRCUrl url
        // 1 - int attempts
        // 2 - Metric metric
        // private object[][] queue;

        public object[][] GetQueue()
        {
            return (object[][])this.queue.ToArray();
        }

        private void Start()
        {
            if (this.ratelimit == null)
            {
                Debug.LogError(
                    $"The ratelimit object isn't set on this Metrics System: {this.gameObject.name}. This object won't report any metrics."
                );
                this.enabled = false;
                return;
            }

            this.SendCustomEventDelayedSeconds(nameof(BroadcastInit), 1);
        }

        public void BroadcastInit()
        {
            this.events.OnMetricsSystemInit();
        }

        [PublicAPI]
        public void RecordMetric(VRCUrl url, Metric metric)
        {
            if (url == null)
            {
                Debug.LogWarning(
                    $"Something tried to queue a null URL by calling RecordMetric(null, {metric})"
                );
                return;
            }

            object[] queueItem = new object[] { url, 0, metric, };

            this.queue.Enqueue(queueItem);
            this.events.OnMetricQueued(metric);
        }

        private bool locked = false;

        private void FixedUpdate()
        {
            if (this.locked)
                return;

            if (this.queue == null || this.queue.Count == 0)
                return;

            if (this.queue.Peek() == null)
            {
                this.queue.Dequeue();
                return;
            }

            this.locked = true;
            this.ratelimit.RequestPlaybackWindow(this);
        }

        public void OnPlaybackWindow()
        {
            this.AttemptDelivery();
        }

        private void AttemptDelivery()
        {
            object[] currentItem = (object[])this.queue.Dequeue();

            if (currentItem == null)
                return;

            VRCUrl url = (VRCUrl)currentItem[0];
            int attempts = (int)currentItem[1];
            Metric metric = (Metric)currentItem[2];

            this.player.LoadURL(url);
            this.events.OnMetricDeliveryAttempt(metric, attempts);

            currentItem = new object[] { url, attempts + 1, metric };
            this.queue.Shift(currentItem);
        }

        public override void OnVideoEnd()
        {
            this.player.Stop();
        }

        public override void OnVideoLoop()
        {
            this.player.Stop();
        }

        public override void OnVideoPause()
        {
            this.player.Stop();
        }

        public override void OnVideoPlay()
        {
            this.player.Stop();
        }

        public override void OnVideoStart()
        {
            this.player.Stop();
        }

        public override void OnVideoReady()
        {
            this.player.Stop();
            this.HandleMetricSubmitted();
        }

        private void HandleMetricSubmitted()
        {
            object[] currentItem = (object[])this.queue.Dequeue();
            if (currentItem == null)
                return;

            this.locked = false;
            int attempts = (int)currentItem[1];
            Metric metric = (Metric)currentItem[2];

            this.events.OnMetricSubmitted(metric, attempts);
        }

        private void DiscardCurrentItem()
        {
            object[] currentItem = (object[])this.queue.Dequeue();
            if (currentItem == null)
                return;

            this.locked = false;
            int attempts = (int)currentItem[1];
            Metric metric = (Metric)currentItem[2];

            this.events.OnMetricDiscarded(metric, attempts);
        }

        private void RequeueCurrentItem()
        {
            object[] currentItem = (object[])this.queue.Dequeue();
            if (currentItem == null)
                return;

            this.locked = false;
            int attempts = (int)currentItem[1];
            Metric metric = (Metric)currentItem[2];

            this.queue.Enqueue(currentItem);
            this.events.OnMetricRequeued(metric, attempts);
        }

        private void RetryCurrentItem()
        {
            object[] currentItem = (object[])this.queue.Peek();
            if (currentItem == null)
                return;

            this.locked = false;
            int attempts = (int)currentItem[1];
            Metric metric = (Metric)currentItem[2];

            this.events.OnMetricDeliveryRetry(metric, attempts);
        }

        public override void OnVideoError(VideoError videoError)
        {
            this.player.Stop();

            object[] currentItem = (object[])this.queue.Peek();
            if (currentItem == null)
                return;

            this.locked = false;
            int attempts = (int)currentItem[1];

            // Delivery failed, need to retry

            if (attempts % this.requeueAttemptsLimit == 0)
            {
                this.RequeueCurrentItem();
                return;
            }
            if (attempts % this.discardAttemptsLimit == 0)
            {
                this.DiscardCurrentItem();
                return;
            }

            this.RetryCurrentItem();
        }
    }
}
