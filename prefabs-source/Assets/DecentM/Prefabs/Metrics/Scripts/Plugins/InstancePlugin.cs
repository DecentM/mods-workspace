using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Metrics.Plugins
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class InstancePlugin : MetricsPlugin
    {
        public string[] instanceIds;

        [UdonSynced]
        private string instanceId;

        public int reportingIntervalSeconds = 60;

        private bool CheckLocalPlayer()
        {
            bool stop =
                Networking.LocalPlayer == null
                || !Networking.LocalPlayer.IsValid()
                || !Networking.LocalPlayer.isMaster;

            return !stop;
        }

        protected override void _Start()
        {
            if (!this.CheckLocalPlayer())
                return;

            this.instanceId = this.instanceIds[Random.Range(0, this.instanceIds.Length - 1)];
            this.RequestSerialization();
        }

        private void DoHeartbeat()
        {
            VRCUrl url = this.urlStore.GetInstanceUrl(
                this.instanceId,
                VRCPlayerApi.GetPlayerCount()
            );
            if (url == null)
                return;

            this.system.RecordMetric(url, Metric.Instance);
        }

        private bool initialised = false;

        protected override void OnMetricsSystemInit()
        {
            this.initialised = true;
            this.DoHeartbeat();
        }

        private float elapsed = 0;

        // Only the master runs this as we only want one report per instance
        private void FixedUpdate()
        {
            if (!this.CheckLocalPlayer() || !this.initialised)
                return;

            this.elapsed += Time.fixedUnscaledDeltaTime;
            if (this.elapsed <= this.reportingIntervalSeconds)
                return;
            this.elapsed = 0;

            this.DoHeartbeat();
        }
    }
}
