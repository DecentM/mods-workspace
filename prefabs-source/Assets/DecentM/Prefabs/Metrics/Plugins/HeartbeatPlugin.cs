
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Metrics.Plugins
{
    public class HeartbeatPlugin : MetricsPlugin
    {
        public int reportingIntervalSeconds = 60;

        private float elapsed = 0;

        private void DoHeartbeat()
        {
            VRCUrl url = this.urlStore.GetMetricUrl(Metric.Heartbeat);
            if (url == null) return;

            this.system.RecordMetric(url, Metric.Heartbeat);
        }

        protected override void OnMetricsSystemInit()
        {
            this.DoHeartbeat();
        }

        private void FixedUpdate()
        {
            if (Networking.LocalPlayer == null || !Networking.LocalPlayer.IsValid()) return;

            this.elapsed += Time.fixedUnscaledDeltaTime;
            if (this.elapsed <= this.reportingIntervalSeconds) return;
            this.elapsed = 0;

            this.DoHeartbeat();
        }
    }
}
