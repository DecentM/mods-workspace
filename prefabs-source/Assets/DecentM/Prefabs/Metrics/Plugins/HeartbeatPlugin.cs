
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Metrics.Plugins
{
    public enum VrPlatform
    {
        Other,
        Index,
        Vive,
        QuestLink,
        QuestStandalone,
    }

    public class HeartbeatPlugin : MetricsPlugin
    {
        public int reportingIntervalSeconds = 60;

        private float elapsed = 0;
        private bool initialised = false;

        private void DoHeartbeat()
        {
            VRCUrl url = this.urlStore.GetHeartbeatUrl(Networking.LocalPlayer.isMaster, Networking.LocalPlayer.IsUserInVR(), false, 1, "index");
            if (url == null) return;

            this.system.RecordMetric(url, Metric.Heartbeat);
        }

        protected override void OnMetricsSystemInit()
        {
            this.DoHeartbeat();
            this.initialised = true;
        }

        private void FixedUpdate()
        {
            if (Networking.LocalPlayer == null || !Networking.LocalPlayer.IsValid() || !this.initialised) return;

            this.elapsed += Time.fixedUnscaledDeltaTime;
            if (this.elapsed <= this.reportingIntervalSeconds) return;
            this.elapsed = 0;

            this.DoHeartbeat();
        }
    }
}
