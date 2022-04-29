using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Metrics.Plugins
{
    public class InteractionsPlugin : MetricsPlugin
    {
        public string metricName = "";

        private bool locked = false;

        private void DoReport()
        {
            VRCUrl url = this.urlStore.GetInteractionUrl(this.metricName);
            if (url == null) return;

            this.locked = true;
            this.system.RecordMetric(url, Metric.Interaction);
            this.SendCustomEventDelayedSeconds(nameof(Unlock), 5.2f);
        }

        public void Unlock()
        {
            this.locked = false;
        }

        public override void Interact()
        {
            // Some rate limiting so players cannot jam the queue by spamming the interaction
            if (this.locked) return;

            this.DoReport();
        }
    }
}
