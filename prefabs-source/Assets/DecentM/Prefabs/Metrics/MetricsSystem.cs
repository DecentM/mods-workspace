using System;
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Video.Components;
using VRC.SDK3.Components.Video;

namespace DecentM.Metrics
{
    public enum Metric
    {
        Heartbeat,
        Respawn,
        PlayerCount,

        TriggerEntered,
        TriggerExited,
        StationEntered,
        StationExited,

        Interaction,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MetricsSystem : UdonSharpBehaviour
    {
        public int requeueAttemptsLimit = 2;
        public int discardAttemptsLimit = 6;

        public MetricsEvents events;

        [Space]
        public VRCUnityVideoPlayer player;

        // Queue item structure:
        // 0 - VRCUrl url
        // 1 - int attempts
        // 2 - Metric metric
        private object[][] queue;

        public object[][] GetQueue()
        {
            return this.queue;
        }

        public void RecordMetric(VRCUrl url, Metric metric)
        {
            object[] queueItem = new object[] { url, 0, metric, };

            this.QueuePush(queueItem);
            this.events.OnMetricQueued(metric);
        }

        private void QueuePush(object[] queueItem)
        {
            if (this.queue == null) this.queue = new object[0][];

            object[][] tmp = new object[this.queue.Length + 1][];
            Array.Copy(this.queue, 0, tmp, 0, this.queue.Length);
            tmp[tmp.Length - 1] = queueItem;
            this.queue = tmp;
        }

        private object[] QueuePop()
        {
            if (this.queue == null || this.queue.Length == 0) return null;

            object[] result = this.queue[0];

            object[][] tmp = new object[this.queue.Length - 1][];
            Array.Copy(this.queue, 1, tmp, 0, this.queue.Length - 1);
            this.queue = tmp;

            return result;
        }

        private void QueueShift(object[] queueItem)
        {
            if (this.queue == null) this.queue = new object[0][];

            object[][] tmp = new object[this.queue.Length + 1][];
            Array.Copy(this.queue, 0, tmp, 1, this.queue.Length);
            tmp[0] = queueItem;
            this.queue = tmp;
        }

        private void Start()
        {
            this.SendCustomEventDelayedSeconds(nameof(BroadcastInit), 5.2f);
        }

        public void BroadcastInit()
        {
            this.events.OnMetricsSystemInit();
        }

        private object[] currentItem;

        private float elapsed = 0;

        private void FixedUpdate()
        {
            this.elapsed += Time.fixedUnscaledDeltaTime;
            if (this.elapsed > 5.2f)
            {
                this.elapsed = 0;
                this.ProcessQueue();
            }
        }

        private void ProcessQueue()
        {
            if (this.currentItem != null) return;

            this.currentItem = this.QueuePop();
            if (this.currentItem == null) return;

            this.AttemptDelivery();
        }

        private void AttemptDelivery()
        {
            if (this.currentItem == null) return;

            VRCUrl url = (VRCUrl)this.currentItem[0];
            int attempts = (int)this.currentItem[1];
            Metric metric = (Metric)this.currentItem[2];

            this.player.LoadURL(url);
            this.events.OnMetricDeliveryAttempt(metric, attempts);

            this.currentItem = new object[] { url, attempts + 1, metric };
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
            this.HandleMetricSubmitted();
        }

        public override void OnVideoStart()
        {
            this.player.Stop();
            this.HandleMetricSubmitted();
        }

        public override void OnVideoReady()
        {
            this.player.Stop();
            this.HandleMetricSubmitted();
        }

        private void HandleMetricSubmitted()
        {
            if (this.currentItem == null) return;

            int attempts = (int)this.currentItem[1];
            Metric metric = (Metric)this.currentItem[2];

            this.events.OnMetricSubmitted(metric, attempts);
            this.currentItem = null;
        }

        private void DiscardCurrentItem()
        {
            if (this.currentItem == null) return;

            int attempts = (int)this.currentItem[1];
            Metric metric = (Metric)this.currentItem[2];

            this.events.OnMetricDiscarded(metric, attempts);

            this.currentItem = null;
        }

        private void RequeueCurrentItem()
        {
            if (this.currentItem == null) return;

            int attempts = (int)this.currentItem[1];
            Metric metric = (Metric)this.currentItem[2];

            this.events.OnMetricRequeued(metric, attempts);

            this.QueuePush(this.currentItem);
            this.currentItem = null;
        }

        private void RetryCurrentItem()
        {
            if (this.currentItem == null) return;

            int attempts = (int)this.currentItem[1];
            Metric metric = (Metric)this.currentItem[2];

            this.events.OnMetricDeliveryRetry(metric, attempts);
        }

        public override void OnVideoError(VideoError videoError)
        {
            this.player.Stop();

            if (this.currentItem == null)
            {
                Debug.LogWarning("I somehow lost the current item");
                return;
            }

            VRCUrl url = (VRCUrl)this.currentItem[0];
            int attempts = (int)this.currentItem[1];
            Metric metric = (Metric)this.currentItem[2];

            // Delivery failed, need to retry

            if (attempts % this.requeueAttemptsLimit == 0) { this.RequeueCurrentItem(); return; }
            if (attempts % this.discardAttemptsLimit == 0) { this.DiscardCurrentItem(); return; }

            this.RetryCurrentItem();
        }
    }
}
