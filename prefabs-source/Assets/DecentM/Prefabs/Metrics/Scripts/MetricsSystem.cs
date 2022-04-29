using System;
using UnityEngine;
using UdonSharp;

using VRC.SDKBase;
using VRC.SDK3.Video.Components;
using VRC.SDK3.Components.Video;

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

            // Clone the first item from the queue
            object[] result = new object[] {  this.queue[0][0], this.queue[0][1], this.queue[0][2] };

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

        private object[] GetCurrentItem()
        {
            if (this.queue == null || this.queue.Length == 0) return null;
            return this.queue[0];
        }

        private void Start()
        {
            this.SendCustomEventDelayedSeconds(nameof(BroadcastInit), 10f);
        }

        public void BroadcastInit()
        {
            this.events.OnMetricsSystemInit();
            this.locked = false;
        }

        private bool locked = true;
        private float elapsed = 0;
        public float queueProcessIntervalMin = 5.2f;
        public float queueProcessIntervalMax = 8.5f;
        private float queueProcessInterval = 10f;

        private void FixedUpdate()
        {
            if (this.locked) return;

            this.elapsed += Time.fixedUnscaledDeltaTime;
            if (this.elapsed > this.queueProcessInterval)
            {
                this.elapsed = 0;
                this.queueProcessInterval = UnityEngine.Random.Range(this.queueProcessIntervalMin, this.queueProcessIntervalMax);
                this.ProcessQueue();
            }
        }

        private void ProcessQueue()
        {
            if (this.queue == null || this.queue.Length == 0) return;

            this.AttemptDelivery();
        }

        private void AttemptDelivery()
        {
            object[] currentItem = this.GetCurrentItem();
            if (currentItem == null) return;

            this.locked = true;
            VRCUrl url = (VRCUrl)currentItem[0];
            int attempts = (int)currentItem[1];
            Metric metric = (Metric)currentItem[2];

            this.player.LoadURL(url);
            this.events.OnMetricDeliveryAttempt(metric, attempts);

            currentItem = new object[] { url, attempts + 1, metric };
            this.queue[0] = currentItem;
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
            object[] currentItem = this.QueuePop();
            if (currentItem == null) return;

            this.locked = false;
            int attempts = (int)currentItem[1];
            Metric metric = (Metric)currentItem[2];

            this.events.OnMetricSubmitted(metric, attempts);
        }

        private void DiscardCurrentItem()
        {
            object[] currentItem = this.QueuePop();
            if (currentItem == null) return;

            this.locked = false;
            int attempts = (int)currentItem[1];
            Metric metric = (Metric)currentItem[2];

            this.events.OnMetricDiscarded(metric, attempts);
        }

        private void RequeueCurrentItem()
        {
            object[] currentItem = this.QueuePop();
            if (currentItem == null) return;

            this.locked = false;
            int attempts = (int)currentItem[1];
            Metric metric = (Metric)currentItem[2];

            this.QueuePush(currentItem);
            this.events.OnMetricRequeued(metric, attempts);
        }

        private void RetryCurrentItem()
        {
            object[] currentItem = this.GetCurrentItem();
            if (currentItem == null) return;

            this.locked = false;
            int attempts = (int)currentItem[1];
            Metric metric = (Metric)currentItem[2];

            this.events.OnMetricDeliveryRetry(metric, attempts);
        }

        public override void OnVideoError(VideoError videoError)
        {
            this.player.Stop();

            object[] currentItem = this.GetCurrentItem();
            if (currentItem == null) return;

            this.locked = false;
            int attempts = (int)currentItem[1];

            // Delivery failed, need to retry

            if (attempts % this.requeueAttemptsLimit == 0) { this.RequeueCurrentItem(); return; }
            if (attempts % this.discardAttemptsLimit == 0) { this.DiscardCurrentItem(); return; }

            this.RetryCurrentItem();
        }
    }
}
