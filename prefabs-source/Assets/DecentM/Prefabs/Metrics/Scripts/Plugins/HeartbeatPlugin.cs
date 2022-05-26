using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Metrics.Plugins
{
    public class HeartbeatPlugin : MetricsPlugin
    {
        public PerformanceGovernor performanceGovernor;
        public int reportingIntervalSeconds = 60;

        public float elapsed = 0;
        public bool initialised = false;

        private bool isVr = false;

        private void DoHeartbeat()
        {
            int fps = Mathf.FloorToInt(this.performanceGovernor.GetFps());
            VRCUrl url = this.urlStore.GetHeartbeatUrl(
                Networking.LocalPlayer.isMaster,
                this.isVr,
                fps
            );

            if (url == null)
                return;

            this.system.RecordMetric(url, Metric.Heartbeat);
        }

        protected override void OnMetricsSystemInit()
        {
            this.isVr =
                Networking.LocalPlayer == null ? false : Networking.LocalPlayer.IsUserInVR();

            this.DoHeartbeat();
            this.initialised = true;
        }

        private void FixedUpdate()
        {
            if (
                Networking.LocalPlayer == null
                || !Networking.LocalPlayer.IsValid()
                || !this.initialised
            )
                return;

            this.elapsed += Time.fixedUnscaledDeltaTime;
            if (this.elapsed <= this.reportingIntervalSeconds)
                return;
            this.elapsed = 0;

            this.DoHeartbeat();
        }
    }
}
