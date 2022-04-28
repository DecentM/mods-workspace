
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Metrics.Plugins
{
    public class PlayerCountPlugin : MetricsPlugin
    {
        public int reportingIntervalSeconds = 60;

        private float elapsed = 0;

        // Only the master runs this as we only want one report per instance
        private void FixedUpdate()
        {
            if (
                Networking.LocalPlayer == null
                || !Networking.LocalPlayer.IsValid()
                || !Networking.LocalPlayer.isMaster
            ) return;

            this.elapsed += Time.fixedUnscaledDeltaTime;
            if (this.elapsed <= this.reportingIntervalSeconds) return;
            this.elapsed = 0;

            // VRCUrl url = this.urlStore.GetMetricUrl(Metric.PlayerCount);
            // if (url == null) return;

            //this.system.RecordMetric(url, Metric.PlayerCount);
        }
    }
}
