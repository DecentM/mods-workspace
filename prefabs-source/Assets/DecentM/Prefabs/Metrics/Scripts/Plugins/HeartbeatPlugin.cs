using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Metrics.Plugins
{
    public class HeartbeatPlugin : MetricsPlugin
    {
        public int reportingIntervalSeconds = 60;

        public float elapsed = 0;
        public bool initialised = false;

        private bool isVr = false;
        private int timezone = 0;

        protected override void _Start()
        {
            TimeZoneInfo tz = TimeZoneInfo.Local;
            TimeSpan offset = tz.BaseUtcOffset;
            this.timezone = offset.Hours;

            this.isVr = Networking.LocalPlayer.IsUserInVR();
        }

        private void DoHeartbeat()
        {
            VRCUrl url = this.urlStore.GetHeartbeatUrl(Networking.LocalPlayer.isMaster, this.isVr, this.timezone);
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
