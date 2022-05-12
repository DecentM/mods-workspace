using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace DecentM.Metrics.Plugins
{
    public class DebugPlugin : MetricsPlugin
    {
        public TextMeshProUGUI logGui;
        public TextMeshProUGUI queueGui;

        string[] logs = new string[25];

        public void RenderQueue()
        {
            object[][] queue = this.system.GetQueue();
            if (queue == null)
                return;

            string result = "";

            for (int i = 0; i < queue.Length; i++)
            {
                object[] queueItem = queue[i];
                if (queueItem == null)
                    continue;

                VRCUrl url = (VRCUrl)queueItem[0];
                int attempts = (int)queueItem[1];
                Metric metric = (Metric)queueItem[2];

                result +=
                    $"{url.ToString()}, attempts: {attempts.ToString()}, metric: {metric.ToString()}\n";
            }

            this.queueGui.text = result;
        }

        public void Log(params string[] messages)
        {
            string[] tmp = new string[logs.Length];
            Array.Copy(logs, 1, tmp, 0, this.logs.Length - 1);
            tmp[tmp.Length - 1] = String.Join(" ", messages);
            this.logs = tmp;

            this.logGui.text = String.Join("\n", this.logs);
            this.RenderQueue();
        }

        protected override void _Start()
        {
            this.Log(nameof(_Start));
        }

        protected override void OnDebugLog(string message)
        {
            this.Log(message);
        }

        protected override void OnMetricDeliveryAttempt(Metric metric, int attempts)
        {
            this.Log(nameof(OnMetricDeliveryAttempt), attempts.ToString());
        }

        protected override void OnMetricDeliveryRetry(Metric metric, int attempts)
        {
            this.Log(nameof(OnMetricDeliveryRetry), attempts.ToString());
        }

        protected override void OnMetricDiscarded(Metric metric, int attempts)
        {
            this.Log(nameof(OnMetricDiscarded), attempts.ToString());
        }

        protected override void OnMetricQueued(Metric metric)
        {
            this.Log(nameof(OnMetricQueued), metric.ToString());
        }

        protected override void OnMetricRequeued(Metric metric, int attempts)
        {
            this.Log(nameof(OnMetricRequeued), attempts.ToString());
        }

        protected override void OnMetricsSystemInit()
        {
            this.Log(nameof(OnMetricsSystemInit));
        }

        protected override void OnMetricSubmitted(Metric metric, int attempts)
        {
            this.Log(nameof(OnMetricSubmitted));
        }
    }
}
