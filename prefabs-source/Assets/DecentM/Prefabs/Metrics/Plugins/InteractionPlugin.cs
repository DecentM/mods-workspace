using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Metrics.Plugins
{
    public class InteractionPlugin : MetricsPlugin
    {
        public string metricName = "";

        private void DoReport()
        {
            VRCUrl url = this.urlStore.GetInteractionUrl(this.metricName);
            if (url == null) return;

            this.system.RecordMetric(url, Metric.Interaction);
        }

        public override void Interact()
        {
            this.DoReport();
        }
    }
}
